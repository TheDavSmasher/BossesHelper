﻿using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;
using static Celeste.Mod.BossesHelper.Code.Helpers.UserFileReader;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
	public partial class BossPuppet : Actor
	{
		#region Enums
		public enum ColliderOption
		{
			Hitboxes,
			Hurtboxes,
			Bouncebox,
			Target
		}

		public enum HurtModes
		{
			PlayerContact,
			PlayerDash,
			HeadBonk,
			SidekickAttack,
			Custom
		}
		#endregion

		public readonly Sprite Sprite;

		private readonly EnumDict<ColliderOption, Dictionary<string, Collider>> hitboxMetadata;

		private readonly Component BossCollision;

		private readonly bool DynamicFacing;

		private readonly bool MirrorSprite;

		private readonly bool freezeSidekickOnAttack;

		private readonly float sidekickCooldown;

		public readonly HurtModes HurtMode;

		private BossFunctions BossFunctions;

		public float BossHitCooldown { get; private set; }

		private readonly float bossHitCooldownBase;

		public int Facing;

		public const float Gravity = 900f;

		public Vector2 Speed;

		public float groundFriction;

		public float airFriction;

		private readonly Dictionary<string, object> storedObjects = [];

		public bool Grounded => Speed.Y >= 0 && OnGround();

		public Collider Hurtbox { get; private set; }

		public Collider Bouncebox { get; private set; }

		public Collider Target { get; private set; }

		public bool SolidCollidable;

		private readonly float maxFall;

		private float effectiveGravity;

		public bool killOnContact;

		public BossPuppet(BossController controller)
			: base(controller.Position)
		{
			EntityData data = controller.SourceData;
			DynamicFacing = data.Bool("dynamicFacing");
			MirrorSprite = data.Bool("mirrorSprite");
			bossHitCooldownBase = data.Float("bossHitCooldown", 0.5f);
			maxFall = data.Float("maxFall", 90f);
			effectiveGravity = data.Float("baseGravityMultiplier", 1f) * Gravity;
			groundFriction = data.Float("groundFriction");
			airFriction = data.Float("airFriction");
			freezeSidekickOnAttack = data.Bool("sidekickFreeze");
			sidekickCooldown = data.Float("sidekickCooldown");
			SolidCollidable = data.Bool("startSolidCollidable");
			Collidable = data.Bool("startCollidable");
			HurtMode = data.Enum("hurtMode", HurtModes.PlayerContact);
			killOnContact = data.Bool("killOnContact");
			Add(new PlayerCollider(KillOnContact));
			Facing = 1;
			if (GFX.SpriteBank.TryCreate(data.Attr("bossSprite"), out Sprite sprite))
			{
				Add(Sprite = sprite);
				Sprite.Scale = Vector2.One;
				PlayBossAnim(data.String("startingAnim", "idle"));
			}
			else
			{
				Sprite = sprite;
			}
			hitboxMetadata = ReadMetadataFile(data.Attr("hitboxMetadataPath"));
			Collider = GetMainOrDefault(ColliderOption.Hitboxes, Sprite.Height);
			Hurtbox = GetMainOrDefault(ColliderOption.Hurtboxes, Sprite.Height);
			(BossCollision = HurtMode switch
			{
				HurtModes.HeadBonk => new PlayerCollider(OnPlayerBounce,
						Bouncebox = GetMainOrDefault(ColliderOption.Bouncebox, 6f)),
				HurtModes.SidekickAttack => new SidekickTarget(OnSidekickLaser, data.Attr("bossID"),
						Target = GetMainOrDefault(ColliderOption.Target, null)),
				HurtModes.PlayerDash => new PlayerCollider(OnPlayerDash, Hurtbox),
				HurtModes.PlayerContact => new PlayerCollider(OnPlayerContact, Hurtbox),
				_ => null //Custom depends on Setup.lua's code, does nothing by default
			})?.AddTo(this);
		}

		internal void LoadFunctions(BossController controller)
		{
			BossFunctions = ReadLuaFilePath(controller.SourceData.Attr("functionsPath"), path => new BossFunctions(path, controller));
		}

		private Collider GetMainOrDefault(ColliderOption option, float? value)
		{
			return GetTagOrDefault(option, "main", value);
		}

		private Collider GetTagOrDefault(ColliderOption option, string key, float? value)
		{
			var dictionary = hitboxMetadata[option];
			if (dictionary.TryGetValue(key, out var result))
				return result;

			if (value == null)
				return new Circle(4f);
			return new Hitbox(Sprite.Width, (float)value, Sprite.Width * -0.5f, Sprite.Height * -0.5f);
		}

		#region Collision Methods
		private void KillOnContact(Player player)
		{
			if (killOnContact)
				player.Die((player.Position - Position).SafeNormalize());
		}

		private void OnSidekickLaser()
		{
			OnDamage(HurtModes.SidekickAttack);
		}

		private void OnPlayerBounce(Player player)
		{
			OnDamage(HurtModes.HeadBonk, postLua: () =>
			{
				Audio.Play("event:/game/general/thing_booped", Position);
				Celeste.Freeze(0.2f);
				player.Bounce(Top + 2f);
			});
		}

		private void OnPlayerDash(Player player)
		{
			OnDamage(HurtModes.PlayerDash, () => player.DashAttacking && player.Speed != Vector2.Zero);
		}

		private void OnPlayerContact(Player _)
		{
			OnDamage(HurtModes.PlayerContact);
		}

		private void OnDamage(HurtModes source, Func<bool> predicate = null, Action postLua = null)
		{
			if (BossHitCooldown <= 0 && (predicate?.Invoke() ?? true))
			{
				ResetBossHitCooldown();
				BossFunctions.OnDamage(source).Coroutine(this);
				postLua?.Invoke();
			}
		}

		public void OnCollideH(CollisionData data)
		{
			if (data.Hit != null && data.Hit.OnCollide != null)
			{
				data.Hit.OnCollide(data.Direction);
			}
			Speed.X = 0;
		}

		public void OnCollideV(CollisionData data)
		{
			if (data.Hit != null && data.Hit.OnCollide != null)
			{
				data.Hit.OnCollide(data.Direction);
			}
			Speed.Y = 0;
		}
		#endregion

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			Player player = scene.GetPlayer();
			if (HurtMode == HurtModes.SidekickAttack && scene.Tracker.GetEntity<BadelineSidekick>() == null)
			{
				(scene as Level).Add(new BadelineSidekick(player.Position + new Vector2(-16f * (int)player.Facing, -4f), freezeSidekickOnAttack, sidekickCooldown));
			}
		}

		public override void Update()
		{
			Facing = Facing != 0 ? Facing / Math.Abs(Facing) : 1;
			if (Scene.GetPlayer() is Player entity && DynamicPositionOver_Quarter(entity.X))
			{
				Facing *= -1;
			}
			if (BossHitCooldown > 0)
			{
				BossHitCooldown -= Engine.DeltaTime;
			}
			base.Update();
			//Move based on speed
			if (SolidCollidable)
			{
				MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
				MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
			}
			else
			{
				NaiveMove(Speed * Engine.DeltaTime);
			}
			//Apply gravity
			if (!Grounded)
			{
				Speed.Y = Calc.Approach(Speed.Y, maxFall, effectiveGravity * Engine.DeltaTime);
			}
			//Apply friction
			Speed.X = Calc.Approach(Speed.X, 0f, (Grounded ? groundFriction : airFriction) * Engine.DeltaTime);
			//Return Sprite Scale
			if (Sprite != null)
			{
				Sprite.Scale.X = Calc.Approach(Sprite.Scale.X, 1f, 1.75f * Engine.DeltaTime);
				Sprite.Scale.Y = Calc.Approach(Sprite.Scale.Y, 1f, 1.75f * Engine.DeltaTime);
			}
		}

		private bool DynamicPositionOver_Quarter(float pos)
		{
			if (!DynamicFacing)
				return false;
			if (Facing == -1)
				return pos < X - Collider.Width / 4;
			return pos > X + Collider.Width / 4;
		}

		public override void Render()
		{
			base.Render();
			int realFacing = Facing * (MirrorSprite ? -1 : 1);
			Sprite?.Scale.X = realFacing;
		}

		public void PlayBossAnim(string name)
		{
			if (Sprite != null && !Sprite.TryPlay(name))
			{
				Logger.Log(LogLevel.Warn, "BossesHelper/BossPuppet", "Animation specified does not exist!");
			}
		}

		public void ResetBossHitCooldown()
		{
			BossHitCooldown = bossHitCooldownBase;
		}
	}
}
