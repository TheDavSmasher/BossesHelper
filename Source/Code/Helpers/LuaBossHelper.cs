﻿using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.BossesHelper.Code.Entities;
using Monocle;
using NLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    internal static class LuaBossHelper
    {
        public static readonly LuaTable cutsceneHelper = Everest.LuaLoader.Require(BossesHelperModule.Instance.Metadata.Name + ":/Assets/LuaBossHelper/cutscene_helper") as LuaTable;

        public static string GetFileContent(string path)
        {
            ModAsset file = Everest.Content.Get(path);
            Stream stream = file?.Stream;
            if (stream != null)
            {
                using (StreamReader streamReader = new(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
            return null;
        }

        public static LuaTable DictionaryToLuaTable(IDictionary<object, object> dict)
        {
            Lua context = Everest.LuaLoader.Context;
            LuaTable luaTable = context.DoString("return {}").FirstOrDefault() as LuaTable;
            foreach (KeyValuePair<object, object> item in dict)
            {
                luaTable[item.Key] = item.Value;
            }
            return luaTable;
        }

        public static LuaTable ListToLuaTable(IList list)
        {
            Lua context = Everest.LuaLoader.Context;
            LuaTable luaTable = context.DoString("return {}").FirstOrDefault() as LuaTable;
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
                Logger.Log(LogLevel.Error, "Lua Cutscenes", "Failed to resume coroutine");
                Logger.LogDetailed(e);
                return false;
            }
        }

        public static IEnumerator LuaFunctionToIEnumerator(LuaFunction func)
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

        public static Action LuaFunctionToAction(LuaFunction func)
        {
            return () => func.Call();
        }

        public static Action<T> LuaFunctionToActionEntity<T>(LuaFunction func) where T : Entity
        {
            return (entity) => func.Call(entity);
        }

        public static LuaFunction[] LoadLuaFile(string filename, string command, Dictionary<object, object> passedVals)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                LuaTable luaTable = DictionaryToLuaTable(passedVals);
                try
                {
                    object[] array = (cutsceneHelper[command] as LuaFunction).Call(filename, luaTable);
                    if (array != null)
                    {
                        return Array.ConvertAll(array.Skip(1).ToArray(), item => (LuaFunction)item);
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
            return null;
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

        public static void DoMethodAfterDelay(LuaFunction func, float delay)
        {
            Alarm.Create(Alarm.AlarmMode.Oneshot, delegate { func.Call(); }, delay, true);
        }

        public static void AddConstantBackgroundCoroutine(BossPuppet puppet, LuaFunction func)
        {
            puppet.Add(new Coroutine(LuaBossHelper.LuaFunctionToIEnumerator(func)));
        }

        public static void AddEntityColliderTo(Entity parent, object baseEntity, LuaFunction func, Collider collider = null)
        {
            Type baseType;
            if (baseEntity is string val)
            {
            baseType = LuaMethodWrappers.GetTypeFromString(val);
            }
            else if (baseEntity is Entity entity)
            {
                baseType = entity.GetType();
            }
            else
            {
                return;
            }

            Type componentType = typeof(EntityCollider<>).MakeGenericType(baseType);
            object[] componentArgs = { func, collider };
            object entityCollider = Activator.CreateInstance(componentType, componentArgs);

            parent.Add(entityCollider as EntityColliderComponent);
        }
    }
}
