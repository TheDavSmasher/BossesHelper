using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    [CustomEntity("BossesHelper/HealthSystemController")]
    public class HealthSystemController : Entity
    {
        public HealthSystemController(EntityData data, Vector2 offset)
        {
            BossesHelperModule.healthData.healthBarPos = data.Position + offset;
            BossesHelperModule.healthData.iconSprite = data.Attr("healthIcon");
            BossesHelperModule.healthData.startAnim = data.Attr("healthIconCreateAnim");
            BossesHelperModule.healthData.endAnim = data.Attr("healthIconRemoveAnim");
            BossesHelperModule.healthData.iconSeparation = data.Float("healthIconSeparation");
            BossesHelperModule.healthData.globalController = data.Bool("isGlobal");
            BossesHelperModule.healthData.playerHealthVal = data.Int("playerHealth");
            BossesHelperModule.healthData.damageCooldown = data.Float("damageCooldown", 1f);
            BossesHelperModule.healthSystemController ??= this;
            BossesHelperModule.playerHealthBar ??= new();
            BossesHelperModule.playerDamageController ??= new();
        }
    }
}
