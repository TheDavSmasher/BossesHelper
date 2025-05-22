using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers;
using NLua;
using System;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;
using static Celeste.Mod.BossesHelper.Code.Other.BossPatterns;

namespace Celeste.Mod.BossesHelper.Code.Other
{
    public static class BossActions
    {
        public interface IBossAction
        {
            public IEnumerator Perform();

            public void EndAction(MethodEndReason reason);
        }

        public class BossAttack : IBossAction
        {
            private readonly LuaFunction endFunction;

            private readonly LuaFunction attackFunction;

            public BossAttack(string filepath, Player player, BossController controller)
            {
                LuaFunction[] array = LoadLuaFile(new Dictionary<object, object>
                {
                    { "player", player },
                    { "bossID", controller.BossID },
                    { "puppet", controller.Puppet },
                    { "boss", controller },
                    { "modMetaData", BossesHelperModule.Instance.Metadata }
                },
                filepath, "getAttackData", 2);
                attackFunction = array[0];
                endFunction = array[1];
            }

            public IEnumerator Perform()
            {
                yield return attackFunction.ToIEnumerator();
            }

            public void EndAction(MethodEndReason reason)
            {
                endFunction?.Call(reason);
            }
        }

        public class BossEvent : CutsceneEntity, IBossAction
        {
            private readonly IEnumerator Cutscene;

            private readonly LuaFunction endMethod;

            private readonly Action AddToScene;

            public BossEvent(string filepath, Player player = null, BossController controller = null)
                : base(fadeInOnSkip: true, endingChapterAfter: false)
            {
                AddToScene = () => controller.Scene.Add(this);
                LuaFunction[] array = LoadLuaFile(new Dictionary<object, object>
                {
                    { "player", player },
                    { "bossID", controller?.BossID },
                    { "puppet", controller?.Puppet },
                    { "boss", controller },
                    { "cutsceneEntity", this },
                    { "modMetaData", BossesHelperModule.Instance.Metadata }
                },
                filepath, "getCutsceneData", 2);
                Cutscene = array[0]?.ToIEnumerator();
                endMethod = array[1];
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
                Add(new Coroutine(Coroutine(level)));
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

            public IEnumerator Perform()
            {
                AddToScene();
                do
                {
                    yield return null;
                }
                while (Running);
            }

            public void EndAction(MethodEndReason reason) { }
        }
    }
}
