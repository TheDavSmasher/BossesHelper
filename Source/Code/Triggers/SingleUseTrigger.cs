using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    public abstract class SingleUseTrigger(EntityData data, Vector2 offset, EntityID id, bool permanent = false)
            : Trigger(data, offset)
    {
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (permanent)
            {
                SceneAs<Level>().Session.DoNotLoad.Add(id);
            }
        }
    }
}
