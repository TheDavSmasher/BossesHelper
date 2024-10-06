using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Other;
using Celeste;
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
    public static BossesHelperSaveData BossSaveData => (BossesHelperSaveData) Instance._SaveData;

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
        if (fromLoader && BossesHelperModule.Session.mapHealthSystemManager != null)
        {
            self.Add(BossesHelperModule.Session.mapHealthSystemManager);
        }
        orig(self, intro, fromLoader);
        if (Session.mapHealthSystemManager == null || !Session.mapHealthSystemManager.enabled)
        {
            return;
        }
        if (Session.healthData.globalController)
        {
            if ((intro == Player.IntroTypes.Transition && !Session.healthData.globalHealth) || intro == Player.IntroTypes.Respawn)
            {
                Session.mapDamageController.health = Session.healthData.playerHealthVal;
                Session.mapHealthBar.RefillHealth();
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
        if (Session.mapHealthSystemManager == null || !Session.mapHealthSystemManager.Active)
        {
            orig(self, data);
            return;
        }
        bool ducked = false;
        if (!self.Ducking && self.StateMachine.State != 1)
        {
            ducked = true;
            self.Ducking = true;
            data.Pusher.Collidable = true;
            if (!self.CollideCheck<Solid>())
            {
                data.Pusher.Collidable = false;
                return;
            }
            Vector2 position = self.Position;
            self.Position = data.TargetPosition;
            if (!self.CollideCheck<Solid>())
            {
                data.Pusher.Collidable = false;
                return;
            }
            self.Position = position;
            data.Pusher.Collidable = false;
        }
        if (!self.TrySquishWiggle(data))
        {
            bool evenIfInvincible = false;
            if (data.Pusher != null && data.Pusher.SquishEvenInAssistMode)
            {
                evenIfInvincible = true;
            }

            if (Session.healthData.playerOnCrush == HealthSystemManager.CrushEffect.PushOut)
            {
                PlayerTakesDamage(Vector2.Zero);
                self.TrySquishWiggle(data, (int)data.Pusher.Width, (int)data.Pusher.Height);
            }
            else if (Session.healthData.playerOnCrush == HealthSystemManager.CrushEffect.InvincibleSolid && !evenIfInvincible)
            {
                PlayerTakesDamage(Vector2.Zero);
                data.Pusher.Add(new SolidOnInvinciblePlayer());
            }
            else //CrushEffect.InstantDeath
            {
                PlayerTakesDamage(Vector2.Zero, Session.mapDamageController.health);
                self.Die(Vector2.Zero, evenIfInvincible);
            }
        }
        else if (ducked && self.CanUnDuck)
        {
            self.Ducking = false;
        }
    }

    public static PlayerDeadBody OnPlayerCollide(On.Celeste.Player.orig_Die orig, Player self, Vector2 dir, bool always, bool register)
    {
        if (Session.mapDamageController == null || Session.mapDamageController.health <= 0 || PlayerIsOffscreen(self) || always)
        {
            if (Session.mapDamageController != null)
                Session.mapDamageController.health = 0;
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
        Session.mapDamageController?.TakeDamage(origin, amount, silent);
    }

    public static EntityData MakeEntityData()
    {
        EntityData entityData = new EntityData();
        entityData.Values = new Dictionary<string, object>();
        return entityData;
    }
}