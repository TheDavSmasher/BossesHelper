﻿using Monocle;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using NLua;
using Celeste.Mod.BossesHelper.Code.Helpers;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    public class DamageController : GlobalEntity
    {
        private static string Filepath => BossesHelperModule.Session.healthData.onDamageFunction;

        private static BossesHelperSession BSession => BossesHelperModule.Session;

        private PlayerHealthBar HealthBar => Scene.Tracker.GetEntity<PlayerHealthBar>();

        private readonly float baseCooldown;

        private Level level;

        private LuaFunction onRecover;

        private LuaFunction onDamage;

        internal DamageController() : base(BSession.healthData.globalController)
        {
            baseCooldown = BSession.healthData.damageCooldown;
        }

        public override void Awake(Scene scene)
        {
            level = scene as Level;
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
                LuaFunction[] array = LoadLuaFile(new Dictionary<object, object>
                    {
                        { "player", level.GetPlayer() },
                        { "healthBar", HealthBar },
                        { "modMetaData", BossesHelperModule.Instance.Metadata }
                    }, 
                    Filepath, "getFunctionData", 2);
                onDamage = array[0];
                onRecover = array[1];
            }
        }

        public void TakeDamage(Vector2 direction, int amount = 1, bool silent = false, bool stagger = true, bool evenIfInvincible = false)
        {
            if (level.InCutscene ||
                !evenIfInvincible && (BSession.damageCooldown > 0 || SaveData.Instance.Assists.Invincible || amount <= 0) ||
                Engine.Scene.GetPlayer() is not Player entity || entity.StateMachine.State == Player.StCassetteFly)
            {
                return;
            }
            BSession.damageCooldown = baseCooldown;
            if ((BSession.currentPlayerHealth -= amount) > 0)
            {
                if (!silent)
                {
                    level.Shake();
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                    level.Flash(Color.Red * 0.3f);
                    Audio.Play("event:/char/madeline/predeath");
                    if (BSession.healthData.playerStagger && stagger)
                        Add(new Coroutine(PlayerStagger(entity.Position, direction)));
                    if (BSession.healthData.playerBlink)
                        Add(new Coroutine(PlayerInvincible()));
                    if (onDamage != null)
                        Add(new Coroutine(onDamage.ToIEnumerator()));
                }
            }
            else
            {
                entity.Die(direction);
            }

            if (HealthBar == null)
            {
                Logger.Log("Bosses Helper", "No Health Bar has been initialized");
            }
            HealthBar?.DecreaseHealth(amount);
        }

        public void RecoverHealth(int amount = 1)
        {
            BSession.currentPlayerHealth += amount;
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
            Tween tween = Tween.Set(this, Tween.TweenMode.Oneshot, BSession.damageCooldown, Ease.CubeOut, delegate
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
