using LuaTableItem = (object Name, object Value);
using Celeste.Mod.BossesHelper.Code.Entities;
using NLua;
using System;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    public static class BossActions
    {
        public static void WarmUp()
        {
            Logger.Log("Bosses Helper", "Warming up Lua cutscenes");
            new BossAttack("Assets/LuaBossHelper/warmup_cutscene", null).Perform().MoveNext();
        }
    }

    public abstract class LuaFileLoader(string command, int functionCount)
    {
        public LuaFunction[] LoadFile(string filepath, BossController controller = null,
            params LuaTableItem[] values)
        {
            Dictionary<object, object> dict = new()
            {
                { "player", controller?.Scene.GetPlayer() },
                { "bossID", controller?.BossID },
                { "puppet", controller?.Puppet },
                { "boss", controller }
            };
            foreach (var (Name, Value) in values)
            {
                dict.Add(Name, Value);
            }
            return LoadLuaFile(dict, filepath, command, functionCount);
        }
    }

    public abstract class BossAction(string command, int functionCount)
        : LuaFileLoader(command, functionCount)
    {
        public abstract IEnumerator Perform();

        public virtual void EndAction(MethodEndReason reason) { }
    }

    public class BossAttack : BossAction
    {
        private readonly LuaFunction attackFunction;

        private readonly LuaFunction endFunction;

        private readonly EnumDict<MethodEndReason, LuaFunction> onEndMethods;

        public BossAttack(string filepath, BossController controller)
            : base("getAttackData", 5)
        {
            LuaFunction[] array = LoadFile(filepath, controller);
            attackFunction = array[0];
            endFunction = array[1];
            onEndMethods = new(option => array[(int)option + 2]);
        }

        public override IEnumerator Perform()
        {
            yield return attackFunction.ToIEnumerator();
        }

        public override void EndAction(MethodEndReason reason)
        {
            endFunction?.Call(reason);
            onEndMethods[reason]?.Call();
        }
    }

    public class BossEvent : BossAction
    {
        private class CutsceneWrapper(LuaFunction[] functions) : CutsceneEntity(true, false)
        {
            private readonly IEnumerator Cutscene = functions[0]?.ToIEnumerator();

            private readonly LuaFunction endMethod = functions[1];

            public override void OnBegin(Level level)
            {
                Coroutine(level).Coroutine(this);
            }

            private IEnumerator Coroutine(Level level)
            {
                yield return Cutscene;
                EndCutscene(level);
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

        private readonly CutsceneWrapper cutscene;

        private readonly BossController controller;

        public BossEvent(string filepath, BossController controller)
            : base("getCutsceneData", 2)
        {
            this.controller = controller;
            cutscene = new(LoadFile(filepath, controller, ("cutsceneEntity", cutscene)));
        }

        public override IEnumerator Perform()
        {
            controller.Scene.Add(cutscene);
            do
            {
                yield return null;
            }
            while (cutscene.Running);
        }
    }

    internal class BossFunctions : LuaFileLoader
    {
        public enum DamageSource
        {
            Contact,
            Dash,
            Bounce,
            Laser
        }

        private readonly EnumDict<DamageSource, LuaFunction> onDamageMethods;

        public BossFunctions(string filepath, BossController controller)
            : base("getInterruptData", 6)
        {
            LuaFunction[] array = LoadFile(filepath, controller);
            LuaFunction OnHitLua = array[0];
            onDamageMethods = new(option => array[(int)option + 1] ?? OnHitLua);
            array[5]?.Call();
        }

        public IEnumerator OnDamage(DamageSource source)
        {
            yield return onDamageMethods[source].ToIEnumerator();
        }
    }
}
