using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Celeste.Mod.BossesHelper.Code.Other;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    public partial class BossPuppet : Actor
    {
        public enum ColliderOption
        {
            Hitboxes,
            Hurtboxes,
            Bouncebox,
            Target
        }

        public readonly Sprite Sprite;

        private readonly Dictionary<ColliderOption, Dictionary<string, Collider>> hitboxMetadata;

        private Component bossCollision;

        private readonly bool DynamicFacing;

        private readonly bool MirrorSprite;

        private readonly bool freezeSidekickOnAttack;

        private readonly float sidekickCooldown;

        public enum HurtModes
        {
            PlayerContact,
            PlayerDash,
            HeadBonk,
            SidekickAttack,
            Custom
        }

        public readonly HurtModes HurtMode;

        internal BossFunctions BossFunctions { get; set; }

        public float BossHitCooldown { get; private set; }

        private readonly float bossHitCooldownBase;

        public int Facing;

        public const float Gravity = 900f;

        public Vector2 Speed;

        public float groundFriction;

        public float airFriction;

        private readonly Dictionary<string, object> storedObjects = [];

        public bool Grounded
        {
            get
            {
                return Speed.Y >= 0 && OnGround();
            }
        }

        public Collider Hurtbox { get; private set; }

        public Collider Bouncebox { get; private set; }

        public Collider Target { get; private set; }

        public bool SolidCollidable;

        private readonly float maxFall;

        private float effectiveGravity;

        private readonly string metadataPath;

        public bool killOnContact;

        public BossPuppet(EntityData data, Vector2 offset, Func<int> health) : base(data.Position + offset)
        {
            DynamicFacing = data.Bool("dynamicFacing");
            MirrorSprite = data.Bool("mirrorSprite");
            bossHitCooldownBase = data.Float("bossHitCooldown", 0.5f);
            maxFall = data.Float("maxFall", 90f);
            effectiveGravity = data.Float("baseGravityMultiplier", 1f) * Gravity;
            groundFriction = data.Float("groundFriction");
            airFriction = data.Float("airFriction");
            freezeSidekickOnAttack = data.Bool("sidekickFreeze");
            sidekickCooldown = data.Float("sidekickCooldown");
            metadataPath = data.Attr("hitboxMetadataPath");
            SolidCollidable = data.Bool("startSolidCollidable");
            Collidable = data.Bool("startCollidable");
            HurtMode = data.Enum("hurtMode", HurtModes.PlayerContact);
            Add(new BossHealthTracker(health));
            killOnContact = data.Bool("killOnContact");
            Add(new PlayerCollider(KillOnContact));
            Facing = 1;
            if (GFX.SpriteBank.TryCreate(data.Attr("bossSprite"), out Sprite sprite))
            {
                Sprite = sprite;
                Sprite.Scale = Vector2.One;
                hitboxMetadata = UserFileReader.ReadMetadataFile(metadataPath);
                SetHitboxesAndColliders(data.Attr("bossID"));
                Add(Sprite);
                PlayBossAnim(data.String("startingAnim", "idle"));
            }
            else
            {
                Sprite = sprite;
            }
        }

        private void SetHitboxesAndColliders(string bossID)
        {
            Collider = GetMainOrDefault(ColliderOption.Hitboxes, Sprite.Height);

            Hurtbox = GetMainOrDefault(ColliderOption.Hurtboxes, Sprite.Height);

            Add(bossCollision = HurtMode switch
            {
                HurtModes.HeadBonk => new PlayerCollider(OnPlayerBounce,
                        Bouncebox = GetMainOrDefault(ColliderOption.Bouncebox, 6f)),
                HurtModes.SidekickAttack => new SidekickTarget(OnSidekickLaser, bossID,
                        Target = GetMainOrDefault(ColliderOption.Target, null)),
                HurtModes.PlayerDash => new PlayerCollider(OnPlayerDash, Hurtbox),
                HurtModes.PlayerContact => new PlayerCollider(OnPlayerContact, Hurtbox),
                _ => null //Custom depends on Setup.lua's code, does nothing by default
            });
        }

        private Collider GetMainOrDefault(ColliderOption option, float? value)
        {
            return GetTagOrDefault(option, "main", value);
        }

        private Collider GetTagOrDefault(ColliderOption option, string key, float? value)
        {
            Dictionary<string, Collider> dictionary = hitboxMetadata[option];
            if (dictionary == null || dictionary.Count == 0 || !dictionary.ContainsKey(key))
            {
                if (value == null)
                    return new Circle(4f);
                return new Hitbox(Sprite.Width, (float)value, Sprite.Width * -0.5f, Sprite.Height * -0.5f);
            }
            return (dictionary.Count > 1) ? dictionary[key] : dictionary.Values.First();
        }

        #region Collision Methods
        private void KillOnContact(Player player)
        {
            if (killOnContact)
                player.Die((player.Position - Position).SafeNormalize());
        }

        private void OnSidekickLaser()
        {
            OnDamage(BossFunctions.DamageSource.Laser);
        }

        private void OnPlayerBounce(Player player)
        {
            OnDamage(BossFunctions.DamageSource.Bounce);
            if (BossHitCooldown <= 0)
            {
                Audio.Play("event:/game/general/thing_booped", Position);
                Celeste.Freeze(0.2f);
                player.Bounce(base.Top + 2f);
            }
        }

        private void OnPlayerDash(Player player)
        {
            if (player.DashAttacking && player.Speed != Vector2.Zero)
            {
                OnDamage(BossFunctions.DamageSource.Dash);
            }
        }

        private void OnPlayerContact(Player _)
        {
            OnDamage(BossFunctions.DamageSource.Contact);
        }

        private void OnDamage(BossFunctions.DamageSource source)
        {
            if (BossHitCooldown <= 0)
            {
                ResetBossHitCooldown();
                Add(BossFunctions.OnDamageCoroutine(source));
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
                return pos < base.X - base.Collider.Width / 4;
            return pos > base.X + base.Collider.Width / 4;
        }

        public override void Render()
        {
            base.Render();
            int realFacing = Facing * (MirrorSprite ? -1 : 1);
            if (Sprite != null)
            {
                Sprite.Scale.X = realFacing;
            }
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
