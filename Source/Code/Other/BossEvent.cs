﻿using System;
using System.Collections;
using Monocle;
using Celeste.Mod.BossesHelper.Code.Entities;
using NLua;
using Celeste.Mod.BossesHelper.Code.Helpers;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;

namespace Celeste.Mod.BossesHelper.Code.Other
{
    public class BossEvent : CutsceneEntity
    {
        public readonly BossController.CustceneDelegates CustceneDelegates;

        private readonly IEnumerator Cutscene;

        private readonly LuaFunction endMethod;

        public BossEvent(string filepath, string bossId = null, Player player = null,
            BossPuppet puppet = null, BossController.CustceneDelegates delegates = default)
            : base(fadeInOnSkip: true, endingChapterAfter: false)
        {
            CustceneDelegates = delegates;
            Dictionary<object, object> dict = new Dictionary<object, object>
            {
                { "player", player },
                { "bossID", bossId },
                { "puppet", puppet },
                { "cutsceneEntity", this },
                { "modMetaData", BossesHelperModule.Instance.Metadata }
            };
            LuaFunction[] array = LoadLuaFile(filepath, "getCutsceneData", dict);
            if (array != null)
            {
                Cutscene = array.ElementAtOrDefault(0)?.ToIEnumerator();
                endMethod = array.ElementAtOrDefault(1);
            }
        }

        public static void WarmUp()
        {
            Logger.Log("Bosses Helper", "Warming up Lua cutscenes");
            Coroutine coroutine = new(new BossEvent("Assets/LuaBossHelper/warmup_cutscene").Coroutine(null));
            while (!coroutine.Finished)
            {
                coroutine.Update();
            }
        }

        private IEnumerator Coroutine(Level level)
        {
            yield return Cutscene;
            if (level != null)
            {
                EndCutscene(level);
            }
        }

        public override void OnBegin(Level level)
        {
            if (Cutscene != null)
            {
                Add(new Coroutine(Coroutine(level)));
            }
        }

        public override void OnEnd(Level level)
        {
            try
            {
                endMethod?.Call(level, WasSkipped);
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to call OnEnd");
                Logger.LogDetailed(e);
            }
        }
    }
}
