﻿using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class SidekickTarget : Component
    {
        private readonly SidekickTargetCollider sidekickTarget;

        public readonly string BossName;

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

        public SidekickTarget(Action onLaser, string bossName, Vector2 position, Collider target)
            : base(active: true, visible: false)
        {
            BossName = bossName;
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

        public override void DebugRender(Camera camera)
        {
            if (Collider != null)
            {
                if (Collider is Circle single)
                {
                    Draw.Circle(single.AbsolutePosition, single.Radius, Color.AliceBlue, 10);
                }
                if (Collider is ColliderList colliderList)
                {
                    foreach (Collider collider in colliderList.colliders)
                    {
                        Circle target = collider as Circle;
                        Draw.Circle(target.AbsolutePosition, target.Radius, Color.AliceBlue, 10);
                    }
                }
            }
        }
    }
}
