﻿using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using NLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code
{
	namespace Helpers
	{
		public class LuaException(string message) : Exception(message) { }

		internal static class LuaBossHelper
		{
			public static readonly LuaTable cutsceneHelper = Everest.LuaLoader.Require(
				BossesHelperModule.Instance.Metadata.Name + ":/Assets/LuaBossHelper/cutscene_helper") as LuaTable;

			public static string GetFileContent(string path)
			{
				ModAsset file = Everest.Content.Get(path);
				Stream stream = file?.Stream;
				if (stream != null)
				{
					using StreamReader streamReader = new(stream);
					return streamReader.ReadToEnd();
				}
				return null;
			}

			public static LuaTable DictionaryToLuaTable(IDictionary<object, object> dict)
			{
				LuaTable luaTable = GetEmptyTable();
				foreach (KeyValuePair<object, object> item in dict)
				{
					luaTable[item.Key] = item.Value;
				}
				return luaTable;
			}

			public static LuaTable ListToLuaTable(IList list)
			{
				LuaTable luaTable = GetEmptyTable();
				int num = 1;
				foreach (object item in list)
				{
					luaTable[num++] = item;
				}
				return luaTable;
			}

			private static bool SafeMoveNext(this LuaCoroutine enumerator)
			{
				try
				{
					return enumerator.MoveNext();
				}
				catch (Exception e)
				{
					Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to resume coroutine");
					Logger.LogDetailed(e);
					return false;
				}
			}

			public static IEnumerator ToIEnumerator(this LuaFunction func)
			{
				LuaCoroutine routine = (cutsceneHelper["setFuncAsCoroutine"] as LuaFunction)
					.Call(func).ElementAtOrDefault(0) as LuaCoroutine;
				while (routine != null && SafeMoveNext(routine))
				{
					if (routine.Current is double || routine.Current is long)
					{
						yield return Convert.ToSingle(routine.Current);
					}
					else
					{
						yield return routine.Current;
					}
				}
				yield return null;
			}

			public static void AddAsCoroutine(this LuaFunction function, Entity target)
			{
				function.ToIEnumerator().Coroutine(target);
			}

			public static LuaFunction[] LoadFile<T>(this T self, string filename) where T : ILuaLoader
			{
				Dictionary<object, object> passedVals = self.Values.ToDictionary();
				LuaFunction[] funcs = null;
				if (!string.IsNullOrEmpty(filename))
				{
					passedVals.Add("modMetaData", BossesHelperModule.Instance.Metadata);
					try
					{
						if ((cutsceneHelper[self.Command.Name] as LuaFunction)
							.Call(filename, DictionaryToLuaTable(passedVals)) is object[] array)
						{
							funcs = [.. array.Skip(1).Cast<LuaFunction>()];
						}
						else
						{
							Logger.Log("Bosses Helper", "Failed to load Lua Cutscene, target file does not exist: \"" + filename + "\"");
						}
					}
					catch (Exception e)
					{
						Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to execute cutscene in C#: {e}");
					}
				}
				Array.Resize(ref funcs, self.Command.Count);
				return funcs;
			}

			public static ColliderList GetColliderListFromLuaTable(LuaTable luaTable)
			{
				return new ColliderList([.. luaTable.Values.OfType<Collider>()]);
			}

			public static IEnumerator Say(string dialog, LuaTable luaEvents)
			{
				return Textbox.Say(dialog, [.. luaEvents.Values.OfType<LuaFunction>()
					.Select<LuaFunction, Func<IEnumerator>>(luaEv => luaEv.ToIEnumerator)]);
			}

			public static LuaTable GetEmptyTable()
			{
				return Everest.LuaLoader.Context.DoString("return {}").FirstOrDefault() as LuaTable;
			}

			public static void DoMethodAfterDelay(LuaFunction func, float delay)
			{
				Alarm.Create(Alarm.AlarmMode.Oneshot, () => func.Call(), delay, true);
			}

			public static void AddConstantBackgroundCoroutine(BossPuppet puppet, LuaFunction func)
			{
				func.AddAsCoroutine(puppet);
			}
		}
	}

	namespace Entities
	{
		public partial class BossController
		{
			public IEnumerator WaitForAttackToEnd()
			{
				while (isActing)
				{
					yield return null;
				}
			}

			public void SavePhaseChangeInSession(int health, int patternIndex, bool startImmediately)
			{
				BossesHelperModule.Session.BossPhasesSaved[BossID] =
					new(health, startImmediately, patternIndex);
			}

			public void RemoveBoss(bool permanent)
			{
				RemoveSelf();
				if (permanent)
				{
					Scene.DoNotLoad(SourceId);
				}
			}

			public int GetCurrentPatternIndex()
			{
				return currentPatternIndex;
			}

			public int GetHealth()
			{
				return Health;
			}

			public void SetHealth(int val)
			{
				Health = val;
			}

			public void DecreaseHealth(int val = 1)
			{
				Health -= val;
			}

			public void ForceNextAttackIndex(int index)
			{
				forcedAttackIndex = index;
			}

			public void AddEntity(Entity entity)
			{
				if (!activeEntities.Contains(entity))
				{
					Scene.Add(entity);
					activeEntities.Add(entity);
					entity.Scene = Scene;
				}
			}

			public void DestroyEntity(Entity entity)
			{
				if (activeEntities.Remove(entity))
				{
					entity.RemoveSelf();
				}
			}
		}

		public partial class BossPuppet
		{
			public IEnumerator WaitBossAnim(string anim)
			{
				if (Sprite != null && Sprite.Has(anim))
				{
					yield return Sprite.PlayAnim(anim);
				}
			}

			public void SetGravityMult(float mult)
			{
				effectiveGravity = Gravity * mult;
			}

			public void SetXSpeed(float speed)
			{
				Speed.X = speed;
			}

			public void SetYSpeed(float speed)
			{
				Speed.Y = speed;
			}

			public void SetSpeed(float x, float y)
			{
				Speed.Y = y;
				Speed.X = x;
			}

			public float SetXSpeedDuring(float speed, float time)
			{
				KeepXSpeed(speed, time).Coroutine(this);
				return time;
			}

			public float SetYSpeedDuring(float speed, float time)
			{
				KeepYSpeed(speed, time).Coroutine(this);
				return time;
			}

			public float SetSpeedDuring(float x, float y, float time)
			{
				SetXSpeedDuring(x, time);
				SetYSpeedDuring(y, time);
				return time;
			}

			private IEnumerator KeepXSpeed(float speed, float time)
			{
				float timer = 0;
				while (timer < time)
				{
					Speed.X = speed;
					timer += Engine.DeltaTime;
					yield return null;
				}
			}

			private IEnumerator KeepYSpeed(float speed, float time)
			{
				float timer = 0;
				while (timer < time)
				{
					Speed.Y = speed;
					timer += Engine.DeltaTime;
					yield return null;
				}
			}

			public void SetBossHitCooldown(float timer)
			{
				BossHitCooldown = timer;
			}

			public void ChangeHitboxOption(string tag)
			{
				Collider = GetTagOrDefault(ColliderOption.Hitboxes, tag, Sprite.Height);
			}

			public void ChangeHurtboxOption(string tag)
			{
				Hurtbox = GetTagOrDefault(ColliderOption.Hurtboxes, tag, Sprite.Height);
				if (BossCollision is PlayerCollider collider)
				{
					collider.Collider = Hurtbox;
				}
			}

			public void ChangeBounceboxOption(string tag)
			{
				Bouncebox = GetTagOrDefault(ColliderOption.Bouncebox, tag, 6f);
				if (BossCollision is PlayerCollider collider)
				{
					collider.Collider = Bouncebox;
				}
			}

			public void ChangeTargetOption(string tag)
			{
				Target = GetTagOrDefault(ColliderOption.Target, tag, null);
				if (BossCollision is SidekickTarget target)
				{
					target.Collider = Target;
				}
			}

			public void PositionTween(Vector2 target, float time, Ease.Easer easer = null)
			{
				Tween.Position(this, target, time, easer);
			}

			public void Speed1DTween(float start, float target, float time, bool isX, Ease.Easer easer = null)
			{
				Tween tween = Tween.Create(Tween.TweenMode.Oneshot, easer, time, true);
				tween.OnUpdate = t => (isX ? ref Speed.X : ref Speed.Y) = start + (target - start) * t.Eased;
				Add(tween);
			}

			public void StoreObject(string key, object toStore)
			{
				storedObjects.TryAdd(key, toStore);
			}

			public object GetStoredObject(string key)
			{
				return storedObjects.TryGetValue(key, out object storedObject) ? storedObject : null;
			}

			public void DeleteStoredObject(string key)
			{
				storedObjects.Remove(key);
			}
		}
	}
}
