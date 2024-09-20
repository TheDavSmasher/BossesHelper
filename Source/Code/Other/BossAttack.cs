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
        private LuaFunction attackFunction;

        private readonly string filepath;

        private LuaTable cutsceneEnv;

        public BossController.AttackDelegates Delegates { get; private set; }

        private void LoadAttacks(string filename, Player player, BossPuppet puppet)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return;
            }
            LuaTable luaTable = LuaBossHelper.DictionaryToLuaTable(new Dictionary<object, object>
            {
                { "player", player },
                { "puppet", puppet },
                { "bossAttack", Delegates },
                { "modMetaData", BossesHelperModule.Instance.Metadata }
            });
            try
            {
                object[] array = (LuaBossHelper.cutsceneHelper["getAttackData"] as LuaFunction).Call(filename, luaTable);
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

        public BossAttack(string filepath, BossController.AttackDelegates allDelegates)
        {
            this.filepath = filepath;
            Delegates = allDelegates;
            LoadAttacks(filepath, allDelegates.playerRef, allDelegates.puppetRef);
        }

        public IEnumerator Coroutine()
        {
            yield return LuaBossHelper.LuaFunctionToIEnumerator(attackFunction);
        }
    }
}
