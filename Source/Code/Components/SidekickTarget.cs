using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    [Tracked(false)]
    public class SidekickTarget(Action onLaser, string bossId, Collider target) : Component(active: true, visible: true)
    {
        public readonly string BossID = bossId;

        public readonly Action OnLaser = onLaser;

        public Collider Collider = target;

        public bool CollideCheck(Vector2 from, Vector2 to)
        {
            if (Entity == null) return false;
            if (Entity.Collidable)
            {
                Collider original = Entity.Collider;
                Entity.Collider = Collider;
                bool hit = Entity.CollideLine(from, to);
                Entity.Collider = original;
                return hit;
            }
            return false;
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            Collider.Entity = entity;
        }

        public override void DebugRender(Camera camera)
        {
            if (Collider is Circle single)
            {
                RenderTarget(single);
            }
            else if (Collider is ColliderList colliderList)
            {
                foreach (Circle collider in colliderList.colliders.Cast<Circle>())
                {
                    RenderTarget(collider);
                }
            }
        }

        private void RenderTarget(Circle target)
        {
            Draw.Circle(target.AbsolutePosition + Entity.Position, target.Radius, Color.AliceBlue, 10);
        }
    }
}
