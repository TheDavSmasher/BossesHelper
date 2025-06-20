using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/PlayerHealthBarVisibleTrigger")]
    public class PlayerHealthBarVisibleTrigger(EntityData data, Vector2 offset, EntityID id)
        : SingleUseTrigger(data, offset, id, false, data.Bool("onlyOnce"))
    {
        private readonly bool state = data.Bool("visible");

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (Scene.GetEntity<PlayerHealthBar>() is PlayerHealthBar bar)
            {
                ConsumeAfter(() => bar.Visible = state);
            }
        }
    }
}
