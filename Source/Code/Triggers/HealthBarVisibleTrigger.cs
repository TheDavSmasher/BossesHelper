using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/HealthBarVisibleTrigger")]
    public class HealthBarVisibleTrigger : Trigger
    {
        private readonly bool state;

        public HealthBarVisibleTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            state = data.Bool("visible");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            SceneAs<Level>().Tracker.GetEntity<DamageHealthBar>().HealthVisible = state;
        }
    }
}
