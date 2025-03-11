using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    [CustomEntity("BossesHelper/HealthSystemManager")]
    public class HealthSystemManager : Entity
    {
        private static BossesHelperSession.HealthSystemData HealthData => BossesHelperModule.Session.healthData;

        public enum CrushEffect
        {
            PushOut,
            InvincibleSolid,
            InstantDeath
        }

        public enum OffscreenEffect
        {
            BounceUp,
            BubbleBack,
            InstantDeath
        }

        public HealthSystemManager(EntityData data, Vector2 _) : this()
        {
            Vector2 screenPosition = new(data.Float("healthIconsScreenX", HealthData.healthBarPos.X), data.Float("healthIconsScreenY", HealthData.healthBarPos.Y));
            Vector2 iconScale = new(data.Float("healthIconsScaleX", HealthData.healthIconScale.X), data.Float("healthIconsScaleY", HealthData.healthIconScale.Y));
            BossesHelperModule.Session.healthData = new()
            {
                iconSprite = data.String("healthIcons", HealthData.iconSprite),
                startAnim = data.String("healthIconsCreateAnim", HealthData.startAnim),
                endAnim = data.String("healthIconsRemoveAnim", HealthData.endAnim),
                healthBarPos = screenPosition,
                healthIconScale = iconScale,
                iconSeparation = data.String("healthIconsSeparation", HealthData.iconSeparation),
                globalController = data.Bool("isGlobal"),
                globalHealth = HealthData.globalController ? data.Bool("globalHealth") : false,
                playerHealthVal = data.Int("playerHealth", HealthData.playerHealthVal),
                damageCooldown = data.Float("damageCooldown", HealthData.damageCooldown),
                playerOnCrush = data.Enum<CrushEffect>("crushEffect", HealthData.playerOnCrush),
                playerOffscreen = data.Enum<OffscreenEffect>("offscreenEffect", HealthData.playerOffscreen),
                fakeDeathEntities = SeparateList(data.String("fakeDeathEntities", JoinList(HealthData.fakeDeathEntities))).ToArray(),
                onDamageFunction = data.String("onDamageFunction", HealthData.onDamageFunction),
                activateInstantly = data.Bool("applySystemInstantly"),
                startVisible = data.Bool("startVisible"),
                playerBlink = data.Bool("playerBlink", true),
                playerStagger = data.Bool("playerStagger", true),
                activateFlag = data.String("activationFlag", HealthData.activateFlag),
                isEnabled = false
            };
            ResetCurrentHealth(!HealthData.isCreated);
            BossesHelperModule.Session.healthData.isCreated = true;
            if (HealthData.globalController)
                AddTag(Tags.Global);
        }

        public HealthSystemManager()
        {
            ResetCurrentHealth(HealthData.isCreated && !HealthData.globalHealth);
            BossesHelperModule.Session.mapHealthSystemManager = this;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (HealthData.isEnabled || HealthData.activateInstantly)
                EnableHealthSystem();
        }

        public override void Update()
        {
            base.Update();
            if (!HealthData.isEnabled && SceneAs<Level>().Session.GetFlag(HealthData.activateFlag))
                EnableHealthSystem();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            BossesHelperModule.Session.mapHealthSystemManager = null;
            BossesHelperModule.Session.healthData.isCreated = false;
            BossesHelperModule.Session.mapHealthBar?.RemoveSelf();
            BossesHelperModule.Session.mapDamageController?.RemoveSelf();
        }

        public void DisableHealthSystem()
        {
            BossesHelperModule.Session.healthData.isEnabled = false;
            RemoveSelf();
        }

        public void EnableHealthSystem()
        {
            BossesHelperModule.Session.healthData.isEnabled = true;
            SceneAs<Level>()?.Add(BossesHelperModule.Session.mapHealthBar ??= new(), BossesHelperModule.Session.mapDamageController ??= new());
        }

        private static void ResetCurrentHealth(bool reset)
        {
            if (reset)
            {
                BossesHelperModule.Session.currentPlayerHealth = BossesHelperModule.Session.healthData.playerHealthVal;
            }
        }
    }
}
