using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class SidekickTarget(Action onLaser, string bossId, Vector2 position, Collider target) : Component(active: true, visible: false)
    {
        private readonly SidekickTargetCollider sidekickTarget = new(position, onLaser, target);

        public readonly string BossID = bossId;

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
