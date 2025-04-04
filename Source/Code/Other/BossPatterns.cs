using System;
using System.Collections;
using System.Collections.Generic;
using Monocle;
using static Celeste.Mod.BossesHelper.Code.Other.BossActions;

namespace Celeste.Mod.BossesHelper.Code.Other
{
    public static class BossPatterns
    {
        public struct Method(string name, float? duration)
        {
            public string ActionName = name;

            public float? Duration = duration;
        }

        public struct ControllerDelegates(Dictionary<string, IBossAction> actions, Action<int?> changeToPattern,
            Func<int> randomNext, Action<bool> setIsAttacking, Func<int?> attackIndexForced)
        {
            public Dictionary<string, IBossAction> Actions = actions;
            public Action<int?> ChangeToPattern = changeToPattern;
            public Func<int> RandomNext = randomNext;
            public Action<bool> SetIsAttacking = setIsAttacking;
            public Func<int?> AttackIndexForced = attackIndexForced;
        }

        public abstract class BossPattern(int? goTo, ControllerDelegates delegates)
        {
            public readonly int? GoToPattern = goTo;

            protected readonly ControllerDelegates delegates = delegates;

            protected IEnumerator PerformMethod(Method method)
            {
                if (!method.ActionName.ToLower().Equals("wait"))
                {
                    if (delegates.Actions.TryGetValue(method.ActionName, out IBossAction attack))
                    {
                        delegates.SetIsAttacking(true);
                        yield return attack.Perform();
                        delegates.SetIsAttacking(false);
                    }
                    else
                    {
                        Logger.Log(LogLevel.Error, "Bosses Helper", "Could not find specified action.");
                    }
                }
                yield return method.Duration;
            }

            protected IEnumerator ChangePattern()
            {
                delegates.ChangeToPattern(GoToPattern);
                yield return null;
            }

            public abstract IEnumerator Perform();
        }

        public class EventCutscene(string eventMethod, int? goTo, ControllerDelegates delegates)
            : BossPattern(goTo, delegates)
        {
            private readonly string Event = eventMethod;

            public override IEnumerator Perform()
            {
                yield return PerformMethod(new(Event, null));
                yield return ChangePattern();
            }
        }

        public abstract class AttackPattern(Method[] patternLoop, Hitbox trigger,
            int? minCount, int? count, int? goTo, ControllerDelegates delegates)
            : BossPattern(goTo, delegates)
        {
            public readonly Hitbox PlayerPositionTrigger = trigger;

            protected readonly Method[] StatePatternOrder = patternLoop;

            private readonly int? MinRandomIter = minCount;

            private readonly int? IterationCount = count;

            protected int currentAction;

            protected IEnumerator PerformRepeat(Func<int> getAttackIndex, Func<int> updateLoop)
            {
                currentAction = 0;
                while (true)
                {
                    yield return PerformMethod(StatePatternOrder[getAttackIndex()]);
                    int counter = updateLoop();
                    if (counter > MinRandomIter && (counter > IterationCount || delegates.RandomNext() % 2 == 1))
                    {
                        yield return ChangePattern();
                    }
                }
            }
        }

        public class RandomPattern(Method[] patternLoop, Hitbox trigger, int? minCount, int? count,
            int? goTo, ControllerDelegates delegates)
            : AttackPattern(patternLoop, trigger, minCount, count, goTo, delegates)
        {
            public override IEnumerator Perform()
            {
                yield return PerformRepeat(
                    () => (delegates.AttackIndexForced() ?? delegates.RandomNext()) % StatePatternOrder.Length,
                    () => currentAction++);
            }
        }

        public class SequentialPattern(Method[] patternLoop, Method[] preLoop,
            Hitbox trigger, int? minCount, int? count, int? goTo, ControllerDelegates delegates)
            : AttackPattern(patternLoop, trigger, minCount, count, goTo, delegates)
        {
            private readonly Method[] PrePatternMethods = preLoop;

            public override IEnumerator Perform()
            {
                int loop = 0;
                foreach (Method method in PrePatternMethods)
                {
                    yield return PerformMethod(method);
                }
                yield return PerformRepeat(
                    () => currentAction,
                    () => loop += ((currentAction = (currentAction + 1) % StatePatternOrder.Length) == 0) ? 1 : 0);
            }
        }
    }
}
