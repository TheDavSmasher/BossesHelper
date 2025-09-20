﻿using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using NLua;
using System;
using System.Collections;
using static Celeste.Mod.BossesHelper.Code.Entities.HealthDisplays;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
	public partial class HealthSystemManager
	{
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
		private class DamageController() : Entity, ILuaLoader
		{
			private LuaFunction onRecover;

			private LuaFunction onDamage;

			private Stopwatch DamageCooldown => Scene.GetPlayer()?.Get<Stopwatch>();

			public LuaCommand Command => ("getFunctionData", 2);

			public LuaTableItem[] Values { get; set; }

			public void UpdateState(PlayerHealthBar healthBar)
			{
				this.ChangeTagState(Tags.Global, HealthData.globalController);
				Player player = Scene.GetPlayer();
				Values = [("player", player), ("healthBar", healthBar)];
				LuaFunction[] array = this.LoadFile(HealthData.onDamageFunction);
				onDamage = array[0];
				onRecover = array[1];
				player.AddIFramesWatch();
			}

			public int TakeDamage(Vector2 direction, int amount = 1, bool silent = false, bool stagger = true, bool evenIfInvincible = false)
			{
				Level Level = SceneAs<Level>();
				if (Scene.GetPlayer() is not Player entity ||
					entity.StateMachine.State == Player.StCassetteFly ||
					Level.InCutscene ||
					!evenIfInvincible && (!DamageCooldown.Finished || SaveData.Instance.Assists.Invincible || amount <= 0)
					)
				{
					return 0;
				}
				DamageCooldown.Reset();
				if ((ModSession.currentPlayerHealth -= amount) > 0)
				{
					if (!silent)
					{
						Level.Shake();
						Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
						Level.Flash(Color.Red * 0.3f);
						Audio.Play("event:/char/madeline/predeath");
						if (HealthData.playerStagger && stagger)
							PlayerStagger(entity.Position, direction).AsCoroutine(entity);
						if (HealthData.playerBlink)
							PlayerInvincible().AsCoroutine(entity);
						AddLuaCoroutine(onDamage);
					}
				}
				else
				{
					entity.Die(direction);
				}
				return amount + Math.Min(ModSession.currentPlayerHealth, 0);
			}

			public void RecoverHealth(int amount = 1)
			{
				ModSession.currentPlayerHealth += amount;
				AddLuaCoroutine(onRecover);
			}

			public void RefillHealth()
			{
				ModSession.currentPlayerHealth = HealthData.playerHealthVal;
				AddLuaCoroutine(onRecover);
			}

			private void AddLuaCoroutine(LuaFunction func) => Add(new LuaCoroutineComponent(func));

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
						if (Scene.GetPlayer() is Player player)
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
				void ChangeVisible(bool state)
				{
					if (Scene.GetPlayer() is Player player)
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
}
