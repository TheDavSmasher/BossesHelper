using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/RecoverHealthTrigger")]
    public class RecoverHealthTrigger : Trigger
    {
        private readonly EntityID entityID;

        private readonly int healAmount;

        private readonly bool onlyOnce;

        private readonly bool permanent;

        public RecoverHealthTrigger(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset)
        {
            entityID = id;
            healAmount = data.Int("healAmount", 1);
            onlyOnce = data.Bool("onlyOnce", true);
            permanent = data.Bool("permanent");
        }

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
