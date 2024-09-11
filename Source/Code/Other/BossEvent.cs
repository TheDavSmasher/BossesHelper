﻿using System;
using System.Collections;
using Monocle;
using Celeste.Mod.BossesHelper.Code.Entities;
using NLua;
using Celeste.Mod.BossesHelper.Code.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Other
{
    public class BossEvent : CutsceneEntity
    {
        private static readonly LuaTable cutsceneHelper = Everest.LuaLoader.Require(BossesHelperModule.Instance.Metadata.Name + ":/Assets/LuaBossHelper/cutscene_helper") as LuaTable;

        private readonly string filepath;

        public bool finished;

        private LuaTable cutsceneEnv;

        private readonly BossPuppet puppet;

        private readonly Player player;

        private IEnumerator Cutscene;

        private LuaFunction endMethod;

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
                { "cutsceneEntity", this },
                { "modMetaData", BossesHelperModule.Instance.Metadata }
            };
            LuaTable luaTable = LuaBossHelper.DictionaryToLuaTable(dict);
            try
            {
                LuaFunction func = cutsceneHelper["getCutsceneData"] as LuaFunction;
                object[] array = func.Call(filename, luaTable);
                if (array != null)
                {
                    cutsceneEnv = array.ElementAtOrDefault(0) as LuaTable;
                    Cutscene = LuaBossHelper.LuaCoroutineToIEnumerator((cutsceneHelper["setFuncAsCoroutine"] as LuaFunction).Call(array.ElementAtOrDefault(1) as LuaFunction).ElementAtOrDefault(0) as LuaCoroutine);
                    endMethod = array.ElementAtOrDefault(2) as LuaFunction;
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

        public BossEvent(string filepath, Player player, BossPuppet puppet, bool fadeInOnSkip = true, bool endingChapterAfter = false)
            : base(fadeInOnSkip, endingChapterAfter)
        {
            this.filepath = filepath;
            this.player = player;
            this.puppet = puppet;
            finished = false;
            LoadCutscene(this.filepath);
        }

        public static void WarmUp()
        {
            Logger.Log("Bosses Helper", "Warming up Lua cutscenes");
            BossEvent bossEvent = new BossEvent("Assets/LuaBossHelper/warmup_cutscene", null, null);
            Coroutine coroutine = new Coroutine(bossEvent.Coroutine(null));
            try
            {
                while (!coroutine.Finished)
                {
                    coroutine.Update();
                }
            }
            catch
            {
            }
        }

        private IEnumerator Coroutine(Level level)
        {
            yield return Cutscene;
            EndCutscene(level);
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
                finished = true;
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to call OnEnd");
                Logger.LogDetailed(e);
            }
        }
    }
}