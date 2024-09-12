using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers;
using NLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Other
{
    public class BossAttack
    {
        public LuaFunction attackFunction;

        private readonly string filepath;

        private LuaTable cutsceneEnv;

        private BossController.ControllerDelegates Delegates;

        public void LoadCutscene(string filename, Player player, BossPuppet puppet)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return;
            }
            Dictionary<object, object> dict = new Dictionary<object, object>
            {
                { "player", player },
                { "puppet", puppet },
                { "bossAttack", this },
                { "modMetaData", BossesHelperModule.Instance.Metadata }
            };
            LuaTable luaTable = LuaBossHelper.DictionaryToLuaTable(dict);
            try
            {
                object[] array = (LuaBossHelper.cutsceneHelper["getCutsceneData"] as LuaFunction).Call(filename, luaTable);
                if (array != null)
                {
                    cutsceneEnv = array.ElementAtOrDefault(0) as LuaTable;
                    attackFunction = array.ElementAtOrDefault(1) as LuaFunction;
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

        public BossAttack(string filepath, Player player, BossPuppet puppet, BossController.ControllerDelegates allDelegates)
        {
            this.filepath = filepath;
            Delegates = allDelegates;
            LoadCutscene(filepath, player, puppet);
        }

        public IEnumerator Coroutine()
        {
            yield return LuaBossHelper.LuaFunctionToIEnumerator(attackFunction);
        }
    }
}
