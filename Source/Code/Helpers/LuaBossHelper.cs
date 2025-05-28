using Celeste.Mod.BossesHelper.Code.Entities;
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
    }
}
