using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers;
using NLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Celeste.Mod.BossesHelper.Code.Other
{
    internal class BossInterruption
    {
        private LuaFunction OnHitLua;

        private readonly string filepath;

        private LuaTable cutsceneEnv;

        public BossController.OnHitDelegates Delegates { get; private set; }

        private void LoadCutscene(string filename, Player player, BossPuppet puppet)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return;
            }
            Dictionary<object, object> dict = new Dictionary<object, object>
                {
                    { "player", player },
                    { "puppet", puppet },
                    { "boss", Delegates },
                    { "modMetaData", BossesHelperModule.Instance.Metadata }
                };
            LuaTable luaTable = LuaBossHelper.DictionaryToLuaTable(dict);
            try
            {
                object[] array = (LuaBossHelper.cutsceneHelper["getCutsceneData"] as LuaFunction).Call(filename, luaTable);
                if (array != null)
                {
                    OnHitLua = array.ElementAtOrDefault(1) as LuaFunction;
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

        public BossInterruption(string filepath, Player player, BossPuppet puppet, BossController.OnHitDelegates delegates)
        {
            this.filepath = filepath;
            Delegates = delegates;
            LoadCutscene(filepath, player, puppet);
        }

        public IEnumerator OnHit()
        {
            yield return LuaBossHelper.LuaFunctionToIEnumerator(OnHitLua);
        }
    }
}
