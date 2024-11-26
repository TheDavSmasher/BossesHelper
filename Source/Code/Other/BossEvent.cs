using System;
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
        public readonly BossController.CustceneDelegates CustceneDelegates;

        public bool finished;

        private IEnumerator Cutscene;

        private LuaFunction endMethod;

        private void LoadCutscene(string filename, string bossId, Player player, BossPuppet puppet)
        {
            Dictionary<object, object> dict = new Dictionary<object, object>
            {
                { "player", player },
                { "bossID", bossId },
                { "puppet", puppet },
                { "cutsceneEntity", this },
                { "modMetaData", BossesHelperModule.Instance.Metadata }
            };
            LuaFunction[] array = LuaBossHelper.LoadLuaFile(filename, "getCutsceneData", dict);
            if (array != null)
            {
                Cutscene = LuaBossHelper.LuaFunctionToIEnumerator(array.ElementAtOrDefault(0));
                endMethod = array.ElementAtOrDefault(1);
            }
        }

        public BossEvent(string filepath, string bossId, Player player, BossPuppet puppet, BossController.CustceneDelegates delegates,
            bool fadeInOnSkip = true, bool endingChapterAfter = false)
            : base(fadeInOnSkip, endingChapterAfter)
        {
            finished = false;
            CustceneDelegates = delegates;
            LoadCutscene(filepath, bossId, player, puppet);
        }

        private BossEvent(string filepath)
        {
            finished = false;
            LoadCutscene(filepath, null, null, null);
        }

        public static void WarmUp()
        {
            Logger.Log("Bosses Helper", "Warming up Lua cutscenes");
            BossEvent bossEvent = new("Assets/LuaBossHelper/warmup_cutscene");
            Coroutine coroutine = new(bossEvent.Coroutine(null));
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
            finished = true;
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
