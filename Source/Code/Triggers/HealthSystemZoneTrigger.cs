using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/HealthSystemZoneTrigger")]
    public class HealthSystemZoneTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
    {
        private readonly bool enableState = data.Bool("enableState");

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (SceneAs<Level>().GetEntity<HealthSystemManager>() is HealthSystemManager manager)
            {
                if (enableState)
                    manager.EnableHealthSystem();
                else
                    manager.DisableHealthSystem();
            }
        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (SceneAs<Level>().GetEntity<HealthSystemManager>() is HealthSystemManager manager)
            {
                if (enableState)
                    manager.EnableHealthSystem();
                else
                    manager.DisableHealthSystem();
            }
        }
    }
}
