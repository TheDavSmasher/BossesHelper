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

	public abstract record BossPattern(string Name, string GoToPattern, BossController Controller)
	{
		public IBossAction CurrentAction { get; private set; }

		public bool IsActing { get; private set; }

		protected IEnumerator PerformMethod(Method method)
		{
			if (!method.IsWait)
			{
				if (Controller.TryGet(method.ActionName, out IBossAction _currentAct))
				{
					CurrentAction = _currentAct;
					IsActing = true;
					yield return CurrentAction.Perform();
					EndAction(BossAttack.EndReason.Completed);
				}
				else
				{
					Logger.Log(LogLevel.Error, "Bosses Helper", "Could not find specified action.");
				}
			}
			yield return method.Duration;
		}

		public void OnPlayerDie(Player _)
			=> EndAction(BossAttack.EndReason.PlayerDied);

		public void Interrupt()
			=> EndAction(BossAttack.EndReason.Interrupted);

		private void EndAction(BossAttack.EndReason reason)
		{
			IsActing = false;
			if (CurrentAction is BossAttack attack)
				attack.End(reason);
			CurrentAction = null;
		}

		public abstract IEnumerator Perform();

		protected IEnumerator PerformAndChange(Func<Method> getMethod, Func<bool> changePattern)
		{
			yield return PerformMethod(getMethod());
			if (changePattern())
			{
				Controller.ChangeToPattern();
				yield return null;
			}
		}
	}

	public record EventCutscene(string Name, Method Event, string GoToPattern, BossController Controller)
		: BossPattern(Name, GoToPattern, Controller)
	{
		public override IEnumerator Perform() => PerformAndChange(() => Event, () => true);
	}

	public abstract record AttackPattern(string Name, List<Method> StatePatternOrder, Hitbox PlayerPositionTrigger,
		int? MinRandomIter, int? IterationCount, string GoToPattern, BossController Controller)
		: BossPattern(Name, GoToPattern, Controller)
	{
		protected virtual int AttackIndex => currentAction;

		private int currentAction;

		public override IEnumerator Perform()
		{
			currentAction = 0;
			while (true)
			{
				yield return PerformAndChange(() => StatePatternOrder[AttackIndex % StatePatternOrder.Count], () =>
				{
					int counter = UpdateLoop();
					return counter > MinRandomIter && (counter > IterationCount || Controller.Random.Next() % 2 == 1);
				});
			}
		}

		protected virtual int UpdateLoop() => currentAction++;
	}

	public record RandomPattern(string Name, List<Method> StatePatternOrder, Hitbox PlayerPositionTrigger,
		int? MinRandomIter, int? IterationCount, string GoToPattern, BossController Controller)
		: AttackPattern(Name, StatePatternOrder, PlayerPositionTrigger, MinRandomIter, IterationCount, GoToPattern, Controller)
	{
		private readonly SingleUse<int> ForcedAttackIndex = new();

		public void ForceNextAttack(int value)
		{
			ForcedAttackIndex.Value = value;
		}

		protected override int AttackIndex => ForcedAttackIndex.Value ?? Controller.Random.Next();
	}

	public record SequentialPattern(string Name, List<Method> StatePatternOrder, List<Method> PrePatternMethods, Hitbox PlayerPositionTrigger,
		int? MinRandomIter, int? IterationCount, string GoToPattern, BossController Controller)
		: AttackPattern(Name, StatePatternOrder, PlayerPositionTrigger, MinRandomIter, IterationCount, GoToPattern, Controller)
	{
		private int loop;

		public override IEnumerator Perform()
		{
			loop = 0;
			foreach (Method method in PrePatternMethods)
			{
				yield return PerformMethod(method);
			}
			yield return base.Perform();
		}

		protected override int UpdateLoop() => loop += base.UpdateLoop() % StatePatternOrder.Count == 0 ? 1 : 0;
	}
}
