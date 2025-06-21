using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using NLua;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    [CustomEntity("BossesHelper/HealthSystemManager")]
    public partial class HealthSystemManager : GlobalEntity
    {
        private static BossesHelperSession ModSession => BossesHelperModule.Session;

        private static ref BossesHelperSession.HealthSystemData HealthData => ref ModSession.healthData;

        public new bool Visible
        {
            get
            {
                return HealthBar.Visible;
            }
            set
            {
                HealthBar.Visible = value;
            }
        }

        [Tracked(false)]
        private class PlayerHealthBar() : HealthIconList(HealthData.globalController)
        {
            public override void Awake(Scene scene)
            {
                Visible = HealthData.startVisible;
                Clear();
                base.Awake(scene);
            }
        }

        [Tracked(false)]
        private class DamageController() : GlobalEntity(false)
        {
            private LuaFunction onRecover;

            private LuaFunction onDamage;

            public void UpdateState(bool isGlobal, Player player, PlayerHealthBar healthBar)
            {
                ChangeGlobalState(isGlobal);
                LuaFunction[] array = LoadLuaFile(new Dictionary<object, object>
                {
                    { "player", player },
                    { "healthBar", healthBar }
                },
                HealthData.onDamageFunction, "getFunctionData", 2);
                onDamage = array[0];
                onRecover = array[1];
            }

            public void TakeDamage(Vector2 direction, int amount = 1, bool silent = false, bool stagger = true, bool evenIfInvincible = false)
            {
                Level Level = SceneAs<Level>();
                if (Level.InCutscene ||
                    !evenIfInvincible && (ModSession.damageCooldown > 0 || SaveData.Instance.Assists.Invincible || amount <= 0) ||
                    Engine.Scene.GetPlayer() is not Player entity || entity.StateMachine.State == Player.StCassetteFly)
                {
                    return;
                }
                ModSession.damageCooldown = HealthData.damageCooldown;
                if ((ModSession.currentPlayerHealth -= amount) > 0)
                {
                    if (!silent)
                    {
                        Level.Shake();
                        Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                        Level.Flash(Color.Red * 0.3f);
                        Audio.Play("event:/char/madeline/predeath");
                        if (HealthData.playerStagger && stagger)
                            Add(new Coroutine(PlayerStagger(entity.Position, direction)));
                        if (HealthData.playerBlink)
                            Add(new Coroutine(PlayerInvincible()));
                        if (onDamage != null)
                            Add(new Coroutine(onDamage.ToIEnumerator()));
                    }
                }
                else
                {
                    entity.Die(direction);
                }
            }

            public void RecoverHealth(int amount = 1)
            {
                ModSession.currentPlayerHealth += amount;
                if (onRecover != null)
                    Add(new Coroutine(onRecover.ToIEnumerator()));
            }

            public void RefillHealth()
            {
                ModSession.currentPlayerHealth = HealthData.playerHealthVal;
                if (onRecover != null)
                    Add(new Coroutine(onRecover.ToIEnumerator()));
            }

            private IEnumerator PlayerStagger(Vector2 from, Vector2 bounce)
            {
                if (bounce != Vector2.Zero)
                {
                    Celeste.Freeze(0.05f);
                    yield return null;
                    Vector2 to = new(from.X + (!(bounce.X < 0f) ? 1 : -1) * 20f, from.Y - 5f);
                    Tween tween = Tween.Set(this, Tween.TweenMode.Oneshot, 0.2f, Ease.CubeOut, delegate (Tween t)
                    {
                        Vector2 val = from + (to - from) * t.Eased;
                        if (Engine.Scene.GetPlayer() is Player player)
                        {
                            player.MoveToX(val.X);
                            player.MoveToY(val.Y);
                            player.Sprite.Rotation = (float)(Math.Floor(t.Eased * 4f) * 6.2831854820251465);
                        }
                    });
                    yield return tween.Wait();
                }
            }

            private IEnumerator PlayerInvincible()
            {
                static void ChangeVisible(bool state)
                {
                    if (Engine.Scene.GetPlayer() is Player player)
                    {
                        player.Sprite.Visible = state;
                        player.Hair.Visible = state;
                    }
                }
                int times = 1;
                Tween tween = Tween.Set(this, Tween.TweenMode.Oneshot, HealthData.damageCooldown, Ease.CubeOut, delegate
                {
                    if (Scene.OnInterval(0.02f))
                    {
                        ChangeVisible(times++ % 3 == 0);
                    }
                });
                yield return tween.Wait();
                ChangeVisible(true);
            }
        }

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

        private PlayerHealthBar HealthBar;

        private DamageController Controller;

        public static bool IsEnabled
        {
            get => HealthData.isEnabled;
            set => HealthData.isEnabled = value;
        }

        private HealthSystemManager(bool resetHealth, bool isGlobal, int setHealthTo = 0, string activateFlag = null)
            : base(isGlobal)
        {
            if (activateFlag != null)
            {
                HealthData.activateFlag = activateFlag;
            }
            if (setHealthTo > 0)
            {
                HealthData.playerHealthVal = setHealthTo;
            }
            if (resetHealth)
            {
                ModSession.currentPlayerHealth = HealthData.playerHealthVal;
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
            bool wasEnabled = IsEnabled;
            ChangeGlobalState(data.Bool("isGlobal"));
            HealthData = new()
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
                isCreated = true
            };
            if (wasEnabled)
            {
                HealthBar.ChangeGlobalState(IsGlobal);
                HealthBar.UpdateState();
                Controller.UpdateState(IsGlobal, Scene.GetPlayer(), HealthBar);
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (scene.GetEntity<HealthSystemManager>() != this)
            {
                RemoveSelf();
            }
            else if (IsEnabled || HealthData.activateInstantly)
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
                HealthData.isCreated = false;
            }
        }

        public void TakeDamage(Vector2 direction, int amount = 1, bool silent = false, bool stagger = true, bool evenIfInvincible = false)
        {
            if (!IsEnabled) return;
            Controller.TakeDamage(direction, amount, silent, stagger, evenIfInvincible);
            HealthBar.DecreaseHealth(amount);
        }

        public void RecoverHealth(int amount = 1)
        {
            if (!IsEnabled) return;
            Controller.RecoverHealth(amount);
            HealthBar.RefillHealth(amount);
            }

        public void RefillHealth()
        {
            if (!IsEnabled) return;
            Controller.RefillHealth();
            HealthBar.RefillHealth();
        }

        public void EnableHealthSystem()
        {
            IsEnabled = true;
            if (Scene.GetEntity<PlayerHealthBar>() == null)
                Scene.Add(HealthBar = new PlayerHealthBar());

            if (Scene.GetEntity<DamageController>() == null)
            {
                Scene.Add(Controller = new DamageController());
                Controller.UpdateState(IsGlobal, Scene.GetPlayer(), HealthBar);
            }

            LoadFakeDeathHooks();
            Get<EntityFlagger>()?.RemoveSelf();
        }

        public void DisableHealthSystem()
        {
            IsEnabled = false;
            UnloadFakeDeathHooks();
            Controller.RemoveSelf();
            HealthBar.RemoveSelf();
        }

        private static partial void LoadFakeDeathHooks();
        private static partial void UnloadFakeDeathHooks();
    }
}
