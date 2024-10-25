using Monocle;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    public class SidekickTarget : Entity
    {
        public readonly string BossName;

        public Action OnLaserCollide;

        public SidekickTarget(string bossName, Vector2 position, Action onLaser, Collider collider)
            : base(position)
        {
            base.Collider = collider;
            OnLaserCollide = onLaser;
            BossName = bossName;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Depth = -1000;
        }

        public void OnLaser()
        {
            OnLaserCollide?.Invoke();
        }

        public override void DebugRender(Camera camera)
        {
            if (Collider != null)
            {
                if (Collider is Circle single)
                {
                    Draw.Circle(Center, single.Radius, Color.AliceBlue, 10);
                }
                if (Collider is ColliderList colliderList)
                {
                    foreach (Collider collider in colliderList.colliders)
                    {
                        Draw.Circle(Center, (collider as Circle).Radius, Color.AliceBlue, 10);
                    }
                }
            }
        }
    }
}
