using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;
using Celeste.Mod.BossesHelper.Code.Components;

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

        private HealthSystemManager(bool resetHealth, bool isGlobal, int setHealthTo = 0, string activateFlag = null)
            : base(isGlobal)
        {
            if (activateFlag != null)
            {
                BossesHelperModule.Session.healthData.activateFlag = activateFlag;
            }
            if (setHealthTo > 0)
            {
                BossesHelperModule.Session.healthData.playerHealthVal = setHealthTo;
            }
            if (resetHealth)
            {
                BossesHelperModule.Session.currentPlayerHealth = HealthData.playerHealthVal;
            }
            Add(new EntityFlagger(HealthData.activateFlag, _ => EnableHealthSystem()));
        }

        public HealthSystemManager(EntityData data, Vector2 _)
            : this(HealthData.isCreated, data.Bool("isGlobal"), data.Int("playerHealth"), data.String("activationFlag"))
        {
            UpdateSessionData(data);
        }

        public HealthSystemManager() : this(!HealthData.globalHealth, HealthData.globalController) { }

        public void UpdateSessionData(EntityData data)
        {
            ChangeGlobalState(data.Bool("isGlobal"));
            BossesHelperModule.Session.healthData = new()
            {
                iconSprite = data.String("healthIcons", HealthData.iconSprite),
                startAnim = data.String("healthIconsCreateAnim", HealthData.startAnim),
                endAnim = data.String("healthIconsRemoveAnim", HealthData.endAnim),
                healthBarPos = new(
                    data.Float("healthIconsScreenX", HealthData.healthBarPos.X),
                    data.Float("healthIconsScreenY", HealthData.healthBarPos.Y)
                ),
                healthIconScale = new(
                    data.Float("healthIconsScaleX", HealthData.healthIconScale.X),
                    data.Float("healthIconsScaleY", HealthData.healthIconScale.Y)
                ),
                iconSeparation = data.String("healthIconsSeparation", HealthData.iconSeparation),
                frameSprite = data.String("frameSprite", HealthData.frameSprite),
                globalController = IsGlobal,
                globalHealth = IsGlobal && data.Bool("globalHealth"),
                playerHealthVal = data.Int("playerHealth", HealthData.playerHealthVal),
                damageCooldown = data.Float("damageCooldown", HealthData.damageCooldown),
                playerOnCrush = (DeathEffect) data.Enum("crushEffect", (CrushEffect) HealthData.playerOnCrush),
                playerOffscreen = (DeathEffect) data.Enum("offscreenEffect", (OffscreenEffect) HealthData.playerOffscreen),
                fakeDeathMethods = data.String("fakeDeathMethods", HealthData.fakeDeathMethods),
                onDamageFunction = data.String("onDamageFunction", HealthData.onDamageFunction),
                activateInstantly = data.Bool("applySystemInstantly"),
                startVisible = data.Bool("startVisible"),
                removeOnDamage = data.Bool("removeOnDamage", true),
                playerBlink = data.Bool("playerBlink", true),
                playerStagger = data.Bool("playerStagger", true),
                activateFlag = data.String("activationFlag", HealthData.activateFlag),
                isEnabled = false,
                isCreated = true
            };
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (scene.GetEntity<HealthSystemManager>() != this)
            {
                RemoveSelf();
            }
            else if (HealthData.isEnabled || HealthData.activateInstantly)
            {
                EnableHealthSystem();
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (scene.GetEntity<HealthSystemManager>() == this)
            {
                DisableHealthSystem();
                BossesHelperModule.Session.healthData.isCreated = false;
            }
        }

        public void EnableHealthSystem(bool withHooks = true)
        {
            BossesHelperModule.Session.healthData.isEnabled = true;
            if (HealthBar == null)
                Scene.Add(new PlayerHealthBar());
            if (DamageController == null)
                Scene.Add(new DamageController());
            if (withHooks)
                LoadFakeDeathHooks();
            Get<EntityFlagger>()?.RemoveSelf();
        }

        public void DisableHealthSystem(bool withHooks = true)
        {
            BossesHelperModule.Session.healthData.isEnabled = false;
            if (withHooks)
                UnloadFakeDeathHooks();
            HealthBar?.RemoveSelf();
            DamageController?.RemoveSelf();
        }

        private static partial void LoadFakeDeathHooks();
        private static partial void UnloadFakeDeathHooks();
    }
}
