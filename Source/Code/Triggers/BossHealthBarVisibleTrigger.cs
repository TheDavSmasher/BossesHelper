using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/BossHealthBarVisibleTrigger")]
    public class BossHealthBarVisibleTrigger : Trigger
    {
        private readonly bool state;

        private readonly bool onlyOnce;

        private readonly Vector2 node;

        public BossHealthBarVisibleTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            state = data.Bool("visible");
            onlyOnce = data.Bool("onlyOnce");
            node = data.Nodes[0];
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            BossHealthBar bar = SceneAs<Level>().Tracker.GetNearestEntity<BossHealthBar>(node);
            if (bar != null)
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
