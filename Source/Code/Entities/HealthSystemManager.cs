using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    [CustomEntity("BossesHelper/HealthSystemManager")]
    public class HealthSystemManager : Entity
    {
        public enum CrushEffect
        {
            PushOut,
            InvincibleSolid,
            InstantDeath
        }

        public bool enabled;

        private readonly string activateFlag;

        public HealthSystemManager(EntityData data, Vector2 _)
        {
            if (BossesHelperModule.Session.mapHealthSystemManager == null)
            {
                BossesHelperModule.Session.mapHealthSystemManager = this;
                BossesHelperModule.Session.healthData.iconSprite = data.Attr("healthIcon", "bird");
                BossesHelperModule.Session.healthData.startAnim = data.Attr("healthIconCreateAnim", "jump");
                BossesHelperModule.Session.healthData.endAnim = data.Attr("healthIconRemoveAnim", "hurt");
                Vector2 screenPosition = new Vector2(data.Float("healthIconScreenX"), data.Float("healthIconScreenY"));
                BossesHelperModule.Session.healthData.healthBarPos = screenPosition;
                Vector2 iconScale = new Vector2(data.Float("healthIconScaleX", 1), data.Float("healthIconScaleY", 1));
                BossesHelperModule.Session.healthData.healthIconScale = iconScale;
                BossesHelperModule.Session.healthData.iconSeparation = data.Float("healthIconSeparation", 10f);
                BossesHelperModule.Session.healthData.globalController = data.Bool("isGlobal");
                BossesHelperModule.Session.healthData.globalHealth = data.Bool("globalHealth");
                BossesHelperModule.Session.healthData.playerHealthVal = data.Int("playerHealth", 3);
                BossesHelperModule.Session.healthData.damageCooldown = data.Float("damageCooldown", 1f);
                BossesHelperModule.Session.healthData.applySystemInstantly = data.Bool("applySystemInstantly");
                BossesHelperModule.Session.healthData.playerOnCrush = data.Enum<CrushEffect>("crushEffect", CrushEffect.InstantDeath);
                activateFlag = data.Attr("activationFlag");
                enabled = false;
                if (BossesHelperModule.Session.healthData.globalController)
                    AddTag(Tags.Global);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (enabled || BossesHelperModule.Session.healthData.applySystemInstantly)
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
            BossesHelperModule.Session.mapHealthBar.RemoveSelf();
            BossesHelperModule.Session.mapHealthBar = null;
            BossesHelperModule.Session.mapDamageController.RemoveSelf();
            BossesHelperModule.Session.mapDamageController = null;
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
