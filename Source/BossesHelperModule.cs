using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using Celeste.Mod.BossesHelper.Code.Other;
using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers;

namespace Celeste.Mod.BossesHelper;

public partial class BossesHelperModule : EverestModule
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

    private static BossesHelperSession.HealthSystemData HealthData => BossesHelperModule.Session.healthData;

    public int TASSeed;

    [Command("set_boss_seed", "Set the seed Bosses will use for their RNG, added to a deterministic per-entity value. Value 0 makes the seed based on the Active Timer")]
    public static void SetBossSeed(int value)
    {
        Instance.TASSeed = value >= 0 ? value : Instance.TASSeed;
    }

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
        BossActions.BossEvent.WarmUp();
    }

    public override void Load()
    {
        using (new DetourConfigContext(new DetourConfig("BossesHelperEnforceBounds", 0, after: ["*"])).Use())
        {
            On.Celeste.Level.EnforceBounds += PlayerDiedWhileEnforceBounds;
        }
        On.Celeste.Level.LoadLevel += SetStartingHealth;
        On.Celeste.Player.Update += UpdatePlayerLastSafe;
        IL.Celeste.Player.OnSquish += ILOnSquish;
        On.Celeste.Player.Die += OnPlayerDie;
    }

    public override void Unload()
    {
        On.Celeste.Level.EnforceBounds -= PlayerDiedWhileEnforceBounds;
        On.Celeste.Level.LoadLevel -= SetStartingHealth;
        On.Celeste.Player.Update -= UpdatePlayerLastSafe;
        IL.Celeste.Player.OnSquish -= ILOnSquish;
        On.Celeste.Player.Die -= OnPlayerDie;
        ILHookHelper.DisposeAll();
    }

    #region Method Hooks
    private static void PlayerDiedWhileEnforceBounds(On.Celeste.Level.orig_EnforceBounds orig, Level self, Player player)
    {
        Session.wasOffscreen = true;
        orig(self, player);
        Session.wasOffscreen = false;
    }

    public static void SetStartingHealth(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes intro, bool fromLoader = false)
    {
        if (fromLoader)
        {
            if (HealthData.isCreated)
                self.Add(new HealthSystemManager());
            if (Session.safeGroundBlockerCreated)
                self.Add(new UpdateSafeBlocker());
        }
        orig(self, intro, fromLoader);
        if (Engine.Scene.GetPlayer() is Player entity)
        {
            entity.Sprite.Visible = true;
            entity.Hair.Visible = true;
            Session.SafeSpawn = entity.SceneAs<Level>().Session.RespawnPoint ?? entity.Position;
            if (Session.savePointSet && !Session.travelingToSavePoint && intro == Player.IntroTypes.Respawn)
            {
                Session.travelingToSavePoint = true;
                self.TeleportTo(entity, Session.savePointLevel, Session.savePointSpawnType, Session.savePointSpawn);
                Session.travelingToSavePoint = false;
            }
        }
        if (Engine.Scene.Tracker.GetEntity<HealthSystemManager>() == null || !HealthData.isEnabled)
            return;
        if (HealthData.globalController &&
            (intro == Player.IntroTypes.Transition && !HealthData.globalHealth ||
            intro == Player.IntroTypes.Respawn && !fromLoader && !Session.fakeDeathRespawn))
        {
            Session.currentPlayerHealth = HealthData.playerHealthVal;
            Engine.Scene.Tracker.GetEntity<PlayerHealthBar>().healthIcons.RefillHealth();
        }
        Session.fakeDeathRespawn = false;
    }

    public static void UpdatePlayerLastSafe(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);
        if (self.OnSafeGround && self.Scene.Tracker.GetEntity<UpdateSafeBlocker>() == null)
            Session.lastSafePosition = self.Position;
        if (self.StateMachine.State != Player.StCassetteFly)
            Session.alreadyFlying = false;
        if (self.SceneAs<Level>().Session.RespawnPoint is Vector2 spawn && Session.lastSpawnPoint != spawn)
            Session.SafeSpawn = spawn;
        if (Session.damageCooldown > 0)
            Session.damageCooldown -= Engine.DeltaTime;
    }

    public static PlayerDeadBody OnPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 dir, bool always, bool register)
    {
        bool damageTracked = self.Scene.Tracker.GetEntity<HealthSystemManager>() != null && HealthData.isEnabled;
        if (always)
        {
            if (damageTracked)
                PlayerTakesDamage(Vector2.Zero, Session.currentPlayerHealth, evenIfInvincible: true);        
            return orig(self, dir, always, register);
        }
        if (damageTracked && Session.currentPlayerHealth <= 0)
            return orig(self, dir, always, register);
        if (Session.useFakeDeath)
            return FakeDie(self, dir);
        if (Session.damageCooldown > 0)
            return null;
        if (!damageTracked)
            return orig(self, dir, always, register);

        if (!KillOffscreen(self))
            PlayerTakesDamage(dir);
        return null;
    }
    #endregion

    #region Hook Helper Methods
    public static void KillOnCrush(Player player, CollisionData data, bool evenIfInvincible)
    {
        if (player.Scene.Tracker.GetEntity<HealthSystemManager>() == null || !HealthData.isEnabled)
            return;
        switch (HealthData.playerOnCrush)
        {
            case HealthSystemManager.DeathEffect.PlayerPush:
                PlayerTakesDamage(Vector2.Zero);
                if (!player.TrySquishWiggle(data, (int)data.Pusher.Width, (int)data.Pusher.Height))
                    player.TrySquishWiggle(data, player.level.Bounds.Width, player.level.Bounds.Height);
                break;
            case HealthSystemManager.DeathEffect.PlayerSafe:
                if (evenIfInvincible) break;
                PlayerTakesDamage(Vector2.Zero);
                data.Pusher.Add(new SolidOnInvinciblePlayer());
                break;
            default:
                SharedDeath(player, HealthData.playerOnCrush);
                break;
        }
    }

    private static bool KillOffscreen(Player player)
    {
        float? offscreemAtY = GetFromY(player.SceneAs<Level>(), player);
        if (offscreemAtY is not float atY)
            return false;
        switch (HealthData.playerOffscreen)
        {
            case HealthSystemManager.DeathEffect.PlayerPush:
                PlayerTakesDamage(Vector2.Zero, stagger: false);
                player.Play("event:/game/general/assist_screenbottom");
                player.Bounce(atY);
                break;
            case HealthSystemManager.DeathEffect.PlayerSafe:
                PlayerTakesDamage(Vector2.Zero, stagger: false);
                if (!Session.alreadyFlying)
                    player.Add(new Coroutine(PlayerFlyBack(player)));
                break;
            default:
                SharedDeath(player, HealthData.playerOffscreen);
                break;
        }
        return true;
    }

    private static void SharedDeath(Player player, HealthSystemManager.DeathEffect effect)
    {
        if (effect == HealthSystemManager.DeathEffect.FakeDeath)
        {
            PlayerTakesDamage(Vector2.Zero, stagger: false, evenIfInvincible: true);
            FakeDie(player);
            return;
        }
        PlayerTakesDamage(Vector2.Zero, Session.currentPlayerHealth, evenIfInvincible: true);
    }

    private static PlayerDeadBody FakeDie(Player self, Vector2? dir = null)
    {
        Level level = self.SceneAs<Level>();

        void TeleportPlayer()
        {
            //level.Add(self);
            self.Position = Session.lastSafePosition;
            level.DoScreenWipe(true);
        }

        if (!self.Dead && self.StateMachine.State != Player.StReflectionFall)
        {
            Session.fakeDeathRespawn = true;
            self.Stop(self.wallSlideSfx);
            self.Depth = -1000000;
            self.Speed = Vector2.Zero;
            self.StateMachine.Locked = true;
            self.Collidable = false;
            self.Drop();
            self.LastBooster?.PlayerDied();
            self.level.InCutscene = false;
            self.level.Shake();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            PlayerDeadBody fakeDeadBody = new(self, dir ?? Vector2.UnitY * -1)
            {
                DeathAction = TeleportPlayer
            };
            fakeDeadBody.Get<Coroutine>().Replace(BossesHelperUtils.FakeDeathRoutine(fakeDeadBody));
            level.Add(fakeDeadBody);
            level.Remove(self);
        }
        return null;
    }

    private static IEnumerator PlayerFlyBack(Player player)
    {
        Session.alreadyFlying = true;
        yield return 0.3f;
        Audio.Play("event:/game/general/cassette_bubblereturn", player.SceneAs<Level>().Camera.Position + new Vector2(160f, 90f));
        Vector2 middle = new(player.X + (Session.lastSafePosition.X - player.X) / 2, player.Y + (Session.lastSafePosition.Y - player.Y) / 2);
        player.StartCassetteFly(Session.lastSafePosition, middle - Vector2.UnitY * 8);
    }

    public static void PlayerTakesDamage(Vector2 origin, int amount = 1, bool silent = false, bool stagger = true, bool evenIfInvincible = false)
    {
        Engine.Scene.Tracker.GetEntity<DamageController>()?.TakeDamage(origin, amount, silent, stagger, evenIfInvincible);
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
    #endregion

    public static EntityData MakeEntityData()
    {
        EntityData entityData = new()
        {
            Values = []
        };
        return entityData;
    }

    public static void GiveIFrames(float time)
    {
        Session.damageCooldown += time;
    }
}