using LuaCommand = (string Name, int Count);
using LuaTableItem = (object Key, object Value);
using Celeste.Mod.BossesHelper.Code.Entities;
using NLua;
using System;
using System.Linq;
using System.Collections;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    public interface ILuaLoader
    {
        LuaCommand Command { get; }

        LuaTableItem[] Values { get; }

        public LuaFunction[] LoadFile(string filepath)
        {
            return LoadLuaFile(Values.ToDictionary(), filepath, Command.Name, Command.Count);
        }
    }

    public interface IBossAction
    {
        IEnumerator Perform();

        void EndAction(MethodEndReason reason) { }
    }

    public static class BossActions
    {
        public static void WarmUp()
        {
            Logger.Log("Bosses Helper", "Warming up Lua cutscenes");
            new BossAttack("Assets/LuaBossHelper/warmup_cutscene", null).Perform().MoveNext();
        }
    }

    public abstract class BossLuaLoader(string filepath, BossController controller = null) : ILuaLoader
    {
        public abstract LuaCommand Command { get; }

        public virtual LuaTableItem[] Values =>
        [
            ( "player", controller?.Scene.GetPlayer() ),
            ( "bossID", controller?.BossID ),
            ( "puppet", controller?.Puppet ),
            ( "boss", controller )
        ];

        protected LuaFunction[] LoadLuaBossFile()
        {
            return (this as ILuaLoader).LoadFile(filepath);
        }
    }

    public class BossAttack : BossLuaLoader, IBossAction
    {
        private readonly LuaFunction attackFunction;

        private readonly LuaFunction endFunction;

        private readonly EnumDict<MethodEndReason, LuaFunction> onEndMethods;

        public override LuaCommand Command => ("getAttackData", 5);

        public BossAttack(string filepath, BossController controller)
            : base(filepath, controller)
        {
            LuaFunction[] array = LoadLuaBossFile();
            attackFunction = array[0];
            endFunction = array[1];
            onEndMethods = new(option => array[(int)option + 2]);
        }

        public IEnumerator Perform()
        {
            return attackFunction.ToIEnumerator();
        }

        public void EndAction(MethodEndReason reason)
        {
            endFunction?.Call(reason);
            onEndMethods[reason]?.Call();
        }
    }

    public class BossEvent : BossLuaLoader, IBossAction
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

        public override LuaCommand Command => ("getCutsceneData", 2);

        public override LuaTableItem[] Values => [("cutsceneEntity", cutscene), ..base.Values];

        public BossEvent(string filepath, BossController controller)
            : base(filepath, controller)
        {
            this.controller = controller;
            cutscene = new(LoadLuaBossFile());
        }

        public IEnumerator Perform()
        {
            controller.Scene.Add(cutscene);
            do
            {
                yield return null;
            }
            while (cutscene.Running);
        }
    }

    internal class BossFunctions : BossLuaLoader
    {
        public enum DamageSource
        {
            Contact,
            Dash,
            Bounce,
            Laser
        }

        private readonly EnumDict<DamageSource, LuaFunction> onDamageMethods;

        public override LuaCommand Command => ("getInterruptData", 6);

        public BossFunctions(string filepath, BossController controller)
            : base(filepath, controller)
        {
            LuaFunction[] array = LoadLuaBossFile();
            LuaFunction OnHitLua = array[0];
            onDamageMethods = new(option => array[(int)option + 1] ?? OnHitLua);
            array[5]?.Call();
        }

        public IEnumerator OnDamage(DamageSource source)
        {
            return onDamageMethods[source].ToIEnumerator();
        }
    }
}
