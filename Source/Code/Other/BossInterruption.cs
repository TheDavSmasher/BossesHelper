﻿using Celeste.Mod.BossesHelper.Code.Entities;
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

        public BossController.OnHitDelegates Delegates { get; private set; }

        private void LoadMethods(string filename, Player player, BossPuppet puppet)
        {
            Dictionary<object, object> dict = new Dictionary<object, object>
            {
                { "player", player },
                { "puppet", puppet },
                { "boss", Delegates },
                { "modMetaData", BossesHelperModule.Instance.Metadata }
            };
            LuaFunction[] array = LuaBossHelper.LoadLuaFile(filename, "getInterruptData", dict);
            if (array != null)
            {
                OnHitLua = array.ElementAtOrDefault(0);
                OnDashLua = array.ElementAtOrDefault(1);
                OnBounceLua = array.ElementAtOrDefault(2);
                OnLaserLua = array.ElementAtOrDefault(3);
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
