using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Celeste.Mod.BossesHelper.Code.Other;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.BossesHelper.Code.Entities.BossController;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    public class BossPuppet : Actor
    {
        public struct HitboxMedatata(Dictionary<string, Collider> baseHitboxes, Dictionary<string, Collider> baseHurtboxes, Hitbox bounceHitbox, Vector2 target, float radius)
        {
            public Dictionary<string, Collider> baseHitboxes = baseHitboxes;

            public Dictionary<string, Collider> baseHurtboxes = baseHurtboxes;

            public Hitbox bounceHitbox = bounceHitbox;

            public Vector2 targetOffset = target;

            public float targetRadius = radius;

            public readonly bool UseDefaultHitbox => baseHitboxes == null || baseHitboxes.Count == 0;

            public readonly bool UseDefaultHurtbox => baseHurtboxes == null || baseHurtboxes.Count == 0;

            public readonly bool UseDefaultBounce => bounceHitbox == null;
        }

        private readonly Sprite Sprite;

        private Dictionary<string, Collider> hitboxOptions;

        private Dictionary<string, Collider> hurtboxOptions;

        public Collider Hurtbox { get; private set; }

        private readonly string SpriteName;

        private readonly bool DynamicFacing;

        private readonly bool MirrorSprite;

        private readonly BossController.HurtModes HurtMode;

        private readonly Vector2[] nodes;

        private BossInterruption OnInterrupt;

        private readonly float bossHitCooldownBase;

        private float bossHitCooldown;

        private int facing;

        private Level Level;

        //Movement constants
        public const float Gravity = 900f;

        //Movement variables
        public Vector2 Speed;

        private readonly Collision onCollideH;

        private readonly Collision onCollideV;

        public bool onGround {  get; private set; }

        private readonly float maxFall;

        private float effectiveGravity;

        public BossPuppet(EntityData data, Vector2 offset, HurtModes hurtMode) : base(data.Position + offset)
        {
            nodes = data.Nodes;
            SpriteName = data.Attr("bossSprite");
            DynamicFacing = data.Bool("dynamicFacing");
            MirrorSprite = data.Bool("mirrorSprite");
            bossHitCooldownBase = data.Float("bossHitCooldown", 0.5f);
            maxFall = data.Float("maxFall", 90f);
            effectiveGravity = data.Float("baseGravityMultiplier", 1f) * Gravity;
            bossHitCooldown = 0f;
            nodes = data.NodesWithPosition(Vector2.Zero);
            HurtMode = hurtMode;
            if (!string.IsNullOrEmpty(SpriteName))
            {
                Sprite = GFX.SpriteBank.Create(SpriteName);
                Sprite.Scale = Vector2.One;
                SetHitboxesAndColliders();
                onCollideH = OnCollideH;
                onCollideV = OnCollideV;
                facing = MirrorSprite ? -1 : 1;
                Add(Sprite);
                PlayBossAnim("idle");
                if (data.Bool("killOnContact"))
                {
                    Add(new PlayerCollider(KillOnContact));
                }
            }
        }

        internal void SetOnInterrupt(BossInterruption onInterrupt)
        {
            OnInterrupt = onInterrupt;
        }

        internal void SetCustomBossSetup(Player player)
        {
            UserFileReader.ReadCustomSetupFile(player, this);
        }

        private void SetHitboxesAndColliders()
        {
            UserFileReader.ReadMetadataFileInto(out HitboxMedatata hitboxMetadata);
            if (hitboxMetadata.UseDefaultHitbox)
            {
                base.Collider = new Hitbox(Sprite.Width, Sprite.Height, Sprite.Width * -0.5f, Sprite.Height * -0.5f);
            }
            else if (hitboxMetadata.baseHitboxes.Count > 1)
            {
                hitboxOptions = hitboxMetadata.baseHitboxes;
                base.Collider = hitboxMetadata.baseHitboxes["main"];
            }
            else
            {
                base.Collider = hitboxMetadata.baseHitboxes.Values.First();
            }
            if (hitboxMetadata.UseDefaultHurtbox)
            {
                Hurtbox = new Hitbox(Sprite.Width, Sprite.Height, Sprite.Width * -0.5f, Sprite.Height * -0.5f);
            }
            else if (hitboxMetadata.baseHurtboxes.Count > 1)
            {
                hurtboxOptions = hitboxMetadata.baseHurtboxes;
                Hurtbox = hitboxMetadata.baseHurtboxes["main"];
            }
            else
            {
                Hurtbox = hitboxMetadata.baseHurtboxes.Values.First();
            }
            switch (HurtMode)
            {
                case HurtModes.HeadBonk:
                    if (hitboxMetadata.UseDefaultBounce)
                    {
                        Add(new PlayerCollider(OnPlayerBounce, new Hitbox(base.Collider.Width, 6f, Sprite.Width * -0.5f, Sprite.Height * -0.5f)));
                    }
                    else
                    {
                        Add(new PlayerCollider(OnPlayerBounce, hitboxMetadata.bounceHitbox));
                    }
                    break;
                case HurtModes.SidekickAttack:
                    Add(new SidekickTargetComp(OnSidekickLaser, SpriteName, Position, hitboxMetadata.targetOffset, hitboxMetadata.targetRadius));
                    break;
                case HurtModes.PlayerDash:
                    Add(new PlayerCollider(OnPlayerDash, Hurtbox));
                    break;
                    case HurtModes.Custom:
                    //Custom depends on Setup.lua's code, does nothing by default
                    break;
                default: //PlayerContact 
                    Add(new PlayerCollider(OnPlayerContact, Hurtbox));
                    break;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level = SceneAs<Level>();
        }

        public override void Update()
        {
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                if (DynamicFacing)
                {
                    if (facing == -1 && PositionOver_Quarter(entity.X, !MirrorSprite))
                    {
                        facing = 1;
                    }
                    else if (facing == 1 && PositionOver_Quarter(entity.X, MirrorSprite))
                    {
                        facing = -1;
                    }
                }
                
            }
            if (bossHitCooldown > 0)
            {
                bossHitCooldown -= Engine.DeltaTime;
            }
            if (Speed.Y >= 0)
            {
                Platform first = CollideFirst<Solid>(Position + Vector2.UnitY);
                first ??= CollideFirstOutside<JumpThru>(Position + Vector2.UnitY);
                onGround = first != null;
            }
            else
            {
                onGround = false;
            }
            base.Update();
            //Move based on speed
            MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            //Apply gravity
            if (!onGround)
            {
                Speed.Y = Calc.Approach(Speed.Y, maxFall, effectiveGravity * Engine.DeltaTime);
            }
        }

        public void EnableCollisions()
        {
            base.Collidable = true;
        }

        public void DisableCollisions()
        {
            base.Collidable = false;
        }

        public void SetGravityMult(float mult)
        {
            effectiveGravity = Gravity * mult;
        }

        public void SetXSpeed(float speed, int dir = 1)
        {
            Speed.X = speed * dir;
        }

        public void SetYSpeed(float speed, int dir = 1)
        {
            Speed.Y = speed * dir;
        }

        public void SetSpeed(float x, float y)
        {
            Speed.Y = y;
            Speed.X = x;
        }

        public void SetXSpeedDuring(float speed, float time, int dir = 1)
        {
            Add(new Coroutine(KeepXSpeed(speed * dir, time)));
        }

        public void SetYSpeedDuring(float speed, float time, int dir = 1)
        {
            Add(new Coroutine(KeepYSpeed(speed * dir, time)));
        }

        public void SetSpeedDuring(float x, float y, float time, int dir = 1)
        {
            Add(new Coroutine(KeepXSpeed(x * dir, time)));
            Add(new Coroutine(KeepYSpeed(y * dir, time)));
        }

        private IEnumerator KeepXSpeed(float speed, float time)
        {
            float timer = 0;
            while (timer < time)
            {
                Speed.X = speed;
                timer += Engine.DeltaTime;
                yield return null;
            }
        }

        private IEnumerator KeepYSpeed(float speed, float time)
        {
            float timer = 0;
            while (timer < time)
            {
                Speed.Y = speed;
                timer += Engine.DeltaTime;
                yield return null;
            }
        }

        private bool PositionOver_Quarter(float pos, bool left)
        {
            if (left)
            {
                return pos < base.X - base.Collider.Width / 4;
            }
            return pos > base.X + base.Collider.Width / 4;
        }

        public override void Render()
        {
            base.Render();
            if (Sprite != null)
            {
                Sprite.Scale.X = facing;
            }
        }

        public void PlayBossAnim(string name)
        {
            if (Sprite != null)
            {
                if (Sprite.Has(name))
                {
                    Sprite.Play(name);
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "BossesHelper/BossPuppet", "Animation specified does not exist!");
                }
            }
        }

        public void SetBossHitCooldown(float timer)
        {
            bossHitCooldown = timer;
        }

        public void ResetBossHitCooldown()
        {
            bossHitCooldown = bossHitCooldownBase;
        }

        public void ChangeHitboxOption(string tag)
        {
            base.Collider = hitboxOptions?[tag];
        }

        public void ChangeHurtboxOption(string tag)
        {
            Hurtbox = hurtboxOptions?[tag];
        }

        private void KillOnContact(Player player)
        {
            player.Die((player.Position - Position).SafeNormalize());
        }

        private void OnSidekickLaser()
        {
            if (bossHitCooldown <= 0)
            {
                ResetBossHitCooldown();
                Add(new Coroutine(OnInterrupt.OnLaserCoroutine()));
            }
        }

        private void OnPlayerBounce(Player player)
        {
            if (bossHitCooldown <= 0)
            {
                ResetBossHitCooldown();
                Audio.Play("event:/game/general/thing_booped", Position);
                Celeste.Freeze(0.2f);
                player.Bounce(base.Top + 2f);
                Add(new Coroutine(OnInterrupt.OnBounceCoroutine()));
            }
        }

        private void OnPlayerDash(Player player)
        {
            if (bossHitCooldown <= 0 && player.DashAttacking && player.Speed != Vector2.Zero)
            {
                ResetBossHitCooldown();
                Add(new Coroutine(OnInterrupt.OnDashCoroutine()));
            }
        }

        private void OnPlayerContact(Player player)
        {
            if (bossHitCooldown <= 0)
            {
                ResetBossHitCooldown();
                Add(new Coroutine(OnInterrupt.OnHitCoroutine()));
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

        public void PositionTween(Vector2 target, float time, Ease.Easer easer = null)
        {
            Tween.Position(this, target, time, easer);
        }

        public void SpeedXTween(float target, float time, Ease.Easer easer = null)
        {
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, easer, time, true);
            tween.OnUpdate = delegate (Tween t)
            {
                Speed.X = target * t.Eased;
            };
            Add(tween);
        }

        public void SpeedYTween(float target, float time, Ease.Easer easer = null)
        {
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, easer, time, true);
            tween.OnUpdate = delegate (Tween t)
            {
                Speed.Y = target * t.Eased;
            };
            Add(tween);
        }
    }
}
