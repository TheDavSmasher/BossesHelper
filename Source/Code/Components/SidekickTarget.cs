using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class SidekickTarget : Component
    {
        private SidekickTargetCollider sidekickTarget;

        private Action onLaser;

        private readonly string bossName;

        public Collider Collider
        {
            get
            {
                return sidekickTarget.Collider;
            }
            set
            {
                sidekickTarget.Collider = value;
            }
        }

        public SidekickTarget(Action onLaser, string bossName, Vector2 position, Collider target)
            : base(active: true, visible: false)
        {
            sidekickTarget = new(bossName, position, onLaser, target);
        }

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            (scene as Level).Add(sidekickTarget);
        }

        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            sidekickTarget.RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            sidekickTarget.Position = Entity.Position;
        }
    }
}
