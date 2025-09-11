global using LuaCommand = (string Name, int Count);
global using LuaTableItem = (object Key, object Value);
using Celeste.Mod.BossesHelper.Code.Entities;
using NLua;
using System;
using System.Collections;
using System.Linq;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
	public interface IBossAction
	{
		IEnumerator Perform();

		void End(MethodEndReason reason) { }
	}

	public interface IBossActionCreator<TSelf> : IBossAction where TSelf : IBossActionCreator<TSelf>
	{
		abstract static TSelf Create(string filepath, BossController controller);
	}

	public abstract class BossLuaLoader(BossController controller) : ILuaLoader
	{
		public abstract LuaCommand Command { get; }

		public virtual LuaTableItem[] Values =>
		[
			( "boss", controller ),
			( "bossID", controller.BossID ),
			( "puppet", controller.Puppet ),
			( "player", controller.Scene.GetPlayer() ),
			( "sidekick", controller.Scene.GetEntity<BadelineSidekick>() )
		];
	}

	public class BossAttack : BossLuaLoader, IBossActionCreator<BossAttack>
	{
		private readonly LuaFunction attackFunction;

		private readonly LuaFunction endFunction;

		private readonly EnumDict<MethodEndReason, LuaFunction> onEndMethods;

		public override LuaCommand Command => ("getAttackData", 5);

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
			return attackFunction.ToIEnumerator();
		}

		public void End(MethodEndReason reason)
		{
			endFunction?.Call(reason);
			onEndMethods[reason]?.Call();
		}

		public static BossAttack Create(string filepath, BossController controller) => new(filepath, controller);
	}

	public class BossEvent : BossLuaLoader, IBossActionCreator<BossEvent>
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

		public override LuaTableItem[] Values => [("cutsceneEntity", cutscene), .. base.Values];

		public BossEvent(string filepath, BossController controller)
			: base(controller)
		{
			this.controller = controller;
			cutscene = new(this.LoadFile(filepath));
		}

		public IEnumerator Perform()
		{
			controller.Scene.Add(cutscene);
			return While(() => cutscene.Running, true);
		}

		public static BossEvent Create(string filepath, BossController controller) => new(filepath, controller);
	}

	internal class BossFunctions : BossLuaLoader
	{
		private readonly EnumDict<BossPuppet.HurtModes, LuaFunction> onDamageMethods;

		public override LuaCommand Command => ("getInterruptData", 6);

		public BossFunctions(string filepath, BossController controller)
			: base(controller)
		{
			LuaFunction[] array = this.LoadFile(filepath);
			array[0]?.Call();
			LuaFunction OnHitLua = array[1];
			onDamageMethods = new(option => array.ElementAtOrDefault((int)option + 2) ?? OnHitLua);
		}

		public IEnumerator OnDamage(BossPuppet.HurtModes source)
		{
			return onDamageMethods[source]?.ToIEnumerator();
		}
	}
}
