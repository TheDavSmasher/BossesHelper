using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class EntityColliderComponent(Collider collider = null) : Component(active: true, visible: false)
    {
        public Collider Collider = collider;
    }
}
