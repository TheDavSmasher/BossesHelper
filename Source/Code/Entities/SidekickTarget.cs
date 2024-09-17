using Monocle;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked]
    internal class SidekickTarget : Entity
    {
        public readonly string BossName;

        private readonly float radius;

        public Action OnLaserCollide;

        public SidekickTarget(string bossName, Vector2 position, Vector2 offset, Action onLaser, float radius = 4f)
            : base(position + offset)
        {
            base.Collider = new Circle(radius, offset.X, offset.Y);
            this.radius = radius;
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
