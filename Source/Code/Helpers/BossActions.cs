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

	public abstract class BossLuaLoader(string filepath, BossController controller) : ILuaLoader
	{
		public abstract PrepareMode Mode { get; }

		public abstract void Initialize(LuaFunction[] funcs);

		public Scene Scene => controller.Scene;

		public string Filepath => filepath;

		public Dictionary<string, object> Values { get; init; } = new()
		{
			{ "boss", controller },
			{ "bossID", controller.BossID },
			{ "puppet", controller.Puppet },
			{ "sidekick", controller.Scene.GetEntity<BadelineSidekick>() }
		};
	}

	public class BossAttack : BossLuaLoader, IBossAction
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

		public BossAttack(string filepath, BossController controller)
			: base(filepath, controller)
		{
			this.LoadFile();
		}

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

		public static BossAttack Create(string filepath, BossController controller) => new(filepath, controller);
	}

	public class BossEvent : BossLuaLoader, IBossAction
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

		public BossEvent(string filepath, BossController controller)
			: base(filepath, controller)
		{
			Values.Add("cutsceneEntity", Cutscene);
			this.LoadFile();
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

		public static BossEvent Create(string filepath, BossController controller) => new(filepath, controller);
	}

	internal class BossFunctions : BossLuaLoader
	{
		private EnumDict<BossPuppet.HurtModes, LuaFunction> onDamageMethods;

		public override PrepareMode Mode => PrepareMode.Interrupt;

		public LuaProxyCoroutine this[BossPuppet.HurtModes m] => new(onDamageMethods[m]);

		public BossFunctions(string filepath, BossController controller)
			: base(filepath,controller)
		{
			this.LoadFile();
		}

		public override void Initialize(LuaFunction[] funcs)
		{
			funcs[0]?.Call();
			onDamageMethods = new(option => funcs.ElementAtOrDefault((int)option + 2) ?? funcs[1]);
		}
	}
}
