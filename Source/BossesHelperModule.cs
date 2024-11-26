using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Other;
using Monocle;
using Celeste.Mod.BossesHelper.Code.Entities;
using MonoMod.RuntimeDetour;
using System.Linq;

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
        On.Celeste.Player.OnSquish += ApplyUserCrush;
        On.Celeste.Player.Die += OnPlayerDie;
    }

    public override void Unload()
    {
        On.Celeste.Level.EnforceBounds -= PlayerDiedWhileEnforceBounds;
        On.Celeste.Level.LoadLevel -= SetStartingHealth;
        On.Celeste.Player.Update -= UpdatePlayerLastSafe;
        On.Celeste.Player.OnSquish -= ApplyUserCrush;
        On.Celeste.Player.Die -= OnPlayerDie;
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
        {
            return;
        }
        if (Session.healthData.globalController)
        {
            if ((intro == Player.IntroTypes.Transition && !Session.healthData.globalHealth) || intro == Player.IntroTypes.Respawn)
            {
                Session.mapDamageController.health = Session.healthData.playerHealthVal;
                Session.mapHealthBar.healthIcons.RefillHealth();
            }
        }
        Player entity = Engine.Scene.Tracker.GetEntity<Player>();
        if (entity != null)
        {
            entity.Sprite.Visible = true;
            entity.Hair.Visible = true;
            Session.lastSafePosition = Session.lastSpawnPoint = entity.SceneAs<Level>().Session.RespawnPoint ?? entity.Position;
        }
    }

    public static void UpdatePlayerLastSafe(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);
        if (self.OnSafeGround)
        {
            Session.lastSafePosition = self.Position;
        }
        if (self.StateMachine.State != Player.StCassetteFly)
        {
            Session.alreadyFlying = false;
        }
        Vector2? currentSpawn = self.SceneAs<Level>().Session.RespawnPoint;
        if (currentSpawn != null && Session.lastSpawnPoint != currentSpawn)
        {
            Session.lastSafePosition = Session.lastSpawnPoint = (Vector2) currentSpawn;
        }
        if (Session.damageCooldown > 0)
        {
            Session.damageCooldown -= Engine.DeltaTime;
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
                if (!self.TrySquishWiggle(data, (int)data.Pusher.Width, (int)data.Pusher.Height))
                {
                    if (!self.TrySquishWiggle(data, self.level.Bounds.Width, self.level.Bounds.Height))
                        self.Die(Vector2.Zero, evenIfInvincible);
                }
            }
            else if (Session.healthData.playerOnCrush == HealthSystemManager.CrushEffect.InvincibleSolid && !evenIfInvincible)
            {
                PlayerTakesDamage(Vector2.Zero);
                data.Pusher.Add(new SolidOnInvinciblePlayer());
            }
            else //CrushEffect.InstantDeath
            {
                PlayerTakesDamage(Vector2.Zero, Session.mapDamageController.health, ignoreCooldown: true);
            }
        }
        else if (ducked && self.CanUnDuck)
        {
            self.Ducking = false;
        }
    }

    public static PlayerDeadBody OnPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 dir, bool always, bool register)
    {
        if (Session.damageCooldown > 0 && !always)
        {
            return null;
        }
        if (Session.mapDamageController == null || Session.mapDamageController.health <= 0)
        {
            return orig(self, dir, always, register);
        }
        if (always)
        {
            PlayerTakesDamage(Vector2.Zero, Session.mapDamageController.health, ignoreCooldown: true);
            return orig(self, dir, always, register);
        }
        float? offscreemAtY = GetFromY(self.SceneAs<Level>(), self);
        if (offscreemAtY != null)
        {
            if (Session.healthData.playerOffscreen == HealthSystemManager.OffscreenEffect.BounceUp)
            {
                PlayerTakesDamage(Vector2.Zero, stagger: false);
                self.Play("event:/game/general/assist_screenbottom");
                self.Bounce((float)offscreemAtY);
                return null;
            }
            if (Session.healthData.playerOffscreen == HealthSystemManager.OffscreenEffect.BubbleBack)
            {
                PlayerTakesDamage(Vector2.Zero, stagger: false);
                if (!Session.alreadyFlying)
                    self.Add(new Coroutine(PlayerFlyBack(self)));
                return null;
            }
            if (Session.healthData.playerOffscreen == HealthSystemManager.OffscreenEffect.InstantDeath)
            {
                PlayerTakesDamage(Vector2.Zero, Session.mapDamageController.health, ignoreCooldown: true);
            }
        }
        PlayerTakesDamage(dir);
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

    public static void PlayerTakesDamage(Vector2 origin, int amount = 1, bool silent = false, bool stagger = true, bool ignoreCooldown = false)
    {
        Session.mapDamageController?.TakeDamage(origin, amount, silent, stagger, ignoreCooldown);
    }

    private static float? GetFromY(Level level, Player player)
    {
        if (!Session.wasOffscreen)
        {
            return null;
        }
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

    #region Everest PR
    public static void AddEntityToTracker(Type type, params Type[] subTypes)
    {
        Tracker.StoredEntityTypes.Add(type);
        if (!Tracker.TrackedEntityTypes.TryGetValue(type, out List<Type> value))
        {
            Tracker.TrackedEntityTypes[type] = new List<Type> { type };
        }
        else if (!value.Contains(type))
        {
            value.Add(type);
        }
        foreach (Type subType in subTypes)
        {
            if (!Tracker.TrackedEntityTypes.TryGetValue(subType, out List<Type> subvalue))
            {
                Tracker.TrackedEntityTypes[subType] = new List<Type> { type };
            }
            else if (!subvalue.Contains(type))
            {
                subvalue.Add(type);
            }
        }
        Dictionary<Type, List<Entity>> entities = Engine.Scene?.Tracker.Entities;
        if (entities != null && !entities.ContainsKey(type))
        {
            entities[type] = Engine.Scene.Entities.Where((Entity e) => e.GetType() == type).ToList();
        }
    }

    public static void AddComponentToTracker(Type type, params Type[] subTypes)
    {
        Tracker.StoredComponentTypes.Add(type);
        if (!Tracker.TrackedComponentTypes.TryGetValue(type, out List<Type> value))
        {
            Tracker.TrackedComponentTypes[type] = new List<Type> { type };
        }
        else if (!value.Contains(type))
        {
            value.Add(type);
        }
        foreach (Type subType in subTypes)
        {
            if (!Tracker.TrackedComponentTypes.TryGetValue(subType, out List<Type> subvalue))
            {
                Tracker.TrackedComponentTypes[subType] = new List<Type> { type };
            }
            else if (!subvalue.Contains(type))
            {
                subvalue.Add(type);
            }
        }
        Dictionary<Type, List<Component>> components = Engine.Scene?.Tracker.Components;
        if (components != null && !components.ContainsKey(type))
        {
            List<Component> list = new List<Component>();
            foreach (Entity entity in Engine.Scene.Entities)
            {
                var component = entity.Components.FirstOrDefault((c) => c.GetType() == type);
                if (component != null)
                {
                    list.Add(component);
                }
            }
            components[type] = list;
        }
    }
    #endregion

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