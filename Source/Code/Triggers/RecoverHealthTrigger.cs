using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/RecoverHealthTrigger")]
    public class RecoverHealthTrigger(EntityData data, Vector2 offset, EntityID id) : Trigger(data, offset)
    {
        private readonly EntityID entityID = id;

        private readonly int healAmount = data.Int("healAmount", 1);

        private readonly bool onlyOnce = data.Bool("onlyOnce", true);

        private readonly bool permanent = data.Bool("permanent");

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Level level = SceneAs<Level>();
            if (level.Tracker.GetEntity<HealthSystemManager>() != null)
            {
                BossesHelperModule.Session.mapDamageController.RecoverHealth(healAmount);
                if (onlyOnce)
                {
                    if (permanent)
                    {
                        level.Session.DoNotLoad.Add(entityID);
                    }
                    RemoveSelf();
                }
            }
        }
    }
}
