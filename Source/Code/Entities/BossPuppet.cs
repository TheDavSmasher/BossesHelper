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

        private readonly Dictionary<string, object> storedObjects;

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
            freezeSidekickOnAttack = data.Bool("sidekickFreeze");
            sidekickCooldown = data.Float("sidekickCooldown");
            metadataPath = data.Attr("hitboxMetadataPath");
            bossHitCooldown = 0f;
            storedObjects = new Dictionary<string, object>();
            HurtMode = data.Enum<HurtModes>("hurtMode", HurtModes.PlayerContact);
            Add(new BossHealthTracker(health));
            killOnContact = data.Bool("killOnContact");
            Add(new PlayerCollider(KillOnContact));
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            facing = MirrorSprite ? -1 : 1;
            if (GFX.SpriteBank.TryCreate(data.Attr("bossSprite"), out Sprite))
            {
                Sprite.Scale = Vector2.One;
                SetHitboxesAndColliders(data.Attr("bossID"));
                Add(Sprite);
                PlayBossAnim("idle");
            }
        }

        internal void SetPuppetFunctions(BossFunctions functions)
        {
            bossFunctions = functions;
        }

        private void SetHitboxesAndColliders(string bossID)
        {
            UserFileReader.ReadMetadataFileInto(metadataPath, out hitboxMetadata);

            base.Collider = GetMainOrDefault(hitboxMetadata.baseHitboxes, Sprite.Height);

            Hurtbox = GetMainOrDefault(hitboxMetadata.baseHurtboxes, Sprite.Height);

            switch (HurtMode)
            {
                case HurtModes.HeadBonk:
                    Add(bossCollision = new PlayerCollider(OnPlayerBounce,
                        Bouncebox = GetMainOrDefault(hitboxMetadata.bounceHitboxes, 6f)));
                    break;
                case HurtModes.SidekickAttack:
                    Add(bossCollision = new SidekickTarget(OnSidekickLaser, bossID,
                        Target = GetMainOrDefault(hitboxMetadata.targetCircles, null)));
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

        private Collider GetMainOrDefault(Dictionary<string, Collider> dictionary, float? value)
        {
            return GetTagOrDefault(dictionary, "main", value);
        }

        private Collider GetTagOrDefault(Dictionary<string, Collider> dictionary, string key, float? value)
        {
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
            if (bossHitCooldown <= 0)
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
            if (bossHitCooldown <= 0)
            {
                ResetBossHitCooldown();
                Add(bossFunctions.OnDamageCoroutine(source));
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
            base.Update();
            //Move based on speed
            MoveH(Speed.X * Engine.DeltaTime, onCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            //Apply gravity
            if (!Grounded)
            {
                Speed.Y = Calc.Approach(Speed.Y, maxFall, effectiveGravity * Engine.DeltaTime);
            }
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
            if (facing == 1 && MirrorSprite || facing == -1 && !MirrorSprite)
                return pos < base.X - base.Collider.Width / 4;
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
            if (Sprite != null && !Sprite.TryPlay(name))
            {
                Logger.Log(LogLevel.Warn, "BossesHelper/BossPuppet", "Animation specified does not exist!");
            }
        }

        #region Lua Helper Functions
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
            base.Collider = GetTagOrDefault(hitboxMetadata.baseHitboxes, tag, Sprite.Height);
        }

        public void ChangeHurtboxOption(string tag)
        {
            Hurtbox = GetTagOrDefault(hitboxMetadata.baseHurtboxes, tag, Sprite.Height);
            if (bossCollision is PlayerCollider collider)
            {
                collider.Collider = Hurtbox;
            }
        }

        public void ChangeBounceboxOption(string tag)
        {
            Bouncebox = GetTagOrDefault(hitboxMetadata.bounceHitboxes, tag, 6f);
            if (bossCollision is PlayerCollider collider)
            {
                collider.Collider = Bouncebox;
            }
        }

        public void ChangeTargetOption(string tag)
        {
            Target = GetTagOrDefault(hitboxMetadata.targetCircles, tag, null);
            if (bossCollision is SidekickTarget target)
            {
                target.Collider = Target;
            }
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

        public void StoreObject(string key, object toStore)
        {
            if (!storedObjects.ContainsKey(key))
                storedObjects.Add(key, toStore);
        }

        public object GetStoredObject(string key)
        {
            return storedObjects.TryGetValue(key, out object storedObject) ? storedObject : null;
        }

        public void DeleteStoredObject(string key)
        {
            storedObjects.Remove(key);
        }
        #endregion
    }
}
