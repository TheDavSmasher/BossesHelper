using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers.Lua;
using Monocle;
using NLua;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
	public interface IBossAction
	{
		IEnumerator Perform();
	}

	public interface ILuaBossAction : IBossAction, ILuaLoader { }

	public abstract class BossLuaLoader(BossController controller) : ILuaLoader
	{
		public abstract PrepareMode Mode { get; }

		public abstract void Initialize(LuaFunction[] funcs);

		public Scene Scene => controller.Scene;

		public Dictionary<string, object> Values { get; init; } = new()
		{
			{ "boss", controller },
			{ "bossID", controller.BossID },
			{ "puppet", controller.Puppet },
			{ "sidekick", controller.Scene.GetEntity<BadelineSidekick>() }
		};
	}

	public class BossAttack(BossController controller)
		: BossLuaLoader(controller), ILuaBossAction
	{
		private LuaFunction attackFunction;

		private LuaFunction endFunction;

		public enum EndReason
		{
			Completed,
			Interrupted,
			PlayerDied
		}

		private EnumDict<EndReason, LuaFunction> onEndMethods;

		public override PrepareMode Mode => PrepareMode.Attack;

		public override void Initialize(LuaFunction[] funcs)
		{
			attackFunction = funcs[0];
			endFunction = funcs[1];
			onEndMethods = new(option => funcs[(int)option + 2]);
		}

		public IEnumerator Perform()
		{
			return new LuaProxyCoroutine(attackFunction);
		}

		public void End(EndReason reason)
		{
			endFunction?.Call(reason);
			onEndMethods[reason]?.Call();
		}

		public static BossAttack Create(BossController controller) => new(controller);
	}

	public class BossEvent : BossLuaLoader, ILuaBossAction
	{
		private class CutsceneWrapper : CutsceneEntity
		{
			public LuaFunction StartFunction;

			public LuaFunction EndFunction;

			public override void OnBegin(Level level)
			{
				Add(new Coroutine(Cutscene(level)));
			}

			private IEnumerator Cutscene(Level level)
			{
				yield return new LuaProxyCoroutine(StartFunction);
				EndCutscene(level);
			}

			public override void OnEnd(Level level)
			{
				EndFunction?.Call(level, WasSkipped);
			}
		}

		private readonly CutsceneWrapper Cutscene = new();

		public override PrepareMode Mode => PrepareMode.Cutscene;

		public BossEvent(BossController controller)
			: base(controller)
		{
			Values.Add("cutsceneEntity", Cutscene);
		}

		public override void Initialize(LuaFunction[] funcs)
		{
			Cutscene.StartFunction = funcs[0];
			Cutscene.EndFunction = funcs[1];
		}

		public IEnumerator Perform()
		{
			Scene.Add(Cutscene);
			return While(() => Cutscene.Running, true);
		}

		public static BossEvent Create(BossController controller) => new(controller);
	}

	internal class BossFunctions(BossController controller)
		: BossLuaLoader(controller)
	{
		private EnumDict<BossPuppet.HurtModes, LuaFunction> onDamageMethods;

		public override PrepareMode Mode => PrepareMode.Interrupt;

		public LuaProxyCoroutine this[BossPuppet.HurtModes m] => new(onDamageMethods[m]);

		public override void Initialize(LuaFunction[] funcs)
		{
			funcs[0]?.Call();
			onDamageMethods = new(option => funcs.ElementAtOrDefault((int)option + 2) ?? funcs[1]);
		}
	}
}
