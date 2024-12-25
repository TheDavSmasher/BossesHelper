using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Other;
using Monocle;
using Celeste.Mod.BossesHelper.Code.Entities;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using System.Reflection;
using MonoMod.Utils;

namespace Celeste.Mod.BossesHelper;

public class BossesHelperModule : EverestModule
{
    public static BossesHelperModule Instance { get; private set; }

    // Store Settings
    public override Type SettingsType => typeof(BossesHelperSettings);
    public static BossesHelperSettings Settings => (BossesHelperSettings)Instance._Settings;

    // Store Session
    public override Type SessionType => typeof(BossesHelperSession);
    public static BossesHelperSession Session => (BossesHelperSession)Instance._Session;

    // Store Save Data
    public override Type SaveDataType => typeof(BossesHelperSaveData);
    public static BossesHelperSaveData BossSaveData => (BossesHelperSaveData)Instance._SaveData;

    public int TASSeed;

    [Command("set_boss_seed", "Set the seed Bosses will use for their RNG, added to a deterministic per-entity value. Value 0 makes the seed based on the Active Timer")]
    public static void SetBossSeed(int value)
    {
        Instance.TASSeed = value >= 0 ? value : Instance.TASSeed;
    }

    private ILHook levelExitRoutineHook;

    public BossesHelperModule()
    {
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

    public override void Load()
    {
        using (new DetourConfigContext(new DetourConfig("BossesHelperEnforceBounds", 1, after: ["*"])).Use())
        {
            On.Celeste.Level.EnforceBounds += PlayerDiedWhileEnforceBounds;
        }
        On.Celeste.Level.LoadLevel += SetStartingHealth;
        On.Celeste.Player.Update += UpdatePlayerLastSafe;
        IL.Celeste.Player.OnSquish += ILOnSquish;
        On.Celeste.Player.Die += OnPlayerDie;
        On.Celeste.Player.Die += ReturnToSavePoint;
        levelExitRoutineHook = new ILHook(
            typeof(LevelExit).GetMethod("Routine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(),
            ILLevelLoadSpawn);
    }

    public override void Unload()
    {
        On.Celeste.Level.EnforceBounds -= PlayerDiedWhileEnforceBounds;
        On.Celeste.Level.LoadLevel -= SetStartingHealth;
        On.Celeste.Player.Update -= UpdatePlayerLastSafe;
        IL.Celeste.Player.OnSquish -= ILOnSquish;
        On.Celeste.Player.Die -= OnPlayerDie;
        On.Celeste.Player.Die -= ReturnToSavePoint;
        levelExitRoutineHook?.Dispose();
        levelExitRoutineHook = null;
    }

    private static void PlayerDiedWhileEnforceBounds(On.Celeste.Level.orig_EnforceBounds orig, Level self, Player player)
    {
        Session.wasOffscreen = true;
        orig(self, player);
        Session.wasOffscreen = false;
    }

    public static void SetStartingHealth(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes intro, bool fromLoader = false)
    {
        if (fromLoader && Session.healthData.isCreated)
        {
            Session.mapHealthSystemManager ??= new();
            self.Add(Session.mapHealthSystemManager);
        }
        orig(self, intro, fromLoader);
        if (Session.mapHealthSystemManager == null || !Session.healthData.isEnabled)
            return;
        if (Session.healthData.globalController &&
            (intro == Player.IntroTypes.Transition && !Session.healthData.globalHealth || intro == Player.IntroTypes.Respawn))
        {
            Session.mapDamageController.health = Session.healthData.playerHealthVal;
            Session.mapHealthBar.healthIcons.RefillHealth();
        }
        Player entity = Engine.Scene.Tracker.GetEntity<Player>();
        if (entity != null)
        {
            entity.Sprite.Visible = true;
            entity.Hair.Visible = true;
            Session.SafeSpawn = entity.SceneAs<Level>().Session.RespawnPoint ?? entity.Position;
        }
    }

    public static void UpdatePlayerLastSafe(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);
        if (self.OnSafeGround)
            Session.lastSafePosition = self.Position;
        if (self.StateMachine.State != Player.StCassetteFly)
            Session.alreadyFlying = false;

        Vector2? currentSpawn = self.SceneAs<Level>().Session.RespawnPoint;
        if (currentSpawn != null && Session.lastSpawnPoint != currentSpawn)
            Session.SafeSpawn = (Vector2) currentSpawn;
        if (Session.damageCooldown > 0)
            Session.damageCooldown -= Engine.DeltaTime;
    }

    public static void ILOnSquish(ILContext il)
    {
        ILCursor dieCursor = new ILCursor(il);
        while (dieCursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("Die")))
        {
            ILCursor argCursor = new ILCursor(dieCursor);
            if (argCursor.TryGotoPrev(MoveType.AfterLabel, instr => instr.MatchLdarg0()))
            {
                //KillOnCrush(self, data, evenIfInvincible);
                argCursor.EmitLdarg0();
                argCursor.EmitLdarg1();
                argCursor.EmitLdloc2();
                argCursor.EmitDelegate(KillOnCrush);
            }
        }
    }

    public static void KillOnCrush(Player player, CollisionData data, bool evenIfInvincible)
    {
        if (Session.mapHealthSystemManager == null || !Session.mapHealthSystemManager.Active)
            return;
        switch (Session.healthData.playerOnCrush)
        {
            case HealthSystemManager.CrushEffect.PushOut:
                PlayerTakesDamage(Vector2.Zero);
                if (!player.TrySquishWiggle(data, (int)data.Pusher.Width, (int)data.Pusher.Height))
                    player.TrySquishWiggle(data, player.level.Bounds.Width, player.level.Bounds.Height);
                break;
            case HealthSystemManager.CrushEffect.InvincibleSolid:
                if (evenIfInvincible) break;
                PlayerTakesDamage(Vector2.Zero);
                data.Pusher.Add(new SolidOnInvinciblePlayer());
                break;
            default: //CrushEffect.InstantDeath
                PlayerTakesDamage(Vector2.Zero, Session.mapDamageController.health, ignoreCooldown: true);
                break;
        }
    }

    private static bool KillOffscreen(Player player)
    {
        float? offscreemAtY = GetFromY(player.SceneAs<Level>(), player);
        if (offscreemAtY == null)
            return false;
        switch (Session.healthData.playerOffscreen)
        {
            case HealthSystemManager.OffscreenEffect.BounceUp:
                PlayerTakesDamage(Vector2.Zero, stagger: false);
                player.Play("event:/game/general/assist_screenbottom");
                player.Bounce((float)offscreemAtY);
                break;
            case HealthSystemManager.OffscreenEffect.BubbleBack:
                PlayerTakesDamage(Vector2.Zero, stagger: false);
                if (!Session.alreadyFlying)
                    player.Add(new Coroutine(PlayerFlyBack(player)));
                break;
            default: //OffscreenEffect.InstantDeath
                PlayerTakesDamage(Vector2.Zero, Session.mapDamageController.health, ignoreCooldown: true);
                break;
        }
        return true;
    }

    public static PlayerDeadBody OnPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 dir, bool always, bool register)
    {
        PlayerDeadBody playerDeadBody = orig(self, dir, always, register);
        if (always)
        {
            if (Session.mapDamageController != null)
            {
                PlayerTakesDamage(Vector2.Zero, Session.mapDamageController.health, ignoreCooldown: true);
                return null;
            }            
            return playerDeadBody;
        }
        if (Session.mapDamageController != null && Session.mapDamageController.health <= 0)
            return playerDeadBody;
        if (Session.damageCooldown > 0)
            return null;
        if (Session.mapDamageController == null)
            return playerDeadBody;

        if (!KillOffscreen(self))
            PlayerTakesDamage(dir);
        return null;
    }

    public static void ILLevelLoadSpawn(ILContext il)
    {
        ILCursor levelLoaderCursor = new ILCursor(il);
        while (levelLoaderCursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld<LevelLoader>("PlayerIntroOverride")))
        {
            levelLoaderCursor.EmitLdloc3();
            levelLoaderCursor.EmitDelegate(ChangeLevelLoader);
        }
    }

    private static void ChangeLevelLoader(LevelLoader loader)
    {
        if (Session.savePointSet)
        {
            loader.startPosition ??= Session.savePointSpawn;
            loader.PlayerIntroTypeOverride = Session.savePointSpawnType;
        }
    }

    public static PlayerDeadBody ReturnToSavePoint(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
    {
        PlayerDeadBody deadPlayer = orig(self, direction, evenIfInvincible, registerDeathInStats);
        if (deadPlayer != null)
        {
            if (!deadPlayer.HasGolden && Session.savePointSet)
            {
                deadPlayer.DeathAction = () =>
                {
                    Engine.Scene = new LevelExit(LevelExit.Mode.GoldenBerryRestart, deadPlayer.player.level.Session)
                    {
                        GoldenStrawberryEntryLevel = Session.savePointLevel
                    };
                };
            }
        }
        return deadPlayer;
    }

    private static IEnumerator PlayerFlyBack(Player player)
    {
        Session.alreadyFlying = true;
        yield return 0.3f;
        Audio.Play("event:/game/general/cassette_bubblereturn", player.SceneAs<Level>().Camera.Position + new Vector2(160f, 90f));
        Vector2 middle = new(player.X + (Session.lastSafePosition.X - player.X) / 2, player.Y + (Session.lastSafePosition.Y - player.Y) / 2);
        player.StartCassetteFly(Session.lastSafePosition, middle - Vector2.UnitY * 8);
    }

    public static void PlayerTakesDamage(Vector2 origin, int amount = 1, bool silent = false, bool stagger = true, bool ignoreCooldown = false)
    {
        Session.mapDamageController?.TakeDamage(origin, amount, silent, stagger, ignoreCooldown);
    }

    private static float? GetFromY(Level level, Player player)
    {
        if (!Session.wasOffscreen)
            return null;
        Rectangle camera = new((int)level.Camera.Left, (int)level.Camera.Top, 320, 180);
        if (level.CameraLockMode != Level.CameraLockModes.None)
        {
            if (camera.Bottom < level.Bounds.Bottom - 4 && player.Top > camera.Bottom)
                return camera.Top;
            if (camera.Top > level.Bounds.Top + 4 && player.Bottom < level.Bounds.Top)
                return level.Bounds.Top;
        }
        if (player.Bottom < level.Bounds.Top)
            return level.Bounds.Top;
        if (player.Top > level.Bounds.Bottom)
            return level.Bounds.Bottom;
        return null;
    }

    public static EntityData MakeEntityData()
    {
        EntityData entityData = new EntityData();
        entityData.Values = new Dictionary<string, object>();
        return entityData;
    }

    public static void GiveIFrames(float time)
    {
        Session.damageCooldown += time;
    }
}