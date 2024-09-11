using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLua;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    internal static class LuaBossHelper
    {
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

        public static IEnumerator LuaCoroutineToIEnumerator(LuaCoroutine routine)
        {
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
    }
}
