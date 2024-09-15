using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked]
    internal class SidekickTarget : Entity
    {
        public Circle circle;

        public readonly string BossName;

        public SidekickTarget(string bossName, Vector2 position, float radius = 4f)
            : base(position)
        {
            circle = new Circle(radius, position.X, position.Y);
            BossName = bossName;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Depth = -1000;
        }
    }
}
