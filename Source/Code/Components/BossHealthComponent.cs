using Monocle;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    [Tracked(false)]
    public class BossHealthComponent : Component
    {
        public readonly Func<int> Health;

        public BossHealthComponent(Func<int> health)
            : base(active: true, visible: false)
        {
            Health = health;
        }
    }
}
