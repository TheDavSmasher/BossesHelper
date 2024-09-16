using Monocle;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked]
    internal class SidekickTarget : Entity
    {
        public readonly string BossName;

        public Action OnLaserCollide;

        public SidekickTarget(string bossName, Vector2 position, Action onLaser, float radius = 4f)
            : base(position)
        {
            base.Collider = new Circle(radius, position.X, position.Y);
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
    }
}
