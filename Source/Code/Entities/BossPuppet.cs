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
        public enum ColliderOption
        {
            Hitboxes,
            Hurtboxes,
            Bouncebox,
            Target
        }

        public Sprite Sprite { get; private set; }

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

        private BossFunctions bossFunctions;

        private readonly float bossHitCooldownBase;

        public float BossHitCooldown { get; private set; }

        public int Facing;

        public const float Gravity = 900f;

        public Vector2 Speed;

        public float groundFriction;

        public float airFriction;

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
            BossHitCooldown = 0f;
            SolidCollidable = data.Bool("startSolidCollidable");
            base.Collidable = data.Bool("startCollidable");
            storedObjects = new Dictionary<string, object>();
            HurtMode = data.Enum<HurtModes>("hurtMode", HurtModes.PlayerContact);
            Add(new BossHealthTracker(health));
            killOnContact = data.Bool("killOnContact");
            Add(new PlayerCollider(KillOnContact));
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            Facing = 1;
            if (GFX.SpriteBank.TryCreate(data.Attr("bossSprite"), out Sprite sprite))
            {
                Sprite = sprite;
                Sprite.Scale = Vector2.One;
                UserFileReader.ReadMetadataFileInto(metadataPath, out hitboxMetadata);
                SetHitboxesAndColliders(data.Attr("bossID"));
                Add(Sprite);
                PlayBossAnim(data.String("startingAnim", "idle"));
            }
            else
            {
                Sprite = sprite;
            }
        }

        internal void SetPuppetFunctions(BossFunctions functions)
        {
            bossFunctions = functions;
        }

        private void SetHitboxesAndColliders(string bossID)
        {
            Collider = GetMainOrDefault(ColliderOption.Hitboxes, Sprite.Height);

            Hurtbox = GetMainOrDefault(ColliderOption.Hurtboxes, Sprite.Height);

            switch (HurtMode)
            {
                case HurtModes.HeadBonk:
                    Add(bossCollision = new PlayerCollider(OnPlayerBounce,
                        Bouncebox = GetMainOrDefault(ColliderOption.Bouncebox, 6f)));
                    break;
                case HurtModes.SidekickAttack:
                    Add(bossCollision = new SidekickTarget(OnSidekickLaser, bossID,
                        Target = GetMainOrDefault(ColliderOption.Target, null)));
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
            Player player = scene.GetPlayer();
            if (HurtMode == HurtModes.SidekickAttack && scene.Tracker.GetEntity<BadelineSidekick>() == null)
            {
                (scene as Level).Add(new BadelineSidekick(player.Position + new Vector2(-16f * (int)player.Facing, -4f), freezeSidekickOnAttack, sidekickCooldown));
            }
        }

        public override void Update()
        {
            if (Facing != 0)
            {
                Facing /= Math.Abs(Facing);
            }
            else
            {
                Facing = 1;
            }
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
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
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

        public IEnumerator WaitBossAnim(string anim)
        {
            if (Sprite != null && Sprite.Has(anim))
            {
                yield return Sprite.PlayAnim(anim);
            }
        }

        #region Lua Helper Functions
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

        public float SetXSpeedDuring(float speed, float time)
        {
            Add(new Coroutine(KeepXSpeed(speed, time)));
            return time;
        }

        public float SetYSpeedDuring(float speed, float time)
        {
            Add(new Coroutine(KeepYSpeed(speed, time)));
            return time;
        }

        public float SetSpeedDuring(float x, float y, float time)
        {
            SetXSpeedDuring(x, time);
            SetYSpeedDuring(y, time);
            return time;
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
            BossHitCooldown = timer;
        }

        public void ResetBossHitCooldown()
        {
            BossHitCooldown = bossHitCooldownBase;
        }

        public void ChangeHitboxOption(string tag)
        {
            Collider = GetTagOrDefault(ColliderOption.Hitboxes, tag, Sprite.Height);
        }

        public void ChangeHurtboxOption(string tag)
        {
            Hurtbox = GetTagOrDefault(ColliderOption.Hurtboxes, tag, Sprite.Height);
            if (bossCollision is PlayerCollider collider)
            {
                collider.Collider = Hurtbox;
            }
        }

        public void ChangeBounceboxOption(string tag)
        {
            Bouncebox = GetTagOrDefault(ColliderOption.Bouncebox, tag, 6f);
            if (bossCollision is PlayerCollider collider)
            {
                collider.Collider = Bouncebox;
            }
        }

        public void ChangeTargetOption(string tag)
        {
            Target = GetTagOrDefault(ColliderOption.Target, tag, null);
            if (bossCollision is SidekickTarget target)
            {
                target.Collider = Target;
            }
        }

        public float PositionTween(Vector2 target, float time, Ease.Easer easer = null)
        {
            Tween.Position(this, target, time, easer);
            return time;
        }

        public float SpeedXTween(float start, float target, float time, Ease.Easer easer = null)
        {
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, easer, time, true);
            tween.OnUpdate = (Tween t) => Speed.X = start + (target - start) * t.Eased;
            Add(tween);
            return time;
        }

        public float SpeedYTween(float start, float target, float time, Ease.Easer easer = null)
        {
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, easer, time, true);
            tween.OnUpdate = (Tween t) => Speed.Y = start + (target - start) * t.Eased;
            Add(tween);
            return time;
        }

        public float SpeedTween(float xStart, float yStart, float xTarget, float yTarget, float time, Ease.Easer easer = null)
        {
            SpeedXTween(xStart, xTarget, time, easer);
            SpeedYTween(yStart, yTarget, time, easer);
            return time;
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
