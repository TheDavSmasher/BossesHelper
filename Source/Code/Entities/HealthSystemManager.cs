using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    [CustomEntity("BossesHelper/HealthSystemManager")]
    public class HealthSystemManager : Entity
    {
        private static BossesHelperSession.HealthSystemData healthData => BossesHelperModule.Session.healthData;

        public enum CrushEffect
        {
            PushOut,
            InvincibleSolid,
            InstantDeath
        }

        public bool enabled;

        private readonly bool activateInstantly;

        private readonly string activateFlag;

        public HealthSystemManager(EntityData data, Vector2 _)
        {
            BossesHelperModule.Session.mapHealthSystemManager ??= this;
            BossesHelperModule.Session.healthData.iconSprite = data.Attr("healthIcon", healthData.iconSprite);
            BossesHelperModule.Session.healthData.startAnim = data.Attr("healthIconCreateAnim", healthData.startAnim);
            BossesHelperModule.Session.healthData.endAnim = data.Attr("healthIconRemoveAnim", healthData.endAnim);
            Vector2 screenPosition = new Vector2(data.Float("healthIconScreenX", healthData.healthBarPos.X), data.Float("healthIconScreenY", healthData.healthBarPos.Y));
            BossesHelperModule.Session.healthData.healthBarPos = screenPosition;
            Vector2 iconScale = new Vector2(data.Float("healthIconScaleX", healthData.healthIconScale.X), data.Float("healthIconScaleY", healthData.healthIconScale.Y));
            BossesHelperModule.Session.healthData.healthIconScale = iconScale;
            BossesHelperModule.Session.healthData.iconSeparation = data.Float("healthIconSeparation", healthData.iconSeparation);
            BossesHelperModule.Session.healthData.globalController = data.Bool("isGlobal");
            BossesHelperModule.Session.healthData.globalHealth = data.Bool("globalHealth");
            BossesHelperModule.Session.healthData.playerHealthVal = data.Int("playerHealth", healthData.playerHealthVal);
            BossesHelperModule.Session.healthData.damageCooldown = data.Float("damageCooldown", healthData.damageCooldown);
            BossesHelperModule.Session.healthData.playerOnCrush = data.Enum<CrushEffect>("crushEffect", healthData.playerOnCrush);
            activateInstantly = data.Bool("applySystemInstantly");
            activateFlag = data.Attr("activationFlag");
            enabled = false;
            if (BossesHelperModule.Session.healthData.globalController)
                AddTag(Tags.Global);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (enabled || activateInstantly)
                EnableHealthSystem();
        }

        public override void Update()
        {
            base.Update();
            if (!enabled && !string.IsNullOrEmpty(activateFlag) && SceneAs<Level>().Session.GetFlag(activateFlag))
                EnableHealthSystem();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            BossesHelperModule.Session.mapHealthSystemManager = null;
            if (BossesHelperModule.Session.mapHealthBar != null)
            {
                BossesHelperModule.Session.mapHealthBar.RemoveSelf();
                BossesHelperModule.Session.mapHealthBar = null;
            }
            if (BossesHelperModule.Session.mapDamageController != null)
            {
                BossesHelperModule.Session.mapDamageController.RemoveSelf();
                BossesHelperModule.Session.mapDamageController = null;
            }
        }

        public void DisableHealthSystem()
        {
            enabled = false;
            RemoveSelf();
        }

        public void EnableHealthSystem()
        {
            enabled = true;
            Level level = SceneAs<Level>();
            if (level != null)
            {
                BossesHelperModule.Session.mapHealthBar ??= new();
                BossesHelperModule.Session.mapDamageController ??= new();
                level.Add(BossesHelperModule.Session.mapHealthBar);
                level.Add(BossesHelperModule.Session.mapDamageController);
            }
        }
    }
}
