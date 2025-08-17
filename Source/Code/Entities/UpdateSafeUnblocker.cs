using Celeste.Mod.BossesHelper.Code.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [CustomEntity("BossesHelper/UpdateSafeUnblocker")]
    public class UpdateSafeUnblocker(EntityData data, Vector2 pos, EntityID id) : Entity(pos)
    {
        private readonly bool onlyOnce = data.Bool("onlyOnce");

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.GetEntity<UpdateSafeBlocker>()?.RemoveSelf();
            RemoveSelf();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (onlyOnce)
            {
                scene.DoNotLoad(id);
            }
        }
    }
}
