using Celeste.Mod.BossesHelper.Code.Components;
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
                LuaCoroutine routine = (cutsceneHelper["setFuncAsCoroutine"] as LuaFunction).Call(func).ElementAtOrDefault(0) as LuaCoroutine;
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

            public static LuaFunction[] LoadLuaFile(Dictionary<object, object> passedVals, string filename, string command, int count = 1)
            {
                LuaFunction[] funcs = null;
                if (!string.IsNullOrEmpty(filename))
                {
                    try
                    {
                        if ((cutsceneHelper[command] as LuaFunction).Call(filename, DictionaryToLuaTable(passedVals)) is object[] array)
                        {
                            funcs = Array.ConvertAll(array.Skip(1).ToArray(), item => (LuaFunction)item);
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
                Array.Resize(ref funcs, count);
                return funcs;
            }

            public static ColliderList GetColliderListFromLuaTable(LuaTable luaTable)
            {
                List<Collider> colliders = new List<Collider>();
                foreach (object colliderVal in luaTable.Values)
                {
                    if (colliderVal is Collider collider)
                    {
                        colliders.Add(collider);
                    }
                }
                return new ColliderList(colliders.ToArray());
            }

            public static IEnumerator Say(string dialog, LuaTable luaEvents)
            {
                List<Func<IEnumerator>> events = new();
                foreach (object luaEvent in luaEvents.Values)
                {
                    if (luaEvent is LuaFunction luaFunction)
                    {
                        events.Add(luaFunction.ToIEnumerator);
                    }
                }
                yield return Textbox.Say(dialog, [.. events]);
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
                puppet.Add(new Coroutine(func.ToIEnumerator()));
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
                BossesHelperModule.Session.BossPhasesSaved.Add(BossID,
                    new BossesHelperSession.BossPhase(health, startImmediately, patternIndex));
            }

            public void RemoveBoss(bool permanent)
            {
                RemoveSelf();
                if (permanent)
                {
                    SceneAs<Level>().Session.DoNotLoad.Add(id);
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
                Add(new Coroutine(KeepXSpeed(speed, time)));
                return time;
            }

            public float SetYSpeedDuring(float speed, float time)
            {
                Add(new Coroutine(KeepYSpeed(speed, time)));
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
                if (bossCollision is PlayerCollider collider)
                {
                    collider.Collider = Hurtbox;
                }
            }

            public void ChangeBounceboxOption(string tag)
            {
                Bouncebox = GetTagOrDefault(ColliderOption.Bouncebox, tag, 6f);
                if (bossCollision is PlayerCollider collider)
                {
                    collider.Collider = Bouncebox;
                }
            }

            public void ChangeTargetOption(string tag)
            {
                Target = GetTagOrDefault(ColliderOption.Target, tag, null);
                if (bossCollision is SidekickTarget target)
                {
                    target.Collider = Target;
                }
            }

            public float PositionTween(Vector2 target, float time, Ease.Easer easer = null)
            {
                Tween.Position(this, target, time, easer);
                return time;
            }

            public float SpeedXTween(float start, float target, float time, Ease.Easer easer = null)
            {
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, easer, time, true);
                tween.OnUpdate = (Tween t) => Speed.X = start + (target - start) * t.Eased;
                Add(tween);
                return time;
            }

            public float SpeedYTween(float start, float target, float time, Ease.Easer easer = null)
            {
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, easer, time, true);
                tween.OnUpdate = (Tween t) => Speed.Y = start + (target - start) * t.Eased;
                Add(tween);
                return time;
            }

            public float SpeedTween(float xStart, float yStart, float xTarget, float yTarget, float time, Ease.Easer easer = null)
            {
                SpeedXTween(xStart, xTarget, time, easer);
                SpeedYTween(yStart, yTarget, time, easer);
                return time;
            }

            public void StoreObject(string key, object toStore)
            {
                if (!storedObjects.ContainsKey(key))
                    storedObjects.Add(key, toStore);
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
