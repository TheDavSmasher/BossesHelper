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

        public Action OnLaser;
        public BossCollider(Collider collider, Action onLaser) : base(active: false, visible: false)
        {
            Collider = collider;
            OnLaser = onLaser;
        }
    }
}
