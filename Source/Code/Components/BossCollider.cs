using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    internal class BossCollider : Component
    {
        public Collider Collider;

        public Action<Player> OnCollide;
        public BossCollider(Collider collider, Action<Player> onCollide) : base(active: false, visible: false)
        {
            Collider = collider;
            OnCollide = onCollide;
        }
    }
}
