using Monocle;
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
        private static readonly LuaTable cutsceneHelper = Everest.LuaLoader.Require(BossesHelperModule.Instance.Metadata.Name + ":/Assets/LuaBossHelper/cutscene_helper") as LuaTable;

        public LuaFunction attackFunction;

        private readonly string filepath;

        private LuaTable cutsceneEnv;

        private readonly Player player;

        private readonly BossPuppet puppet;

        private BossController.ControllerDelegates Delegates;

        public void LoadCutscene(string filename)
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
                object[] array = (cutsceneHelper["getCutsceneData"] as LuaFunction).Call(filename, luaTable);
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
            this.player = player;
            this.puppet = puppet;
            Delegates = allDelegates;
            LoadCutscene(filepath);
        }

        public IEnumerator Coroutine()
        {
            yield return LuaBossHelper.LuaCoroutineToIEnumerator((cutsceneHelper["setFuncAsCoroutine"] as LuaFunction).Call(attackFunction).ElementAtOrDefault(0) as LuaCoroutine);
        }

        public void AddEntity(Entity entity)
        {
            Delegates.addEntity(entity);
        }

        public void AddEntity(Entity entity, string name, Action<Entity> action, float timer)
        {
            Delegates.addEntityWithTimer(entity, name, action, timer);
        }

        public void AddEntity(Entity entity, string flag, Action<Entity> action, bool state = true, bool resetFlag = true)
        {
            Delegates.addEntityWithFlagger(entity, flag, action, state, resetFlag);
        }
    }
}
