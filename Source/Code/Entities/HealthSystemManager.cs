using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    [CustomEntity("BossesHelper/HealthSystemManager")]
    public class HealthSystemManager : Entity
    {
        public bool enabled;

        private readonly string activateFlag;

        public HealthSystemManager(EntityData data, Vector2 offset)
        {
            BossesHelperModule.playerHealthBar ??= new();
            BossesHelperModule.playerDamageController ??= new();
            if (BossesHelperModule.healthSystemManager == null)
            {
                BossesHelperModule.healthSystemManager = this;
                BossesHelperModule.healthData.healthBarPos = data.Position + offset;
                BossesHelperModule.healthData.iconSprite = data.Attr("healthIcon");
                BossesHelperModule.healthData.startAnim = data.Attr("healthIconCreateAnim");
                BossesHelperModule.healthData.endAnim = data.Attr("healthIconRemoveAnim");
                BossesHelperModule.healthData.iconSeparation = data.Float("healthIconSeparation");
                BossesHelperModule.healthData.globalController = data.Bool("isGlobal");
                BossesHelperModule.healthData.globalHealth = data.Bool("globalHealth");
                BossesHelperModule.healthData.playerHealthVal = data.Int("playerHealth", 3);
                BossesHelperModule.healthData.damageCooldown = data.Float("damageCooldown", 1f);
                BossesHelperModule.healthData.applySystemInstantly = data.Bool("applySystemInstantly");
            }
            activateFlag = data.Attr("activationFlag");
            enabled = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (BossesHelperModule.healthData.applySystemInstantly)
                EnableHealthSystem();
        }

        public override void Update()
        {
            base.Update();
            if (SceneAs<Level>().Session.GetFlag(activateFlag))
                EnableHealthSystem();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            BossesHelperModule.playerHealthBar.RemoveSelf();
            BossesHelperModule.playerDamageController.RemoveSelf();
        }

        public void EnableHealthSystem()
        {
            enabled = true;
            Level level = SceneAs<Level>();
            if (level != null)
            {
                level.Add(BossesHelperModule.playerHealthBar);
                level.Add(BossesHelperModule.playerDamageController);
            }
        }
    }
}
