using System;
using System.Collections;
using System.Collections.Generic;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    public record ControllerDelegates(Action ChangeToPattern,
        Func<int> RandomNext, Action<bool> SetIsAttacking, Func<int?> AttackIndexForced);

    public readonly record struct Method(string ActionName, float? Duration);

    public enum MethodEndReason
    {
        Completed,
        Interrupted,
        PlayerDied
    }

    public abstract class BossPattern(int? goTo, Dictionary<string, IBossAction> actions, ControllerDelegates delegates)
    {
        public readonly int? GoToPattern = goTo;

        private readonly Dictionary<string, IBossAction> Actions = actions;

        protected readonly ControllerDelegates delegates = delegates;

        public IBossAction CurrentAction;

        protected IEnumerator PerformMethod(Method method)
        {
            if (!method.ActionName.ToLower().Equals("wait"))
            {
                if (Actions.TryGetValue(method.ActionName, out CurrentAction))
                {
                    delegates.SetIsAttacking(true);
                    yield return CurrentAction.Perform();
                    delegates.SetIsAttacking(false);
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
                delegates.ChangeToPattern();
                yield return null;
            }
        }
    }

    public class EventCutscene(Method eventMethod, int? goTo,
        Dictionary<string, IBossAction> actions, ControllerDelegates delegates)
        : BossPattern(goTo, actions, delegates)
    {
        private readonly Method Event = eventMethod;

        public override IEnumerator Perform() => PerformAndChange(() => Event, () => true);
            
    }

    public abstract class AttackPattern(List<Method> patternLoop, Hitbox trigger, int? minCount, int? count, int? goTo, 
        Dictionary<string, IBossAction> actions, ControllerDelegates delegates)
        : BossPattern(goTo, actions, delegates)
    {
        public readonly Hitbox PlayerPositionTrigger = trigger;

        protected readonly List<Method> StatePatternOrder = patternLoop;

        private readonly int? MinRandomIter = minCount;

        private readonly int? IterationCount = count;

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
                    return counter > MinRandomIter && (counter > IterationCount || delegates.RandomNext() % 2 == 1);
                });
            }
        }

        protected virtual int UpdateLoop() => currentAction++;
    }

    public class RandomPattern(List<Method> patternLoop, Hitbox trigger, int? minCount, int? count,
        int? goTo, Dictionary<string, IBossAction> actions, ControllerDelegates delegates)
        : AttackPattern(patternLoop, trigger, minCount, count, goTo, actions, delegates)
    {
        protected override int AttackIndex => delegates.AttackIndexForced() ?? delegates.RandomNext();
    }

    public class SequentialPattern(List<Method> patternLoop, List<Method> preLoop, Hitbox trigger,
        int? minCount, int? count, int? goTo, Dictionary<string, IBossAction> actions, ControllerDelegates delegates)
        : AttackPattern(patternLoop, trigger, minCount, count, goTo, actions, delegates)
    {
        private readonly List<Method> PrePatternMethods = preLoop;

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
