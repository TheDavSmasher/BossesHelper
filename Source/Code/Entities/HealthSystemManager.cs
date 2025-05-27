using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    [CustomEntity("BossesHelper/HealthSystemManager")]
    public partial class HealthSystemManager : Entity
    {
        private static BossesHelperSession.HealthSystemData HealthData => BossesHelperModule.Session.healthData;

        public enum CrushEffect
        {
            PushOut,
            InvincibleSolid,
            FakeDeath,
            InstantDeath
        }

        public enum OffscreenEffect
        {
            BounceUp,
            BubbleBack,
            FakeDeath,
            InstantDeath
        }

        public HealthSystemManager(EntityData data, Vector2 _)
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
                globalController = data.Bool("isGlobal"),
                globalHealth = HealthData.globalController && data.Bool("globalHealth"),
                playerHealthVal = data.Int("playerHealth", HealthData.playerHealthVal),
                damageCooldown = data.Float("damageCooldown", HealthData.damageCooldown),
                playerOnCrush = data.Enum("crushEffect", HealthData.playerOnCrush),
                playerOffscreen = data.Enum("offscreenEffect", HealthData.playerOffscreen),
                fakeDeathMethods = [.. SeparateList(data.String("fakeDeathMethods", JoinList(HealthData.fakeDeathMethods ?? [])))],
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
            ResetCurrentHealth(!HealthData.isCreated);
            BossesHelperModule.Session.healthData.isCreated = true;
            if (HealthData.globalController)
                AddTag(Tags.Global);
        }

        public HealthSystemManager()
        {
            ResetCurrentHealth(HealthData.isCreated && !HealthData.globalHealth);
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
                scene.GetEntity<PlayerHealthBar>()?.RemoveSelf();
                scene.GetEntity<DamageController>()?.RemoveSelf();
            }
        }

        public void EnableHealthSystem()
        {
            BossesHelperModule.Session.healthData.isEnabled = true;
            if (Scene.GetEntity<PlayerHealthBar>() == null)
                Scene.Add(new PlayerHealthBar());
            if (Scene.GetEntity<DamageController>() == null)
                Scene.Add(new DamageController());
            LoadFakeDeathHooks();
        }

        public void DisableHealthSystem()
        {
            BossesHelperModule.Session.healthData.isEnabled = false;
            RemoveSelf();
            UnloadFakeDeathHooks();
        }

        private static void ResetCurrentHealth(bool reset)
        {
            if (reset)
            {
                BossesHelperModule.Session.currentPlayerHealth = BossesHelperModule.Session.healthData.playerHealthVal;
            }
        }

        private static partial void LoadFakeDeathHooks();
    }
}
