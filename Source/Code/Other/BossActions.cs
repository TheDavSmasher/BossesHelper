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

            public virtual void EndAction(MethodEndReason reason) { }
        }

        public interface ILuaLoader
        {
            public (string Name, int Count) Command { get; }
        }

        public static LuaFunction[] LoadFile(this ILuaLoader self, string filepath, BossController controller = null, string selfName = null)
        {
            Dictionary<object, object> dict = new()
            {
                { "player", controller?.Scene.GetPlayer() },
                { "bossID", controller?.BossID },
                { "puppet", controller?.Puppet },
                { "boss", controller }
            };
            if (selfName != null)
                dict.Add(selfName, self);
            return LoadLuaFile(dict, filepath, self.Command.Name, self.Command.Count);
        }

        public class BossAttack : IBossAction, ILuaLoader
        {
            private readonly LuaFunction attackFunction;

            private readonly LuaFunction endFunction;

            private readonly Dictionary<MethodEndReason, LuaFunction> onEndMethods = [];

            (string Name, int Count) ILuaLoader.Command => ("getAttackData", 5);

            public BossAttack(string filepath, BossController controller)
            {
                LuaFunction[] array = this.LoadFile(filepath, controller);
                attackFunction = array[0];
                endFunction = array[1];
                foreach (var option in Enum.GetValues<MethodEndReason>())
                {
                    onEndMethods.Add(option, array[(int) option + 2]);
                }
            }

            public IEnumerator Perform()
            {
                yield return attackFunction.ToIEnumerator();
            }

            public void EndAction(MethodEndReason reason)
            {
                endFunction?.Call(reason);
                onEndMethods[reason]?.Call();
            }
        }

        public class BossEvent : CutsceneEntity, IBossAction, ILuaLoader
        {
            private readonly IEnumerator Cutscene;

            private readonly LuaFunction endMethod;

            private readonly Action AddToScene;

            public (string Name, int Count) Command => ("getCutsceneData", 2);

            public BossEvent(string filepath, BossController controller = null)
                : base(fadeInOnSkip: true, endingChapterAfter: false)
            {
                AddToScene = () => controller.Scene.Add(this);
                LuaFunction[] array = this.LoadFile(filepath, controller, "cutsceneEntity");
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
        }
    }
}
