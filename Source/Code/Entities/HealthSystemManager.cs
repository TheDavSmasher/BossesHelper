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
            BossesHelperModule.healthData.iconSprite = data.Attr("healthIcon", "bird");
            BossesHelperModule.healthData.startAnim = data.Attr("healthIconCreateAnim", "jump");
            BossesHelperModule.healthData.endAnim = data.Attr("healthIconRemoveAnim", "hurt");
            Vector2 screenPosition = new Vector2(data.Float("healthIconScreenX"), data.Float("healthIconScreenY"));
            BossesHelperModule.healthData.healthBarPos = screenPosition;
            Vector2 iconScale = new Vector2(data.Float("healthIconScaleX", 1), data.Float("healthIconScaleY", 1));
            BossesHelperModule.healthData.healthIconScale = iconScale;
            BossesHelperModule.healthData.iconSeparation = data.Float("healthIconSeparation", 10f);
            BossesHelperModule.healthData.globalController = data.Bool("isGlobal");
            BossesHelperModule.healthData.globalHealth = data.Bool("globalHealth");
            BossesHelperModule.healthData.playerHealthVal = data.Int("playerHealth", 3);
            BossesHelperModule.healthData.damageCooldown = data.Float("damageCooldown", 1f);
            BossesHelperModule.healthData.applySystemInstantly = data.Bool("applySystemInstantly");
            BossesHelperModule.healthData.playerOnCrush = data.Enum<CrushEffect>("crushEffect", CrushEffect.InstantDeath);
            activateFlag = data.Attr("activationFlag");
            enabled = false;
            if (BossesHelperModule.healthData.globalController)
                AddTag(Tags.Global);
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
            if (!enabled && !string.IsNullOrEmpty(activateFlag) && SceneAs<Level>().Session.GetFlag(activateFlag))
                EnableHealthSystem();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            BossesHelperModule.playerHealthBar.RemoveSelf();
            BossesHelperModule.playerHealthBar = null;
            BossesHelperModule.playerDamageController.RemoveSelf();
            BossesHelperModule.playerDamageController = null;
        }

        public void EnableHealthSystem()
        {
            enabled = true;
            Level level = SceneAs<Level>();
            if (level != null)
            {
                BossesHelperModule.playerHealthBar ??= new();
                BossesHelperModule.playerDamageController ??= new();
                level.Add(BossesHelperModule.playerHealthBar);
                level.Add(BossesHelperModule.playerDamageController);
            }
        }
    }
}
