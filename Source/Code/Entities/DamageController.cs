using Monocle;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using NLua;
using Celeste.Mod.BossesHelper.Code.Helpers;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;
using static Celeste.Mod.BossesHelper.Code.Entities.HealthSystemManager;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    public class DamageController : GlobalEntity
    {
        private static BossesHelperSession ModSession => BossesHelperModule.Session;

        private static BossesHelperSession.HealthSystemData HealthData => ModSession.healthData;

        private PlayerHealthBar HealthBar => Scene.GetEntity<PlayerHealthBar>();

        private Level Level => SceneAs<Level>();

        private LuaFunction onRecover;

        private LuaFunction onDamage;

        internal DamageController() : base(HealthData.globalController)
        {
        }

        public override void Awake(Scene scene)
        {
            if (scene.GetEntity<DamageController>() != this)
            {
                RemoveSelf();
                return;
            }
            base.Awake(scene);
            LoadFunction();
        }

        private void LoadFunction()
        {
            LuaFunction[] array = LoadLuaFile(new Dictionary<object, object>
            {
                { "player", Scene.GetPlayer() },
                { "healthBar", HealthBar }
            },
            HealthData.onDamageFunction, "getFunctionData", 2);
            onDamage = array[0];
            onRecover = array[1];
        }

        public void TakeDamage(Vector2 direction, int amount = 1, bool silent = false, bool stagger = true, bool evenIfInvincible = false)
        {
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
            HealthBar.DecreaseHealth(amount);
        }

        public void RecoverHealth(int amount = 1)
        {
            ModSession.currentPlayerHealth += amount;
            HealthBar.RefillHealth(amount);
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
            static void ChangeVisible(bool state) {
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
}
