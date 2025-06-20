using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/HealthSystemZoneTrigger")]
    public class HealthSystemZoneTrigger(EntityData data, Vector2 offset) : HealthEnablerTrigger(data, offset)
    {
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            ChangeManagerState();
        }
    }
}
