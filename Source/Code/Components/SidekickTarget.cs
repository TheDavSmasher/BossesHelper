using Monocle;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    [Tracked(false)]
    public class SidekickTarget(Action onLaser, string bossId, Collider target) : Component(active: true, visible: true)
    {
        public readonly string BossID = bossId;

        private readonly Action onLaser = onLaser;

        public Collider Collider = target;

        public bool CollideCheck(Vector2 from, Vector2 to)
        {
            if (Entity == null) return false;
            if (Entity.Collidable)
            {
                Collider original = base.Entity.Collider;
                base.Entity.Collider = Collider;
                bool hit = Entity.CollideLine(from, to);
                base.Entity.Collider = original;
                return hit;
            }
            return false;
        }

        public void OnLaser()
        {
            onLaser?.Invoke();
        }

        public override void DebugRender(Camera camera)
        {
            if (Collider != null)
            {
                if (Collider is Circle single)
                {
                    Draw.Circle(single.AbsolutePosition, single.Radius, Color.AliceBlue, 10);
                }
                if (Collider is ColliderList colliderList)
                {
                    foreach (Collider collider in colliderList.colliders)
                    {
                        Circle target = collider as Circle;
                        Draw.Circle(target.AbsolutePosition, target.Radius, Color.AliceBlue, 10);
                    }
                }
            }
        }
    }
}
