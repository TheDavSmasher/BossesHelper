using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Celeste.Mod.BossesHelper.Code.Entities;
using NLua;

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
                using (StreamReader streamReader = new StreamReader(stream))
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

        public static void DoCustomSetup(string filename, Player player, BossPuppet puppet)
        {
            Dictionary<object, object> dict = new Dictionary<object, object>
            {
                { "player", player },
                { "puppet", puppet },
                { "modMetaData", BossesHelperModule.Instance.Metadata }
            };
            LoadLuaFile(filename, "setupCustomData", dict)?.ElementAtOrDefault(0)?.Call();
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
    }
}
