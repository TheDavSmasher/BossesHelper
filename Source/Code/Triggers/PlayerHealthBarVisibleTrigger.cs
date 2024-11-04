using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/PlayerHealthBarVisibleTrigger")]
    public class PlayerHealthBarVisibleTrigger : Trigger
    {
        private readonly bool state;

        private readonly bool onlyOnce;

        public PlayerHealthBarVisibleTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            state = data.Bool("visible");
            onlyOnce = data.Bool("onlyOnce");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            DamageHealthBar bar = SceneAs<Level>().Tracker.GetEntity<DamageHealthBar>();
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
