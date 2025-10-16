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

		void End(ActionEndReason reason) { }
	}

	public interface IBossActionCreator<TSelf> : IBossAction where TSelf : IBossActionCreator<TSelf>
	{
		abstract static TSelf Create(string filepath, BossController controller);
	}

	public abstract class BossLuaLoader(BossController controller) : ILuaLoader
	{
		public abstract PrepareMode Mode { get; }

		public Scene Scene => controller.Scene;

		public Dictionary<string, object> Values { get; init; } = new()
		{
			{ "boss", controller },
			{ "bossID", controller.BossID },
			{ "puppet", controller.Puppet },
			{ "sidekick", controller.Scene.GetEntity<BadelineSidekick>() }
		};
	}

	public class BossAttack : BossLuaLoader, IBossActionCreator<BossAttack>
	{
		private readonly LuaFunction attackFunction;

		private readonly LuaFunction endFunction;

		private readonly EnumDict<ActionEndReason, LuaFunction> onEndMethods;

		public override PrepareMode Mode => PrepareMode.Attack;

		public BossAttack(string filepath, BossController controller)
			: base(controller)
		{
			LuaFunction[] array = this.LoadFile(filepath);
			attackFunction = array[0];
			endFunction = array[1];
			onEndMethods = new(option => array[(int)option + 2]);
		}

		public IEnumerator Perform()
		{
			return new LuaProxyCoroutine(attackFunction);
		}

		public void End(ActionEndReason reason)
		{
			endFunction?.Call(reason);
			onEndMethods[reason]?.Call();
		}

		public static BossAttack Create(string filepath, BossController controller) => new(filepath, controller);
	}

	public class BossEvent : BossLuaLoader, IBossActionCreator<BossEvent>
	{
		private class CutsceneWrapper : CutsceneEntity
		{
			private readonly LuaFunction StartFunction;

			private readonly LuaFunction EndFunction;

			public CutsceneWrapper(BossEvent @event, string filepath)
				: base()
			{
				@event.Values.Add("cutsceneEntity", this);
				LuaFunction[] funcs = @event.LoadFile(filepath);
				StartFunction = funcs[0];
				EndFunction = funcs[1];
			}

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

		private readonly CutsceneWrapper Cutscene;

		public override PrepareMode Mode => PrepareMode.Cutscene;

		public BossEvent(string filepath, BossController controller)
			: base(controller)
		{
			Cutscene = new(this, filepath);
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
		private readonly EnumDict<BossPuppet.HurtModes, LuaFunction> onDamageMethods;

		public override PrepareMode Mode => PrepareMode.Interrupt;

		public LuaProxyCoroutine this[BossPuppet.HurtModes m] => new(onDamageMethods[m]);

		public BossFunctions(string filepath, BossController controller)
			: base(controller)
		{
			LuaFunction[] array = this.LoadFile(filepath);
			array[0]?.Call();
			onDamageMethods = new(option => array.ElementAtOrDefault((int)option + 2) ?? array[1]);
		}
	}
}
