using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    internal class EntityChainComp : Component
    {
        public Entity chained;

        public bool chainedPosition;

        public EntityChainComp(Entity entity, bool chainPosition, bool active, bool visible) : base(active, visible)
        {
            this.chained = entity;
            this.chainedPosition = chainPosition;
        }

        public override void Update()
        {
            base.Update();
            if (chainedPosition)
            {
                chained.Position = Entity.Position + chained.Position;
            }
        }
    }
}
