using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class EntityColliderCast(string entityType, Collider collider = null) : Component(active: true, visible: false)
    {
        public readonly string entityType = entityType;

        public Collider Collider = collider;
    }
}
