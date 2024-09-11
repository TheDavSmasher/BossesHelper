using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class SidekickTarget : Component
    {
        public Vector2 Position { get; private set; }
        public SidekickTarget() : base(active: false, visible: false)
        {

        }
    }
}
