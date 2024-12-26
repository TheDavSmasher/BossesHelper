using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [CustomEntity("BossesHelper/UpdateSafeUnblocker")]
    public class UpdateSafeUnblocker : Entity
    {
        private readonly EntityID ID;

        private readonly bool onlyOnce;

        private Level level;

        public UpdateSafeUnblocker(EntityData data, Vector2 _, EntityID id)
        {
            ID = id;
            onlyOnce = data.Bool("onlyOnce");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = scene as Level;
            BossesHelperModule.Session.mapUpdateSafeBlocker?.RemoveSelf();
            RemoveSelf();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (onlyOnce)
            {
                level.Session.DoNotLoad.Add(ID); 
            }
        }
    }
}
