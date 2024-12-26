using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    public class UpdateSafeBlocker : Entity
    {
        public UpdateSafeBlocker(EntityData data, Vector2 _)
        {
            BossesHelperModule.Session.globalSafeGroundBlocker = data.Bool("isGlobal", BossesHelperModule.Session.globalSafeGroundBlocker);
            if (BossesHelperModule.Session.globalSafeGroundBlocker)
                AddTag(Tags.Global);
        }

        public UpdateSafeBlocker() //Will only be called if already created prior but is currently null
        {
            BossesHelperModule.Session.mapUpdateSafeBlocker = this;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
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
