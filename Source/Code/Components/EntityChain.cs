using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class EntityChain(Entity entity, bool chainPosition, bool removeTogether = false) : Component(active: true, visible: false)
    {
        public Entity chained = entity;

        public bool chainedPosition = chainPosition;

        public Vector2 positionOffset;

        private readonly bool removeTogether = removeTogether;

        public override void Added(Entity entity)
        {
            base.Added(entity);
            positionOffset = chained.Position - Entity.Position;
        }

        public override void Update()
        {
            if (chainedPosition)
            {
                chained.Position = Entity.Position + positionOffset;
            }
        }

        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            if (removeTogether)
                chained.RemoveSelf();
        }
    }
}
