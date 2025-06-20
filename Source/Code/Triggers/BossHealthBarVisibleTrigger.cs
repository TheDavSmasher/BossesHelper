using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/BossHealthBarVisibleTrigger")]
    public class BossHealthBarVisibleTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
    {
        private readonly bool state = data.Bool("visible");

        private readonly bool onlyOnce = data.Bool("onlyOnce");

        private readonly Vector2 node = data.Nodes[0];

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (SceneAs<Level>().Tracker.GetNearestEntity<BossHealthBar>(node) is BossHealthBar bar)
            {
                bar.Visible = state;
                if (onlyOnce)
                {
                    RemoveSelf();
                }
            }
        }
    }
}
