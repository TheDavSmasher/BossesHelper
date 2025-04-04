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

            protected readonly Dictionary<string, IBossAction> Actions = delegates.Actions;

            private readonly Action<int?> ChangeToPattern = delegates.ChangeToPattern;

            private readonly Action<bool> SetIsAttacking = delegates.SetIsAttacking;

            protected IEnumerator PerformMethod(Method method)
            {
                if (!method.ActionName.ToLower().Equals("wait"))
                {
                    if (Actions.TryGetValue(method.ActionName, out IBossAction attack))
                    {
                        SetIsAttacking(true);
                        yield return attack.Perform();
                        SetIsAttacking(false);
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
                ChangeToPattern(GoToPattern);
                yield return null;
            }

            public abstract IEnumerator Perform();
        }

        public abstract class AttackPattern(Method[] patternLoop, Hitbox trigger,
            int? minCount, int? count, int? goTo, ControllerDelegates delegates)
            : BossPattern(goTo, delegates)
        {
            public readonly Hitbox PlayerPositionTrigger = trigger;

            protected readonly Method[] StatePatternOrder = patternLoop;

            protected readonly Func<int> RandomNext = delegates.RandomNext;

            private readonly int? MinRandomIter = minCount;

            private readonly int? IterationCount = count;

            protected int currentAction = 0;

            protected IEnumerator ChangeWhenCounter(int counter)
            {
                if (counter > MinRandomIter && (counter > IterationCount || RandomNext() % 2 == 1))
                {
                    yield return ChangePattern();
                }
            }
        }

        public class RandomPattern(Method[] patternLoop, Hitbox trigger, int? minCount, int? count,
            int? goTo, ControllerDelegates delegates)
            : AttackPattern(patternLoop, trigger, minCount, count, goTo, delegates)
        {
            private readonly Func<int?> AttackIndexForced = delegates.AttackIndexForced;

            public override IEnumerator Perform()
            {
                while (true)
                {
                    int nextAttack = (AttackIndexForced() ?? RandomNext()) % StatePatternOrder.Length;
                    yield return PerformMethod(StatePatternOrder[nextAttack]);
                    currentAction++;

                    yield return ChangeWhenCounter(currentAction);
                }
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
                while (true)
                {
                    if (currentAction >= StatePatternOrder.Length)
                    {
                        loop++;
                        currentAction = 0;
                    }
                    yield return ChangeWhenCounter(loop);

                    yield return PerformMethod(StatePatternOrder[currentAction]);

                    currentAction++;
                }
            }
        }

        public class EventCutscene(string eventMethod, int? goTo, ControllerDelegates delegates)
            : BossPattern(goTo, delegates)
        {
            private readonly Method Event = new(eventMethod, null);

            public override IEnumerator Perform()
            {
                yield return PerformMethod(Event);
                yield return ChangePattern();
            }
        }
    }
}
