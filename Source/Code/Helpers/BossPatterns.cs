using Celeste.Mod.BossesHelper.Code.Entities;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
	public readonly record struct Method(string ActionName, float? Duration)
	{
		public readonly bool IsWait = ActionName.ToLower().Equals("wait");
	}

	public enum PatternType
	{
		Event,
		Pattern,
		Random
	}

	public abstract record BossPattern(string Name, string GoToPattern, BossController Controller)
	{
		public bool IsActing => currentAction == null;

		private IBossAction currentAction;

		public abstract IEnumerator Perform();

		public void EndAction(BossAttack.EndReason reason)
		{
			if (currentAction is BossAttack attack)
				attack.End(reason);
			currentAction = null;
		}

		protected IEnumerator PerformMethod(Method method)
		{
			if (!method.IsWait)
			{
				if (Controller.TryGet(method.ActionName, out currentAction))
				{
					yield return currentAction.Perform();
					EndAction(BossAttack.EndReason.Completed);
				}
				else
				{
					Logger.Log(LogLevel.Error, "Bosses Helper", "Could not find specified action.");
				}
			}
			yield return method.Duration;
		}
	}

	public record EventCutscene(string Name, Method Event, string GoToPattern, BossController Controller)
		: BossPattern(Name, GoToPattern, Controller)
	{
		public override IEnumerator Perform()
		{
			yield return PerformMethod(Event);
			Controller.ChangeToPattern();
			yield return null;
		}
	}

	public abstract record AttackPattern(string Name, List<Method> StatePatternOrder, Hitbox PlayerPositionTrigger,
		int? MinRandomIter, int? IterationCount, string GoToPattern, BossController Controller)
		: BossPattern(Name, GoToPattern, Controller)
	{
		private int currentAction;

		protected virtual int AttackIndex => currentAction;

		protected virtual int UpdateLoop()
			=> currentAction++;

		public override IEnumerator Perform()
		{
			currentAction = 0;
			while (true)
			{
				yield return PerformMethod(StatePatternOrder[AttackIndex % StatePatternOrder.Count]);
				int counter = UpdateLoop();
				if (counter > MinRandomIter && (counter > IterationCount || Controller.Random.Next() % 2 == 1))
				{
					Controller.ChangeToPattern();
					yield return null;
				}
			}
		}
	}

	public record RandomPattern(string Name, List<Method> StatePatternOrder, Hitbox PlayerPositionTrigger,
		int? MinRandomIter, int? IterationCount, string GoToPattern, BossController Controller)
		: AttackPattern(Name, StatePatternOrder, PlayerPositionTrigger, MinRandomIter, IterationCount, GoToPattern, Controller)
	{
		protected override int AttackIndex => ForcedAttackIndex.Value ?? Controller.Random.Next();

		public readonly SingleUse<int> ForcedAttackIndex = new();
	}

	public record SequentialPattern(string Name, List<Method> StatePatternOrder, List<Method> PrePatternMethods, Hitbox PlayerPositionTrigger,
		int? MinRandomIter, int? IterationCount, string GoToPattern, BossController Controller)
		: AttackPattern(Name, StatePatternOrder, PlayerPositionTrigger, MinRandomIter, IterationCount, GoToPattern, Controller)
	{
		private int loop;

		protected override int UpdateLoop()
		{
			if (base.UpdateLoop() % StatePatternOrder.Count == 0)
				loop++;
			return loop;
		}

		public override IEnumerator Perform()
		{
			loop = 0;
			foreach (Method method in PrePatternMethods)
			{
				yield return PerformMethod(method);
			}
			yield return base.Perform();
		}
	}
}
