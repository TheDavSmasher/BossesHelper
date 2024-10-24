using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    internal class SidekickTargetComp(Action onLaser, string bossName, Vector2 position, Circle target)
        : Component(active: true, visible: false)
    {
        public SidekickTarget target = new(bossName, position, onLaser, target);

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            (scene as Level).Add(target);
        }

        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            target.RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            target.Position = Entity.Position;
        }
    }
}
