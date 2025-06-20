using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    [CustomEntity("BossesHelper/HealthSystemManager")]
    public partial class HealthSystemManager : GlobalEntity
    {
        private static BossesHelperSession.HealthSystemData HealthData => BossesHelperModule.Session.healthData;

        private PlayerHealthBar HealthBar => Scene.GetEntity<PlayerHealthBar>();

        private DamageController DamageController => Scene.GetEntity<DamageController>();

        private enum CrushEffect
        {
            PushOut, InvincibleSolid, FakeDeath, InstantDeath
        }

        private enum OffscreenEffect
        {
            BounceUp, BubbleBack, FakeDeath, InstantDeath
        }

        public enum DeathEffect
        {
            PlayerPush,
            PlayerSafe,
            FakeDeath,
            InstantDeath
        }

        public HealthSystemManager(EntityData data, Vector2 _)
            : base(data.Bool("isGlobal"))
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
                frameSprite = data.String("frameSprite", HealthData.frameSprite),
                globalController = IsGlobal,
                globalHealth = IsGlobal && data.Bool("globalHealth"),
                playerHealthVal = data.Int("playerHealth", HealthData.playerHealthVal),
                damageCooldown = data.Float("damageCooldown", HealthData.damageCooldown),
                playerOnCrush = (DeathEffect) data.Enum("crushEffect", (CrushEffect) HealthData.playerOnCrush),
                playerOffscreen = (DeathEffect) data.Enum("offscreenEffect", (OffscreenEffect) HealthData.playerOffscreen),
                fakeDeathMethods = [.. SeparateList(data.String("fakeDeathMethods", JoinList(HealthData.fakeDeathMethods)))],
                onDamageFunction = data.String("onDamageFunction", HealthData.onDamageFunction),
                activateInstantly = data.Bool("applySystemInstantly"),
                startVisible = data.Bool("startVisible"),
                removeOnDamage = data.Bool("removeOnDamage", true),
                playerBlink = data.Bool("playerBlink", true),
                playerStagger = data.Bool("playerStagger", true),
                activateFlag = data.String("activationFlag", HealthData.activateFlag),
                isEnabled = false,
                isCreated = HealthData.isCreated                
            };
            if (!HealthData.isCreated)
                ResetCurrentHealth();
            BossesHelperModule.Session.healthData.isCreated = true;
        }

        public HealthSystemManager() : base(false)
        {
            if (HealthData.isCreated && !HealthData.globalHealth)
                ResetCurrentHealth();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (HealthData.isEnabled || HealthData.activateInstantly)
                EnableHealthSystem();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (scene.GetEntity<HealthSystemManager>() != this)
                RemoveSelf();
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
            if (scene.GetEntity<HealthSystemManager>() == this)
            {
                DisableHealthSystem();
                BossesHelperModule.Session.healthData.isCreated = false;
                HealthBar?.RemoveSelf();
                DamageController?.RemoveSelf();
            }
        }

        public void EnableHealthSystem()
        {
            BossesHelperModule.Session.healthData.isEnabled = true;
            if (HealthBar == null)
                Scene.Add(new PlayerHealthBar());
            if (DamageController == null)
                Scene.Add(new DamageController());
            LoadFakeDeathHooks();
        }

        public void DisableHealthSystem()
        {
            BossesHelperModule.Session.healthData.isEnabled = false;
            RemoveSelf();
            UnloadFakeDeathHooks();
        }

        private static void ResetCurrentHealth()
        {
            BossesHelperModule.Session.currentPlayerHealth = HealthData.playerHealthVal;
        }

        private static partial void LoadFakeDeathHooks();
    }
}
