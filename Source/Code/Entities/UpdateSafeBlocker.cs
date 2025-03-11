using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    [CustomEntity("BossesHelper/UpdateSafeBlocker")]
    public class UpdateSafeBlocker : Entity
    {
        public UpdateSafeBlocker(EntityData data, Vector2 _) : this()
        {
            BossesHelperModule.Session.globalSafeGroundBlocker = data.Bool("isGlobal", BossesHelperModule.Session.globalSafeGroundBlocker);
            if (BossesHelperModule.Session.globalSafeGroundBlocker)
                AddTag(Tags.Global);
        }

        public UpdateSafeBlocker()
        {
            BossesHelperModule.Session.mapUpdateSafeBlocker = this;
            BossesHelperModule.Session.safeGroundBlockerCreated = true;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            BossesHelperModule.Session.mapUpdateSafeBlocker = null;
            BossesHelperModule.Session.safeGroundBlockerCreated = false;
        }
    }
}
