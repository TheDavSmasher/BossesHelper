﻿using LuaCommand = (string Name, int Count);
using Celeste.Mod.BossesHelper.Code.Entities;
using Monocle;
using NLua;
using System;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    public static class BossActions
    {
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

        public static void WarmUp()
        {
            Logger.Log("Bosses Helper", "Warming up Lua cutscenes");
            new BossAttack("Assets/LuaBossHelper/warmup_cutscene", null).Perform().MoveNext();
        }
    }

    public class BossAttack : IBossAction, ILuaLoader
    {
        private readonly LuaFunction attackFunction;

        private readonly LuaFunction endFunction;

        private readonly Dictionary<MethodEndReason, LuaFunction> onEndMethods = [];

        public LuaCommand Command => ("getAttackData", 5);

        public BossAttack(string filepath, BossController controller)
        {
            LuaFunction[] array = this.LoadFile(filepath, controller);
            attackFunction = array[0];
            endFunction = array[1];
            foreach (var option in Enum.GetValues<MethodEndReason>())
            {
                onEndMethods.Add(option, array[(int)option + 2]);
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

        private readonly Action<Entity> AddToScene;

        public LuaCommand Command => ("getCutsceneData", 2);

        public BossEvent(string filepath, BossController controller)
            : base(fadeInOnSkip: true, endingChapterAfter: false)
        {
            AddToScene = self => controller.Scene.Add(self);
            LuaFunction[] array = this.LoadFile(filepath, controller, "cutsceneEntity");
            Cutscene = array[0]?.ToIEnumerator();
            endMethod = array[1];
        }

        private IEnumerator Coroutine(Level level)
        {
            yield return Cutscene;
            EndCutscene(level);
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
            AddToScene(this);
            do
            {
                yield return null;
            }
            while (Running);
        }
    }

    public interface IBossAction
    {
        public IEnumerator Perform();

        public virtual void EndAction(MethodEndReason reason) { }
    }

    public interface ILuaLoader
    {
        public LuaCommand Command { get; }
    }

    internal class BossFunctions : ILuaLoader
    {
        public enum DamageSource
        {
            Contact,
            Dash,
            Bounce,
            Laser
        }

        private readonly LuaFunction OnContactLua;

        private readonly LuaFunction OnDashLua;

        private readonly LuaFunction OnBounceLua;

        private readonly LuaFunction OnLaserLua;

        public LuaCommand Command => ("getInterruptData", 6);

        public BossFunctions(string filepath, BossController controller)
        {
            LuaFunction[] array = this.LoadFile(filepath, controller);
            LuaFunction OnHitLua = array[0];
            OnContactLua = array[1] ?? OnHitLua;
            OnDashLua = array[2] ?? OnHitLua;
            OnBounceLua = array[3] ?? OnHitLua;
            OnLaserLua = array[4] ?? OnHitLua;
            array[5]?.Call();
        }

        public Coroutine OnDamageCoroutine(DamageSource source)
        {
            return new Coroutine(OnDamage(source));
        }

        private IEnumerator OnDamage(DamageSource source)
        {
            yield return (source switch
            {
                DamageSource.Contact => OnContactLua,
                DamageSource.Dash => OnDashLua,
                DamageSource.Bounce => OnBounceLua,
                DamageSource.Laser => OnLaserLua,
                _ => null
            })?.ToIEnumerator();
        }
    }
}
