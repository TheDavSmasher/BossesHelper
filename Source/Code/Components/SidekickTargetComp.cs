using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    [Tracked]
    internal class SidekickTargetComp : Component
    {
        public SidekickTarget target;

        public Vector2 offset;

        public SidekickTargetComp(Vector2 position, Vector2 offset, float radius) : base(active: true, visible: false)
        {
            target = new SidekickTarget(position + offset, radius);
        }

        public override void Update()
        {
            base.Update();
            target.Position = Entity.Position + offset;
        }
    }
}
