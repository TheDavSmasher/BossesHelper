using Monocle;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    internal class SidekickTargetCollider : Entity
    {
        private readonly Action onLaser;

        public SidekickTargetCollider(Vector2 position, Action onLaser, Collider collider)
            : base(position)
        {
            base.Collider = collider;
            this.onLaser = onLaser;
        }

        public void OnLaser()
        {
            onLaser?.Invoke();
        }
    }
}
