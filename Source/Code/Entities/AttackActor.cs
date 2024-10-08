using Microsoft.Xna.Framework;
using Monocle;
using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    internal class AttackActor : Actor
    {
        private readonly Sprite sprite;

        private readonly LuaFunction onCollide;

        public Vector2 Speed;

        public const float Gravity = 900f;

        private readonly Collision onCollideH;

        private readonly Collision onCollideV;

        public bool onGround { get; private set; }

        private readonly float maxFall;

        private float effectiveGravity;

        public AttackActor(Vector2 position, Collider attackbox, LuaFunction onPlayer, bool startCollidable, string spriteName, float gravMult, float maxFall, float xScale = 1f, float yScale = 1f)
            : base(position)
        {
            base.Collider = attackbox;
            base.Collidable = startCollidable;
            effectiveGravity = gravMult * Gravity;
            this.maxFall = maxFall;
            sprite = GFX.SpriteBank.Create(spriteName);
            sprite.Scale = new Vector2(xScale, yScale);
            Add(sprite);
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            onCollide = onPlayer;
            Add(new PlayerCollider(OnPlayer));
        }

        public override void Update()
        {
            base.Update();
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
            if (sprite.Has(anim))
            {
                sprite.Play(anim);
            }
            else
            {
                Logger.Log(LogLevel.Warn, "BossesHelper/AttackEntity", "Animation specified does not exist!");
            }
        }

        private void OnPlayer(Player player)
        {
            onCollide.Call(player);
        }

        public void SetCollisionActive(bool active)
        {
            base.Collidable = active;
        }
    }
}
