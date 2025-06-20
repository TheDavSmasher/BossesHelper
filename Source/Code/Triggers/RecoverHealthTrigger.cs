using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/RecoverHealthTrigger")]
    public class RecoverHealthTrigger(EntityData data, Vector2 offset, EntityID entityID)
        : SingleUseTrigger(data, offset, entityID, data.Bool("permanent"))
    {
        private readonly int healAmount = data.Int("healAmount", 1);

        private readonly bool onlyOnce = data.Bool("onlyOnce", true);

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (Scene.GetEntity<DamageController>() is DamageController controller)
            {
                controller.RecoverHealth(healAmount);
                if (onlyOnce)
                {
                    RemoveSelf();
                }
            }
        }
    }
}
