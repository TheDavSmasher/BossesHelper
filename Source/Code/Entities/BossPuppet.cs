using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Celeste.Mod.BossesHelper.Code.Other;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    public class BossPuppet : Actor
    {
        public struct HitboxMedatata(Dictionary<string, Collider> baseHitboxes, Dictionary<string, Collider> baseHurtboxes,
            Dictionary<string, Collider> bounceHitboxes, Dictionary<string, Collider> targetCircles)
        {
            public Dictionary<string, Collider> baseHitboxes = baseHitboxes;

            public Dictionary<string, Collider> baseHurtboxes = baseHurtboxes;

            public Dictionary<string, Collider> bounceHitboxes = bounceHitboxes;

            public Dictionary<string, Collider> targetCircles = targetCircles;

            public readonly bool UseDefaultHitbox => baseHitboxes == null || baseHitboxes.Count == 0;

            public readonly bool UseDefaultHurtbox => baseHurtboxes == null || baseHurtboxes.Count == 0;

            public readonly bool UseDefaultBounce => bounceHitboxes == null || bounceHitboxes.Count == 0;

            public readonly bool UseDefaultTarget => targetCircles == null || targetCircles.Count == 0;
        }

        private readonly Sprite Sprite;

        private HitboxMedatata hitboxMetadata;

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

        private BossFunctions bossFunctions;

        private readonly float bossHitCooldownBase;

        private float bossHitCooldown;

        private int facing;

        public const float Gravity = 900f;

        public Vector2 Speed;

        private readonly Collision onCollideH;

        private readonly Collision onCollideV;

        public bool onGround { get; private set; }

        private readonly float maxFall;

        private float effectiveGravity;

        private readonly string metadataPath;

        public BossPuppet(EntityData data, Vector2 offset, Func<int> health) : base(data.Position + offset)
        {
            string SpriteName = data.Attr("bossSprite");
            DynamicFacing = data.Bool("dynamicFacing");
            MirrorSprite = data.Bool("mirrorSprite");
            bossHitCooldownBase = data.Float("bossHitCooldown", 0.5f);
            maxFall = data.Float("maxFall", 90f);
            effectiveGravity = data.Float("baseGravityMultiplier", 1f) * Gravity;
            freezeSidekickOnAttack = data.Bool("sidekickFreeze");
            sidekickCooldown = data.Float("sidekickCooldown");
            metadataPath = data.Attr("hitboxMetadataPath");
            bossHitCooldown = 0f;
            HurtMode = data.Enum<HurtModes>("hurtMode", HurtModes.PlayerContact);
            Add(new BossHealthTracker(health));
            if (!string.IsNullOrEmpty(SpriteName))
            {
                Sprite = GFX.SpriteBank.Create(SpriteName);
                Sprite.Scale = Vector2.One;
                SetHitboxesAndColliders(data.Attr("bossID"));
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

        internal void SetPuppetFunctions(BossFunctions functions)
        {
            bossFunctions = functions;
        }

        private void SetHitboxesAndColliders(string bossID)
        {
            UserFileReader.ReadMetadataFileInto(metadataPath, out hitboxMetadata);

            base.Collider = hitboxMetadata.UseDefaultHitbox
                ? new Hitbox(Sprite.Width, Sprite.Height, Sprite.Width * -0.5f, Sprite.Height * -0.5f)
                : GetMainFromDictionary(hitboxMetadata.baseHitboxes);

            Collider Hurtbox = hitboxMetadata.UseDefaultHurtbox
                ? new Hitbox(Sprite.Width, Sprite.Height, Sprite.Width * -0.5f, Sprite.Height * -0.5f)
                : GetMainFromDictionary(hitboxMetadata.baseHurtboxes);

            switch (HurtMode)
            {
                case HurtModes.HeadBonk:
                    Add(bossCollision = new PlayerCollider(OnPlayerBounce,
                        hitboxMetadata.UseDefaultBounce
                        ? new Hitbox(base.Collider.Width, 6f, Sprite.Width * -0.5f, Sprite.Height * -0.5f)
                        : GetMainFromDictionary(hitboxMetadata.bounceHitboxes)
                    ));
                    break;
                case HurtModes.SidekickAttack:
                    Add(bossCollision = new SidekickTarget(OnSidekickLaser, bossID, Position,
                        hitboxMetadata.UseDefaultTarget
                        ? new Circle(4f)
                        : GetMainFromDictionary(hitboxMetadata.targetCircles)));
                    break;
                case HurtModes.PlayerDash:
                    Add(bossCollision = new PlayerCollider(OnPlayerDash, Hurtbox));
                    break;
                case HurtModes.PlayerContact:
                    Add(bossCollision = new PlayerCollider(OnPlayerContact, Hurtbox));
                    break;
                default: //Custom
                    //Custom depends on Setup.lua's code, does nothing by default
                    break;
            }
        }

        private static Collider GetMainFromDictionary(Dictionary<string, Collider> dictionary)
        {
            return (dictionary.Count > 1) ? dictionary["main"] : dictionary.Values.First();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player player = scene.Tracker.GetEntity<Player>();
            if (HurtMode == HurtModes.SidekickAttack && scene.Tracker.GetEntity<BadelineSidekick>() == null)
            {
                (scene as Level).Add(new BadelineSidekick(player.Position + new Vector2(-16f * (int)player.Facing, -4f), freezeSidekickOnAttack, sidekickCooldown));
            }
        }

        public override void Update()
        {
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null && DynamicPositionOver_Quarter(entity.X))
            {
                facing *= -1;
            }
            if (bossHitCooldown > 0)
            {
                bossHitCooldown -= Engine.DeltaTime;
            }
            onGround = Speed.Y >= 0 && OnGround();
            base.Update();
            //Move based on speed
            MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            //Apply gravity
            if (!onGround)
            {
                Speed.Y = Calc.Approach(Speed.Y, maxFall, effectiveGravity * Engine.DeltaTime);
            }
            //Return Sprite Scale
            Sprite.Scale.X = Calc.Approach(Sprite.Scale.X, 1f, 1.75f * Engine.DeltaTime);
            Sprite.Scale.Y = Calc.Approach(Sprite.Scale.Y, 1f, 1.75f * Engine.DeltaTime);
        }

        private bool DynamicPositionOver_Quarter(float pos)
        {
            if (!DynamicFacing)
                return false;
            if (facing == 1 && MirrorSprite || facing == -1 && !MirrorSprite)
            {
                return pos < base.X - base.Collider.Width / 4;
            }
            return pos > base.X + base.Collider.Width / 4;
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

        public void SetXSpeed(float speed)
        {
            Speed.X = speed;
        }

        public void SetYSpeed(float speed)
        {
            Speed.Y = speed;
        }

        public void SetSpeed(float x, float y)
        {
            Speed.Y = y;
            Speed.X = x;
        }

        public void SetXSpeedDuring(float speed, float time)
        {
            Add(new Coroutine(KeepXSpeed(speed, time)));
        }

        public void SetYSpeedDuring(float speed, float time)
        {
            Add(new Coroutine(KeepYSpeed(speed, time)));
        }

        public void SetSpeedDuring(float x, float y, float time)
        {
            SetXSpeedDuring(x, time);
            SetYSpeedDuring(y, time);
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
            base.Collider = hitboxMetadata.baseHitboxes?[tag];
        }

        public void ChangeHurtboxOption(string tag)
        {
            (bossCollision as PlayerCollider).Collider = hitboxMetadata.baseHurtboxes?[tag];
        }

        public void ChangeBounceboxOption(string tag)
        {
            (bossCollision as PlayerCollider).Collider = hitboxMetadata.bounceHitboxes?[tag];
        }

        public void ChangeTargetOption(string tag)
        {
            (bossCollision as SidekickTarget).Collider = hitboxMetadata.targetCircles?[tag];
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
                Add(new Coroutine(bossFunctions.OnLaserCoroutine()));
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
                Add(new Coroutine(bossFunctions.OnBounceCoroutine()));
            }
        }

        private void OnPlayerDash(Player player)
        {
            if (bossHitCooldown <= 0 && player.DashAttacking && player.Speed != Vector2.Zero)
            {
                ResetBossHitCooldown();
                Add(new Coroutine(bossFunctions.OnDashCoroutine()));
            }
        }

        private void OnPlayerContact(Player player)
        {
            if (bossHitCooldown <= 0)
            {
                ResetBossHitCooldown();
                Add(new Coroutine(bossFunctions.OnContactCoroutine()));
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

        public void SpeedXTween(float start, float target, float time, Ease.Easer easer = null)
        {
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, easer, time, true);
            tween.OnUpdate = delegate (Tween t)
            {
                Speed.X = start + (target - start) * t.Eased;
            };
            Add(tween);
        }

        public void SpeedYTween(float start, float target, float time, Ease.Easer easer = null)
        {
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, easer, time, true);
            tween.OnUpdate = delegate (Tween t)
            {
                Speed.Y = start + (target - start) * t.Eased;
            };
            Add(tween);
        }
    }
}
