using System;
using System.Collections;
using System.Collections.Generic;
using Monocle;
using static Celeste.Mod.BossesHelper.Code.Other.BossActions;

namespace Celeste.Mod.BossesHelper.Code.Other
{
    public static class Patterns
    {
        public struct Method(string name, float? duration)
        {
            public string ActionName = name;

            public float? Duration = duration;
        }

        public class BossPatterns(Method[] patternLoop, Method[] prePattern = null, Hitbox trigger = null,
            int? minCount = null, int? count = null, int? goTo = null, bool random = false, bool isEvent = false)
        {
            public readonly bool IsEvent = isEvent;

            public readonly int? MinRandomIter = minCount;

            public readonly int? IterationCount = count;

            public readonly int? GoToPattern = goTo;

            public readonly Hitbox PlayerPositionTrigger = trigger;

            public readonly bool RandomPattern = random;

            public readonly Method[] PrePatternMethods = prePattern ?? [];

            public readonly Method[] StatePatternOrder = patternLoop;

            public BossPatterns(string eventName, int? goTo)
                : this([new Method(eventName, null)], goTo: goTo, isEvent: true) { }
        }

        public abstract class BossPattern(Dictionary<string, IBossAction> references, Action<bool> setIsAttacking,
            Func<int?, IEnumerator> changePattern, int? goTo = null)
        {
            protected readonly Dictionary<string, IBossAction> Actions = references;

            protected readonly int? GoToPattern = goTo;

            protected readonly Func<int?, IEnumerator> ChangePattern = changePattern;

            private readonly Action<bool> SetIsAttacking = setIsAttacking;

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

            public abstract IEnumerator Perform();
        }

        public abstract class AttackPattern(
            Dictionary<string, IBossAction> references, Method[] patternLoop,
            Func<int?, IEnumerator> changePattern, Func<int> randomNext, Action<bool> setIsAttacking,
            Hitbox trigger = null, int? minCount = null, int? count = null, int? goTo = null)
            : BossPattern(references, setIsAttacking, changePattern, goTo)
        {
            public readonly Hitbox PlayerPositionTrigger = trigger;

            protected readonly Method[] StatePatternOrder = patternLoop;

            protected readonly Func<int> RandomNext = randomNext;

            private readonly int? MinRandomIter = minCount;

            private readonly int? IterationCount = count;

            protected int currentAction = 0;

            protected IEnumerator ChangeWhenCounter(int counter)
            {
                if (counter > MinRandomIter && (counter > IterationCount || RandomNext() % 2 == 1))
                {
                    yield return ChangePattern(GoToPattern);
                }
            }
        }

        public class RandomPattern(Dictionary<string, IBossAction> references, Method[] patternLoop,
            Func<int?, IEnumerator> changePattern, Func<int> randomNext, Action<bool> setIsAttacking, Func<int?> indexForced,
            Hitbox trigger = null, int? minCount = null, int? count = null, int? goTo = null)
            : AttackPattern(references, patternLoop, changePattern, randomNext, setIsAttacking, trigger, minCount, count, goTo)
        {
            private readonly Func<int?> AttackIndexForced = indexForced;

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

        public class SequentialPattern(Dictionary<string, IBossAction> references, Method[] patternLoop, Method[] preLoop,
            Func<int?, IEnumerator> changePattern, Func<int> randomNext, Action<bool> setIsAttacking,
            Hitbox trigger = null, int? minCount = null, int? count = null, int? goTo = null)
            : AttackPattern(references, patternLoop, changePattern, randomNext, setIsAttacking, trigger, minCount, count, goTo)
        {
            private readonly Method[] PrePatternMethods = preLoop;

            private int loop = 0;

            public override IEnumerator Perform()
            {
                if (PrePatternMethods != null)
                {
                    foreach (Method method in PrePatternMethods)
                    {
                        yield return PerformMethod(method);
                    }
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

        public class EventCutscene(Method eventMethod, Dictionary<string, IBossAction> references, Action<bool> setIsAttacking,
            Func<int?, IEnumerator> changePattern, int? goTo = null)
            : BossPattern(references, setIsAttacking, changePattern, goTo)
        {
            private readonly Method Event = eventMethod;

            public override IEnumerator Perform()
            {
                yield return PerformMethod(Event);
                yield return ChangePattern(GoToPattern);
            }
        }
    }
}
