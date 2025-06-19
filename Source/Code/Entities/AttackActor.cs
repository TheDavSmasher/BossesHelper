using Celeste.Mod.BossesHelper.Code.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using NLua;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    internal class AttackActor : Actor
    {
        public readonly Sprite Sprite;

        private readonly LuaFunction onCollide;

        public Vector2 Speed;

        public const float Gravity = 900f;

        public bool SolidCollidable;

        private readonly Collision onCollideH;

        private readonly Collision onCollideV;

        public bool Grounded { get; private set; }

        private readonly float maxFall;

        private float effectiveGravity;

        public AttackActor(Vector2 position, Collider attackbox, LuaFunction onPlayer, bool startCollidable,
            bool solidCollidable, string spriteName, float gravMult, float maxFall, float xScale = 1f, float yScale = 1f)
            : base(position)
        {
            base.Collider = attackbox;
            base.Collidable = startCollidable;
            SolidCollidable = solidCollidable;
            effectiveGravity = gravMult * Gravity;
            this.maxFall = maxFall;
            if (GFX.SpriteBank.TryCreate(spriteName, out Sprite sprite))
            {
                Sprite = sprite;
                Sprite.Scale = new Vector2(xScale, yScale);
                Add(Sprite);
            }
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            onCollide = onPlayer;
            Add(new PlayerCollider(OnPlayer));
        }

        public override void Update()
        {
            Grounded = Speed.Y >= 0 && OnGround();
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

        public void PlayAnim(string anim)
        {
            if (!Sprite.TryPlay(anim))
            {
                Logger.Log(LogLevel.Warn, "BossesHelper/AttackEntity", "Animation specified does not exist!");
            }
        }

        private void OnPlayer(Player player)
        {
            onCollide.Call(this, player);
        }

        public void SetSolidCollisionActive(bool active)
        {
            SolidCollidable = active;
        }

        public void SetCollisionActive(bool active)
        {
            base.Collidable = active;
        }

        public void SetEffectiveGravityMult(float mult)
        {
            effectiveGravity = mult * Gravity;
        }
    }
}
