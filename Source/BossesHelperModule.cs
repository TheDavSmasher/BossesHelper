using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Other;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Monocle;
using Celeste;

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

    public static int playerHealthVal;

    public static string iconSprite;

    public static string startAnim;

    public static string endAnim;

    public static float iconSeparation;

    public static Vector2 healthBarPos;

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
        On.Celeste.Player.Die += new On.Celeste.Player.hook_Die(OnPlayerCollide);
    }

    public override void Unload() {
        On.Celeste.Level.LoadLevel -= new On.Celeste.Level.hook_LoadLevel(SetStartingHealth);
        On.Celeste.Player.Die -= new On.Celeste.Player.hook_Die(OnPlayerCollide);
    }

    public static void SetStartingHealth(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes intro, bool fromLoader = false)
    {
        orig(self, intro, fromLoader);
        if (intro != 0 && playerHealthBar != null)
        {
            self.Remove(playerHealthBar);
            playerHealthBar = null;
            if (playerDamageController != null)
            {
                self.Remove(playerDamageController);
                playerDamageController = null;
            }
        }
        if (self.Session.Area.Mode == AreaMode.Normal && playerHealthBar == null)
        {
            playerHealthBar = new DamageHealthBar(healthBarPos, playerHealthVal, iconSprite, startAnim, endAnim, iconSeparation);
            self.Add(playerHealthBar);
            if  (playerDamageController == null)
            {
                playerDamageController = new DamageController();
                self.Add(playerDamageController);
            }
        }
        if (intro == Player.IntroTypes.Transition && playerDamageController != null && playerHealthBar != null)
        {
            playerDamageController.health = playerHealthVal;
            playerHealthBar.RefillHealth();
        }
        Player entity = Engine.Scene.Tracker.GetEntity<Player>();
        if (entity != null)
        {
            entity.Sprite.Visible = true;
            entity.Hair.Visible = true;
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

    public static void PlayerTakesDamage(Vector2 origin, int amount = 1)
    {
        playerDamageController?.TakeDamage(origin, amount);
    }

    public static EntityData MakeEntityData()
    {
        EntityData entityData = new EntityData();
        entityData.Values = new Dictionary<string, object>();
        return entityData;
    }
}