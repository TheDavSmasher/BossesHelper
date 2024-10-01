﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Other;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Monocle;
using Celeste.Mod.BossesHelper.Code.Entities;

namespace Celeste.Mod.BossesHelper;

public class BossesHelperModule : EverestModule {
    public static BossesHelperModule Instance { get; private set; }

    // Store Settings
    public override Type SettingsType => typeof(BossesHelperSettings);
    public static BossesHelperSettings Settings => (BossesHelperSettings) Instance._Settings;

    // Store Session
    public override Type SessionType => typeof(BossesHelperSession);
    public static BossesHelperSession Session => (BossesHelperSession) Instance._Session;

    // Store Save Data
    public override Type SaveDataType => typeof(BossesHelperSaveData);
    public static BossesHelperSaveData SaveData => (BossesHelperSaveData) Instance._SaveData;

    public static DamageController playerDamageController;
    
    public static DamageHealthBar playerHealthBar;

    public struct HealthSystemData
    {
        public EntityData entityDataUsed;

        public int playerHealthVal;

        public string iconSprite;

        public string startAnim;

        public string endAnim;

        public float iconSeparation;

        public Vector2 healthBarPos;

        public Vector2 healthIconScale;

        public float damageCooldown;

        public bool globalController;

        public bool globalHealth;

        public bool applySystemInstantly;

        public HealthSystemManager.CrushEffect playerOnCrush;
    }

    public static HealthSystemData healthData;

    public BossesHelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(BossesHelperModule), LogLevel.Info);
        Logger.Log("BossesHelper", "BossesHelper Loaded!");
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(BossesHelperModule), LogLevel.Info);
#endif
    }

    public override void LoadContent(bool firstLoad)
    {
        base.LoadContent(firstLoad);
        BossEvent.WarmUp();
    }

    public override void Load() {
        On.Celeste.Level.LoadLevel += new On.Celeste.Level.hook_LoadLevel(SetStartingHealth);
        On.Celeste.Player.OnSquish += new On.Celeste.Player.hook_OnSquish(ApplyUserCrush);
        On.Celeste.Player.Die += new On.Celeste.Player.hook_Die(OnPlayerCollide);
    }

    public override void Unload() {
        On.Celeste.Level.LoadLevel -= new On.Celeste.Level.hook_LoadLevel(SetStartingHealth);
        On.Celeste.Player.OnSquish -= new On.Celeste.Player.hook_OnSquish(ApplyUserCrush);
        On.Celeste.Player.Die -= new On.Celeste.Player.hook_Die(OnPlayerCollide);
    }

    public static void SetStartingHealth(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes intro, bool fromLoader = false)
    {
        orig(self, intro, fromLoader);
        HealthSystemManager healthSystemManager = self.Tracker.GetEntity<HealthSystemManager>();
        if (healthSystemManager == null || !healthSystemManager.enabled)
        {
            return;
        }
        if (healthData.globalController)
        {
            if ((intro == Player.IntroTypes.Transition && !healthData.globalHealth) || intro == Player.IntroTypes.Respawn)
            {
                playerDamageController.health = healthData.playerHealthVal;
                playerHealthBar.RefillHealth();
            }
        }
        Player entity = Engine.Scene.Tracker.GetEntity<Player>();
        if (entity != null)
        {
            entity.Sprite.Visible = true;
            entity.Hair.Visible = true;
        }
    }

    public static void ApplyUserCrush(On.Celeste.Player.orig_OnSquish orig, Player self, CollisionData data)
    {
        orig(self, data);
        HealthSystemManager healthSystemManager = self.Scene.Tracker.GetEntity<HealthSystemManager>();
        if (healthSystemManager != null && healthSystemManager.Active && !self.Dead)
        {
            if (healthData.playerOnCrush == HealthSystemManager.CrushEffect.PushOut)
            {
                self.TrySquishWiggle(data, (int)data.Pusher.Width, (int)data.Pusher.Height);
            }
            else if (healthData.playerOnCrush == HealthSystemManager.CrushEffect.InvincibleSolid)
            {
                data.Pusher.Add(new SolidOnInvinciblePlayer());
            }
            else //CrushEffect.InstantDeath
            {
                PlayerTakesDamage(Vector2.Zero, playerDamageController.health);
            }
        }
    }

    public static PlayerDeadBody OnPlayerCollide(On.Celeste.Player.orig_Die orig, Player self, Vector2 dir, bool always, bool register)
    {
        if (playerDamageController == null || playerDamageController.health <= 0 || PlayerIsOffscreen(self) || always)
        {
            return orig(self, dir, always, register);
        }
        PlayerTakesDamage(dir);
        return null;
    }

    private static bool PlayerIsOffscreen(Player player)
    {
        if (player != null)
        {
            Level level = player.SceneAs<Level>();
            if (level != null)
            {
                Rectangle bounds = level.Bounds;
                Rectangle val = new Rectangle((int)level.Camera.Left, (int)level.Camera.Top, 320, 180);
                return (level.CameraLockMode != 0 && (val.Bottom < bounds.Bottom - 4 && player.Top > val.Bottom) || player.Top > bounds.Bottom + 4);
            }
        }
        return false;
    }

    public static void PlayerTakesDamage(Vector2 origin, int amount = 1, bool silent = false)
    {
        playerDamageController?.TakeDamage(origin, amount, silent);
    }

    public static EntityData MakeEntityData()
    {
        EntityData entityData = new EntityData();
        entityData.Values = new Dictionary<string, object>();
        return entityData;
    }
}