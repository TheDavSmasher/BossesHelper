using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    public record ControllerDelegates(Action ChangeToPattern,
        Func<int> RandomNext, Action<bool> SetIsAttacking, Func<int?> AttackIndexForced);

    public readonly record struct Method(string ActionName, float? Duration)
    {
        public bool IsWait => ActionName.ToLower().Equals("wait");
    }

    public enum MethodEndReason
    {
        Completed,
        Interrupted,
        PlayerDied
    }

    public abstract record BossPattern(string Name, string GoToPattern, Dictionary<string, IBossAction> Actions, ControllerDelegates Delegates)
    {
        public IBossAction CurrentAction { get; private set; }

        protected IEnumerator PerformMethod(Method method)
        {
            if (!method.IsWait)
            {
                if (Actions.TryGetValue(method.ActionName, out IBossAction _currentAct))
                {
                    CurrentAction = _currentAct;
                    Delegates.SetIsAttacking(true);
                    yield return CurrentAction.Perform();
                    Delegates.SetIsAttacking(false);
                    EndAction(MethodEndReason.Completed);
                    CurrentAction = null;
                }
                else
                {
                    Logger.Log(LogLevel.Error, "Bosses Helper", "Could not find specified action.");
                }
            }
            yield return method.Duration;
        }

        public void EndAction(MethodEndReason reason)
        {
            CurrentAction?.EndAction(reason);
        }

        public abstract IEnumerator Perform();

        protected IEnumerator PerformAndChange(Func<Method> getMethod, Func<bool> changePattern)
        {
            yield return PerformMethod(getMethod());
            if (changePattern())
            {
                Delegates.ChangeToPattern();
                yield return null;
            }
        }
    }

    public record EventCutscene(string Name, Method Event, string GoToPattern,
        Dictionary<string, IBossAction> Actions, ControllerDelegates Delegates)
        : BossPattern(Name, GoToPattern, Actions, Delegates)
    {
        public override IEnumerator Perform() => PerformAndChange(() => Event, () => true);
    }

    public abstract record AttackPattern(string Name, List<Method> StatePatternOrder, Hitbox PlayerPositionTrigger,
        int? MinRandomIter, int? IterationCount, string GoToPattern,
        Dictionary<string, IBossAction> Actions, ControllerDelegates Delegates)
        : BossPattern(Name, GoToPattern, Actions, Delegates)
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
                    return counter > MinRandomIter && (counter > IterationCount || Delegates.RandomNext() % 2 == 1);
                });
            }
        }

        protected virtual int UpdateLoop() => currentAction++;
    }

    public record RandomPattern(string Name, List<Method> StatePatternOrder, Hitbox PlayerPositionTrigger,
        int? MinRandomIter, int? IterationCount, string GoToPattern,
        Dictionary<string, IBossAction> Actions, ControllerDelegates Delegates)
        : AttackPattern(Name, StatePatternOrder, PlayerPositionTrigger, MinRandomIter, IterationCount, GoToPattern, Actions, Delegates)
    {
        protected override int AttackIndex => Delegates.AttackIndexForced() ?? Delegates.RandomNext();
    }

    public record SequentialPattern(string Name, List<Method> StatePatternOrder, List<Method> PrePatternMethods, Hitbox PlayerPositionTrigger,
        int? MinRandomIter, int? IterationCount, string GoToPattern,
        Dictionary<string, IBossAction> Actions, ControllerDelegates Delegates)
        : AttackPattern(Name, StatePatternOrder, PlayerPositionTrigger, MinRandomIter, IterationCount, GoToPattern, Actions, Delegates)
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
