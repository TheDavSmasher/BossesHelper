using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class EntityChain(Entity entity, bool chainPosition) : Component(active: true, visible: false)
    {
        public Entity chained = entity;

        public bool chainedPosition = chainPosition;

        public override void Update()
        {
            base.Update();
            if (chainedPosition)
            {
                chained.Position = Entity.Position + chained.Position;
            }
        }

        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            chained.RemoveSelf();
        }
    }
}
