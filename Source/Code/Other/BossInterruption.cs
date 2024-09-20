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

        private LuaFunction OnDashLua;

        private LuaFunction OnBounceLua;

        private LuaFunction OnLaserLua;

        private readonly string filepath;

        private LuaTable cutsceneEnv;

        public BossController.OnHitDelegates Delegates { get; private set; }

        private void LoadMethods(string filename, Player player, BossPuppet puppet)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return;
            }
            LuaTable luaTable = LuaBossHelper.DictionaryToLuaTable(new Dictionary<object, object>
                {
                    { "player", player },
                    { "puppet", puppet },
                    { "boss", Delegates },
                    { "modMetaData", BossesHelperModule.Instance.Metadata }
                });
            try
            {
                object[] array = (LuaBossHelper.cutsceneHelper["getInterruptData"] as LuaFunction).Call(filename, luaTable);
                if (array != null)
                {
                    OnHitLua = array.ElementAtOrDefault(1) as LuaFunction;
                    OnDashLua = array.ElementAtOrDefault(2) as LuaFunction;
                    OnBounceLua = array.ElementAtOrDefault(3) as LuaFunction;
                    OnLaserLua = array.ElementAtOrDefault(4) as LuaFunction;
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

        public BossInterruption(string filepath, BossController.OnHitDelegates delegates)
        {
            this.filepath = filepath;
            Delegates = delegates;
            LoadMethods(filepath, delegates.playerRef, delegates.puppetRef);
        }

        public IEnumerator OnHitCoroutine()
        {
            if (OnHitLua != null)
            yield return LuaBossHelper.LuaFunctionToIEnumerator(OnHitLua);
        }

        public IEnumerator OnDashCoroutine()
        {
            if (OnDashLua != null)
            yield return LuaBossHelper.LuaFunctionToIEnumerator(OnDashLua);
        }

        public IEnumerator OnBounceCoroutine()
        {
            if (OnBounceLua != null)
            yield return LuaBossHelper.LuaFunctionToIEnumerator(OnBounceLua);
        }

        public IEnumerator OnLaserCoroutine()
        {
            if (OnLaserLua != null)
            yield return LuaBossHelper.LuaFunctionToIEnumerator(OnLaserLua);
        }
    }
}
