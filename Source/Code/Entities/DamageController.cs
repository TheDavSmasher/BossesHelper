using Monocle;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using NLua;
using System.Linq;
using Celeste.Mod.BossesHelper.Code.Helpers;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    public class DamageController : Entity
    {
        private static string Filepath => BossesHelperModule.Session.healthData.onDamageFunction;

        private static BossesHelperSession BSession => BossesHelperModule.Session;

        private PlayerHealthBar HealthBar => Scene.Tracker.GetEntity<PlayerHealthBar>();

        private readonly float baseCooldown;

        private Level level;

        private LuaFunction onRecover;

        private LuaFunction onDamage;

        internal DamageController()
        {
            if (BSession.healthData.globalController)
                AddTag(Tags.Global);
            baseCooldown = BSession.healthData.damageCooldown;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Awake(Scene scene)
        {
            if (scene.Tracker.GetEntity<DamageController>() != this)
            {
                RemoveSelf();
                return;
            }
            base.Awake(scene);
            LoadFunction();
        }

        private void LoadFunction()
        {
            if (!string.IsNullOrEmpty(Filepath))
            {
                Dictionary<object, object> dict = new Dictionary<object, object>
                {
                    { "player", level.GetPlayer() },
                    { "healthBar", HealthBar.healthIcons },
                    { "modMetaData", BossesHelperModule.Instance.Metadata }
                };
                LuaFunction[] array = LoadLuaFile(Filepath, "getFunctionData", dict);
                if (array != null)
                {
                    onDamage = array.ElementAtOrDefault(0);
                    onRecover = array.ElementAtOrDefault(1);
                }
            }
        }

        public void TakeDamage(Vector2 direction, int amount = 1, bool silent = false, bool stagger = true, bool evenIfInvincible = false)
        {
            if (level.InCutscene ||
                !evenIfInvincible && (BSession.damageCooldown > 0 || SaveData.Instance.Assists.Invincible || amount <= 0))
            {
                return;
            }
            BSession.damageCooldown = baseCooldown;
            BSession.currentPlayerHealth -= amount;
            if (Engine.Scene.GetPlayer() is not Player entity || entity.StateMachine.State == Player.StCassetteFly)
            {
                return;
            }
            if (BSession.currentPlayerHealth > 0)
            {
                if (!silent)
                {
                    level.Shake();
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                    level.Flash(Color.Red * 0.3f);
                    Audio.Play("event:/char/madeline/predeath");
                    if (BSession.healthData.playerStagger && stagger)
                        Add(new Coroutine(PlayerStagger(entity, direction)));
                    if (BSession.healthData.playerBlink)
                        Add(new Coroutine(PlayerInvincible(entity)));
                    if (onDamage != null)
                        Add(new Coroutine(onDamage.ToIEnumerator()));
                }
            }
            else
            {
                entity.Die(direction);
            }
            if (HealthBar != null)
            {
                for (int i = 0; i < amount; i++)
                {
                    HealthBar.healthIcons.DecreaseHealth();
                }
            }
            else
            {
                Logger.Log("Bosses Helper", "No Health Bar has been initialized");
            }
        }

        public void RecoverHealth(int amount = 1)
        {
            BSession.currentPlayerHealth += amount;
            HealthBar.healthIcons.RefillHealth(amount);
            if (onRecover != null)
                Add(new Coroutine(onRecover.ToIEnumerator()));
        }

        private IEnumerator PlayerStagger(Player player, Vector2 bounce)
        {
            if (bounce != Vector2.Zero)
            {
                Celeste.Freeze(0.05f);
                yield return null;
                Vector2 from = player.Position;
                Vector2 to = new Vector2(from.X + (!(bounce.X < 0f) ? 1 : -1) * 20f, from.Y - 5f);
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.2f, start: true);
                Add(tween);
                tween.OnUpdate = delegate (Tween t)
                {
                    Vector2 val = from + (to - from) * t.Eased;
                    if (player != null)
                    {
                        player.MoveToX(val.X);
                        player.MoveToY(val.Y);
                        player.Sprite.Rotation = (float)(Math.Floor(t.Eased * 4f) * 6.2831854820251465);
                    }
                };
                yield return tween.Duration;
                tween.Stop();
            }
        }

        private IEnumerator PlayerInvincible(Player player)
        {
            int times = 2;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, BSession.damageCooldown, start: true);
            Add(tween);
            tween.OnUpdate = delegate
            {
                if (Scene.OnInterval(0.02f))
                {
                    if (times <= 0)
                    {
                        player.Sprite.Visible = false;
                        player.Hair.Visible = false;
                        times = 2;
                    }
                    else
                    {
                        player.Sprite.Visible = true;
                        player.Hair.Visible = true;
                        times--;
                    }
                }
            };
            yield return tween.Duration;
            tween.Stop();
            player.Sprite.Visible = true;
            player.Hair.Visible = true;
        }
    }
}
