﻿using Celeste.Mod.Entities;
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

        public HealthSystemManager(EntityData data, Vector2 _)
        {
            BossesHelperModule.Session.mapHealthSystemManager = this;
            BossesHelperModule.Session.healthData.isCreated = true;
            BossesHelperModule.Session.healthData.iconSprite = data.String("healthIcons", HealthData.iconSprite);
            BossesHelperModule.Session.healthData.startAnim = data.String("healthIconsCreateAnim", HealthData.startAnim);
            BossesHelperModule.Session.healthData.endAnim = data.String("healthIconsRemoveAnim", HealthData.endAnim);
            Vector2 screenPosition = new Vector2(data.Float("healthIconsScreenX", HealthData.healthBarPos.X), data.Float("healthIconsScreenY", HealthData.healthBarPos.Y));
            BossesHelperModule.Session.healthData.healthBarPos = screenPosition;
            Vector2 iconScale = new Vector2(data.Float("healthIconsScaleX", HealthData.healthIconScale.X), data.Float("healthIconsScaleY", HealthData.healthIconScale.Y));
            BossesHelperModule.Session.healthData.healthIconScale = iconScale;
            BossesHelperModule.Session.healthData.iconSeparation = data.String("healthIconsSeparation", HealthData.iconSeparation);
            BossesHelperModule.Session.healthData.globalController = data.Bool("isGlobal");
            BossesHelperModule.Session.healthData.globalHealth = data.Bool("globalHealth");
            BossesHelperModule.Session.healthData.playerHealthVal = data.Int("playerHealth", HealthData.playerHealthVal);
            BossesHelperModule.Session.healthData.damageCooldown = data.Float("damageCooldown", HealthData.damageCooldown);
            BossesHelperModule.Session.healthData.playerOnCrush = data.Enum<CrushEffect>("crushEffect", HealthData.playerOnCrush);
            BossesHelperModule.Session.healthData.playerOffscreen = data.Enum<OffscreenEffect>("offscreenEffect", HealthData.playerOffscreen);
            BossesHelperModule.Session.healthData.onDamageFunction = data.String("onDamageFunction", HealthData.onDamageFunction);
            BossesHelperModule.Session.healthData.activateInstantly = data.Bool("applySystemInstantly");
            BossesHelperModule.Session.healthData.startVisible = data.Bool("startVisible");
            BossesHelperModule.Session.healthData.playerBlink = data.Bool("playerBlink", true);
            BossesHelperModule.Session.healthData.playerStagger = data.Bool("playerStagger", true);
            BossesHelperModule.Session.healthData.activateFlag = data.String("activationFlag", HealthData.activateFlag);
            BossesHelperModule.Session.healthData.isEnabled = false;
            if (BossesHelperModule.Session.healthData.globalController)
                AddTag(Tags.Global);
        }

        public HealthSystemManager() //Will only be called if already created prior but is currently null
        {
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
            if (!HealthData.isEnabled && !string.IsNullOrEmpty(HealthData.activateFlag) && SceneAs<Level>().Session.GetFlag(HealthData.activateFlag))
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
