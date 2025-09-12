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
	public abstract partial class BossPuppet : Actor
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

		protected readonly Component BossCollision;

		public abstract HurtModes HurtMode { get; }

		private readonly bool DynamicFacing;

		private readonly bool MirrorSprite;

		internal BossFunctions BossFunctions;

		public readonly Stopwatch BossDamageCooldown;

		public int Facing;

		public const float Gravity = 900f;

		public float gravityMult;

		public Vector2 Speed;

		public float groundFriction;

		public float airFriction;

		public bool Grounded => Speed.Y >= 0 && OnGround();

		public Collider Hurtbox { get; private set; }

		public bool SolidCollidable;

		private readonly float maxFall;

		public bool killOnContact;

		public BossPuppet(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			DynamicFacing = data.Bool("dynamicFacing");
			MirrorSprite = data.Bool("mirrorSprite");
			Add(BossDamageCooldown = new(data.Float("bossHitCooldown", 0.5f)));
			maxFall = data.Float("maxFall", 90f);
			gravityMult = data.Float("baseGravityMultiplier", 1f);
			groundFriction = data.Float("groundFriction");
			airFriction = data.Float("airFriction");
			SolidCollidable = data.Bool("startSolidCollidable");
			Collidable = data.Bool("startCollidable");
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
			(BossCollision = GetBossCollision())?.AddTo(this);
		}

		protected abstract Component GetBossCollision();

		protected Collider GetMainOrDefault(ColliderOption option, float? value)
			=> GetTagOrDefault(option, "main", value);

		protected Collider GetTagOrDefault(ColliderOption option, string key, float? value)
		{
			if (hitboxMetadata[option].TryGetValue(key, out var result))
				return result;

			if (value == null)
				return new Circle(4f);
			return new Hitbox(Sprite.Width, (float)value, Sprite.Width * -0.5f, Sprite.Height * -0.5f);
		}

		public override void Update()
		{
			Facing = Facing != 0 ? Facing / Math.Abs(Facing) : 1;
			if (Scene.GetPlayer() is Player entity && DynamicPositionOver_Quarter(entity.X))
			{
				Facing *= -1;
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
				Speed.Y = Calc.Approach(Speed.Y, maxFall, Gravity * gravityMult * Engine.DeltaTime);
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

		public void PlayBossAnim(string anim)
			=> Sprite.PlayOrWarn(anim);

		#region Collision Methods
		private void KillOnContact(Player player)
		{
			if (killOnContact)
				player.Die((player.Position - Position).SafeNormalize());
		}

		protected void OnDamage(Func<bool> predicate = null, Action postLua = null)
		{
			if (BossDamageCooldown.Finished && (predicate?.Invoke() ?? true))
			{
				BossDamageCooldown.Reset();
				BossFunctions.OnDamage(HurtMode).Coroutine(this);
				postLua?.Invoke();
			}
		}

		private void OnCollideH(CollisionData data)
		{
			if (data.Hit != null && data.Hit.OnCollide != null)
			{
				data.Hit.OnCollide(data.Direction);
			}
			Speed.X = 0;
		}

		private void OnCollideV(CollisionData data)
		{
			if (data.Hit != null && data.Hit.OnCollide != null)
			{
				data.Hit.OnCollide(data.Direction);
			}
			Speed.Y = 0;
		}
		#endregion
	}

	public class ContactBossPuppet(EntityData data, Vector2 offset) : BossPuppet(data, offset)
	{
		public override HurtModes HurtMode => HurtModes.PlayerContact;

		protected override Component GetBossCollision()
			=> new PlayerCollider(_ => OnDamage(), Hurtbox);
	}

	public class DashBossPuppet(EntityData data, Vector2 offset) : BossPuppet(data, offset)
	{
		public override HurtModes HurtMode => HurtModes.PlayerDash;

		protected override Component GetBossCollision()
			=> new PlayerCollider(OnPlayerDash, Hurtbox);

		private void OnPlayerDash(Player player)
		{
			OnDamage(() => player.DashAttacking && player.Speed != Vector2.Zero);
		}
	}

	public partial class BounceBossPuppet(EntityData data, Vector2 offset) : BossPuppet(data, offset)
	{
		public override HurtModes HurtMode => HurtModes.HeadBonk;

		public Collider Bouncebox { get; private set; }

		protected override Component GetBossCollision()
			=> new PlayerCollider(OnPlayerBounce,
				Bouncebox = GetMainOrDefault(ColliderOption.Bouncebox, 6f));

		private void OnPlayerBounce(Player player)
		{
			OnDamage(postLua: () =>
			{
				Audio.Play("event:/game/general/thing_booped", Position);
				Celeste.Freeze(0.2f);
				player.Bounce(Top + 2f);
			});
		}
	}

	public partial class SidekickBossPuppet(EntityData data, Vector2 offset) : BossPuppet(data, offset)
	{
		public override HurtModes HurtMode => HurtModes.SidekickAttack;

		private readonly string BossID = data.Attr("bossID");

		private readonly bool freezeSidekickOnAttack = data.Bool("sidekickFreeze");

		private readonly float sidekickCooldown = data.Float("sidekickCooldown");

		public Collider Target { get; private set; }

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			Player player = scene.GetPlayer();
			if (scene.Tracker.GetEntity<BadelineSidekick>() == null)
			{
				(scene as Level).Add(new BadelineSidekick(player.Position + new Vector2(-16f * (int)player.Facing, -4f), freezeSidekickOnAttack, sidekickCooldown));
			}
		}

		protected override Component GetBossCollision()
			=> new SidekickTarget(() => OnDamage(), BossID,
				Target = GetMainOrDefault(ColliderOption.Target, null));
	}

	public class CustomBossPuppet(EntityData data, Vector2 offset) : BossPuppet(data, offset)
	{
		public override HurtModes HurtMode => HurtModes.Custom;

		protected override Component GetBossCollision()
			=> null;
	}
}
