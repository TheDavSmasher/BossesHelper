using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Helpers;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/HealthEnableTrigger")]
    public class HealthEnablerTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
    {
        private readonly bool enableState = data.Bool("enableState");

        private readonly bool pauseState = data.Bool("pauseState");

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            ChangeManagerState();
        }

        protected void ChangeManagerState()
        {
            if (SceneAs<Level>().GetEntity<HealthSystemManager>() is HealthSystemManager manager)
            {
                if (pauseState)
                    manager.SetActiveState(enableState);
                else
                    manager.SetEnableState(enableState);
            }
        }
    }
}
