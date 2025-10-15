using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;
using static Celeste.Mod.BossesHelper.Code.Helpers.UserFileReader;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
	public abstract partial class BossPuppet : BossActor
	{
		#region Enums
		public enum ColliderOption
		{
			Hitboxes,
			Hurtboxes,
			SolidColliders,
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

		private readonly EnumDict<ColliderOption, Dictionary<string, Collider>> hitboxMetadata;

		public Collider Hurtbox;

		public Collider SolidCollider;

		protected readonly Component BossCollision;

		public abstract HurtModes HurtMode { get; }

		private readonly bool DynamicFacing;

		private readonly bool MirrorSprite;

		internal BossFunctions BossFunctions;

		public readonly Stopwatch BossDamageCooldown;

		public int Facing;

		public float groundFriction;

		public float airFriction;

		public bool killOnContact;

		protected BossPuppet(EntityData data, Vector2 offset)
			: base(data.Position + offset, data.Attr("bossSprite"), Vector2.One, data.Float("maxFall", 90f),
				  data.Bool("startCollidable"), data.Bool("startSolidCollidable"), data.Float("baseGravityMultiplier", 1f))
		{
			Facing = 1;
			DynamicFacing = data.Bool("dynamicFacing");
			MirrorSprite = data.Bool("mirrorSprite");
			GravityMult = data.Float("baseGravityMultiplier", 1f);
			groundFriction = data.Float("groundFriction");
			airFriction = data.Float("airFriction");
			killOnContact = data.Bool("killOnContact");

			hitboxMetadata = ReadMetadataFile(data.Attr("hitboxMetadataPath"));
			SolidCollider = GetCollider(ColliderOption.SolidColliders);
			Collider = GetCollider(ColliderOption.Hitboxes);
			Hurtbox = GetCollider(HurtMode switch
			{
				HurtModes.HeadBonk => ColliderOption.Bouncebox,
				HurtModes.SidekickAttack => ColliderOption.Target,
				_ => ColliderOption.Hurtboxes
			});
			if ((BossCollision = GetBossCollision()) != null)
				Add(BossCollision);

			Add(BossDamageCooldown = new(data.Float("bossHitCooldown", 0.5f)));
			Add(new PlayerCollider(KillOnContact, KillCollider));
			PlayAnim(data.String("startingAnim", "idle"));
		}

		protected abstract Component GetBossCollision();

		internal static BossPuppet Create(HurtModes hurtMode, EntityData data, Vector2 offset)
		{
			return hurtMode switch
			{
				HurtModes.PlayerContact => new ContactBossPuppet(data, offset),
				HurtModes.PlayerDash => new DashBossPuppet(data, offset),
				HurtModes.HeadBonk => new BounceBossPuppet(data, offset),
				HurtModes.SidekickAttack => new SidekickBossPuppet(data, offset),
				_ => new CustomBossPuppet(data, offset)
			};
		}

		public override void Update()
		{
			Facing = Facing != 0 ? Facing / Math.Abs(Facing) : 1;
			if (Scene.GetPlayer() is Player entity && DynamicPositionOver_Quarter(entity.X))
			{
				Facing *= -1;
			}
			base.Update();
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
			Sprite.Scale.X = realFacing;
		}

		protected Collider GetCollider(ColliderOption option, string key = "main")
		{
			if (hitboxMetadata[option].TryGetValue(key, out var result))
				return result;

			if (option == ColliderOption.Target)
				return new Circle(4f);

			return new Hitbox(Sprite.Width, option == ColliderOption.Bouncebox ? 6f : Sprite.Height,
				Sprite.Width * -0.5f, Sprite.Height * -0.5f);
		}

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
				BossFunctions.OnDamage(HurtMode).AsCoroutine(this);
				postLua?.Invoke();
			}
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

		protected override Component GetBossCollision()
			=> new PlayerCollider(OnPlayerBounce, Hurtbox);

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
			=> new SidekickTarget(() => OnDamage(), BossID, Hurtbox);
	}

	public class CustomBossPuppet(EntityData data, Vector2 offset) : BossPuppet(data, offset)
	{
		public override HurtModes HurtMode => HurtModes.Custom;

		protected override Component GetBossCollision()
			=> null;
	}
}
