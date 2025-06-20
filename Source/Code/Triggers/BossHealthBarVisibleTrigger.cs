using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/BossHealthBarVisibleTrigger")]
    public class BossHealthBarVisibleTrigger(EntityData data, Vector2 offset, EntityID id)
        : SingleUseTrigger(data, offset, id, false, data.Bool("onlyOnce"))
    {
        private readonly bool state = data.Bool("visible");

        private readonly Vector2 node = data.Nodes[0];

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (SceneAs<Level>().Tracker.GetNearestEntity<BossHealthBar>(node) is BossHealthBar bar)
            {
                ConsumeAfter(() => bar.Visible = state);
            }
        }
    }
}
