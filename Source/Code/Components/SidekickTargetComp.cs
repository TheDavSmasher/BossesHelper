using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    internal class SidekickTargetComp : Component
    {
        public SidekickTarget target;

        public Vector2 Offset {  get; set; }

        public SidekickTargetComp(string bossName, Vector2 position, Vector2 offset, float radius = 4f)
            : base(active: true, visible: false)
        {
            target = new SidekickTarget(bossName, position + offset, radius);
            Offset = offset;
        }

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            (scene as Level).Add(target);
        }

        public override void Update()
        {
            base.Update();
            target.Position = Entity.Position + Offset;
        }
    }
}
