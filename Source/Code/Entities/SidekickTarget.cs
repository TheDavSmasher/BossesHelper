using Monocle;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    internal class SidekickTarget : Entity
    {
        public readonly string BossName;

        private readonly float radius;

        public Action OnLaserCollide;

        public SidekickTarget(string bossName, Vector2 position, Action onLaser, Circle collider)
            : base(position)
        {
            base.Collider = collider;
            this.radius = collider.Radius;
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
                Draw.Circle(Center, radius, Color.AliceBlue, 10);
            }
        }
    }
}
