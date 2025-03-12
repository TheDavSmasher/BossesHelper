using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;
using System;
using System.Reflection;
using Celeste.Mod.BossesHelper.Code.Helpers;
using MonoMod.Cil;

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
                fakeDeathMethods = SeparateList(data.String("fakeDeathMethods", JoinList(HealthData.fakeDeathMethods ?? []))).ToArray(),
                onDamageFunction = data.String("onDamageFunction", HealthData.onDamageFunction),
                activateInstantly = data.Bool("applySystemInstantly"),
                startVisible = data.Bool("startVisible"),
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

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (scene.Tracker.GetEntity<HealthSystemManager>() != this)
            {
                RemoveSelf();
                return;
            }
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
            if (scene.Tracker.GetEntity<HealthSystemManager>() == this)
            {
                DisableHealthSystem();
                BossesHelperModule.Session.healthData.isCreated = false;
                scene.Tracker.GetEntity<PlayerHealthBar>()?.RemoveSelf();
                scene.Tracker.GetEntity<DamageController>()?.RemoveSelf();
            }
        }

        public void EnableHealthSystem()
        {
            BossesHelperModule.Session.healthData.isEnabled = true;
            SceneAs<Level>()?.Add(new PlayerHealthBar(), new DamageController());
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

        //Fake Data Shenanigans
        private static void LoadFakeDeathHooks()
        {
            foreach (string fakeMethod in HealthData.fakeDeathMethods)
            {
                string[] opts = fakeMethod.Split(':');
                if (opts.Length != 2)
                    continue;
                Type entityType = LuaMethodWrappers.GetTypeFromString(opts[0], "");
                MethodInfo methodInfo = entityType?.GetMethod(opts[1], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (methodInfo != null)
                {
                    ILHookHelper.GenerateHookOn(fakeMethod, methodInfo, DetermineDeathCall);
                }
            }
        }

        private static void DetermineDeathCall(ILContext il)
        {
            ILCursor cursor = new(il);
            cursor.EmitDelegate(BossesHelperExports.UseFakeDeath);
            while (cursor.TryGotoNext(instr => instr.MatchRet()))
            {
                cursor.EmitDelegate(BossesHelperExports.ClearFakeDeath);
                cursor.Index++;
            }
        }

        private static void UnloadFakeDeathHooks()
        {
            foreach (string fakeMethod in HealthData.fakeDeathMethods)
            {
                ILHookHelper.DisposeHook(fakeMethod);
            }
        }
    }
}
