using Monocle;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    internal class SidekickTarget : Entity
    {
        public Circle circle;

        public SidekickTarget(Vector2 position, float radius) : base(position)
        {
            circle = new Circle(radius, position.X, position.Y);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Depth = -1000;
        }
    }
}
