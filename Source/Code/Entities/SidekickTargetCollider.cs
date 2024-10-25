using Monocle;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    internal class SidekickTargetCollider : Entity
    {
        private readonly Action onLaser;

        public SidekickTargetCollider(Vector2 position, Action onLaser, Collider collider)
            : base(position)
        {
            base.Collider = collider;
            this.onLaser = onLaser;
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
