﻿using Celeste.Mod.BossesHelper.Code.Entities;
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
		public bool IsActing => currentAction != null;

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

	public abstract record AttackPattern(string Name, List<Method> StatePatternOrder,
		Hitbox PlayerPositionTrigger, NullRange RangeCounter, string GoToPattern, BossController Controller)
		: BossPattern(Name, GoToPattern, Controller)
	{
		protected abstract int AttackIndex { get; }

		protected virtual bool Update => true;

		public override IEnumerator Perform()
		{
			RangeCounter.Reset();
			do
			{
				yield return PerformMethod(StatePatternOrder[AttackIndex % StatePatternOrder.Count]);
				if (Update)
					RangeCounter.Inc();
			}
			while (RangeCounter.CanContinue);
			Controller.ChangeToPattern();
			yield return null;
		}
	}

	public record RandomPattern(string Name, List<Method> StatePatternOrder,
		Hitbox PlayerPositionTrigger, NullRange IterationRange, string GoToPattern, BossController Controller)
		: AttackPattern(Name, StatePatternOrder, PlayerPositionTrigger, IterationRange, GoToPattern, Controller)
	{

		public readonly SingleUse<int> ForcedAttackIndex = new();

		protected override int AttackIndex => ForcedAttackIndex.Value ?? Controller.Random.Next();
	}

	public record SequentialPattern(string Name, List<Method> StatePatternOrder, List<Method> PrePatternMethods,
		Hitbox PlayerPositionTrigger, NullRange IterationRange, string GoToPattern, BossController Controller)
		: AttackPattern(Name, StatePatternOrder, PlayerPositionTrigger, IterationRange, GoToPattern, Controller)
	{
		private int actions;

		protected override bool Update => actions++ % StatePatternOrder.Count == 0;

		protected override int AttackIndex => RangeCounter.Counter;

		public override IEnumerator Perform()
		{
			actions = 0;
			foreach (Method method in PrePatternMethods)
			{
				yield return PerformMethod(method);
			}
			yield return base.Perform();
		}
	}
}
