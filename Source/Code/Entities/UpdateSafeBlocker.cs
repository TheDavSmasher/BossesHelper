using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    [CustomEntity("BossesHelper/UpdateSafeBlocker")]
    public class UpdateSafeBlocker : GlobalEntity
    {
        public UpdateSafeBlocker(EntityData data, Vector2 _)
            : this(data.Bool("isGlobal", BossesHelperModule.Session.globalSafeGroundBlocker))
        {
            BossesHelperModule.Session.globalSafeGroundBlocker = IsGlobal;
        }

        public UpdateSafeBlocker(bool isGlobal = false) : base(isGlobal)
        {
            BossesHelperModule.Session.safeGroundBlockerCreated = true;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (scene.GetEntity<UpdateSafeBlocker>() != this)
                RemoveSelf();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            BossesHelperModule.Session.safeGroundBlockerCreated = false;
        }
    }
}
