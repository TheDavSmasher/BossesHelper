using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class SidekickTarget : Component
    {
        private readonly SidekickTargetCollider sidekickTarget;

        public readonly string BossID;

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

        public Vector2 Position
        {
            get
            {
                return sidekickTarget.Position;
            }
            set
            {
                sidekickTarget.Position = value;
            }
        }

        public SidekickTarget(Action onLaser, string bossId, Vector2 position, Collider target)
            : base(active: true, visible: false)
        {
            BossID = bossId;
            sidekickTarget = new(position, onLaser, target);
        }

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            (scene as Level).Add(sidekickTarget);
            sidekickTarget.Depth = -1000;
        }

        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            sidekickTarget.RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            Position = Entity.Position;
        }
    }
}
