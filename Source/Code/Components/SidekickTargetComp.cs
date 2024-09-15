using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    internal class SidekickTargetComp : Component
    {
        public SidekickTarget target;

        public Vector2 Offset {  get; set; }

        public SidekickTargetComp(Vector2 position, Vector2 offset, float radius = 4f) : base(active: true, visible: false)
        {
            target = new SidekickTarget(position + offset, radius);
            Offset = offset;
        }

        public override void Update()
        {
            base.Update();
            target.Position = Entity.Position + Offset;
        }
    }
}
