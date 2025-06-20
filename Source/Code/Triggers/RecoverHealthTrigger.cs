using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Helpers;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/RecoverHealthTrigger")]
    public class RecoverHealthTrigger(EntityData data, Vector2 offset, EntityID entityID) : Trigger(data, offset)
    {
        private readonly int healAmount = data.Int("healAmount", 1);

        private readonly bool onlyOnce = data.Bool("onlyOnce", true);

        private readonly bool permanent = data.Bool("permanent");

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (Scene.GetEntity<DamageController>() is DamageController controller)
            {
                controller.RecoverHealth(healAmount);
                if (onlyOnce)
                {
                    if (permanent)
                    {
                        SceneAs<Level>().Session.DoNotLoad.Add(entityID);
                    }
                    RemoveSelf();
                }
            }
        }
    }
}
