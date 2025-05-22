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

            public IBossAction CurrentAction;

            protected IEnumerator PerformMethod(Method method)
            {
                if (!method.ActionName.ToLower().Equals("wait"))
                {
                    if (delegates.Actions.TryGetValue(method.ActionName, out CurrentAction))
                    {
                        delegates.SetIsAttacking(true);
                        yield return CurrentAction.Perform();
                        delegates.SetIsAttacking(false);
                        CurrentAction = null;
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

            protected virtual int AttackIndex => currentAction;

            protected int currentAction;

            public override IEnumerator Perform()
            {
                currentAction = 0;
                while (true)
                {
                    yield return PerformMethod(StatePatternOrder[AttackIndex % StatePatternOrder.Length]);
                    int counter = UpdateLoop();
                    if (counter > MinRandomIter && (counter > IterationCount || delegates.RandomNext() % 2 == 1))
                    {
                        yield return ChangePattern();
                    }
                }
            }

            protected virtual int UpdateLoop()
            {
                return currentAction++;
            }
        }

        public class RandomPattern(Method[] patternLoop, Hitbox trigger, int? minCount, int? count,
            int? goTo, ControllerDelegates delegates)
            : AttackPattern(patternLoop, trigger, minCount, count, goTo, delegates)
        {
            protected override int AttackIndex => delegates.AttackIndexForced() ?? delegates.RandomNext();
        }

        public class SequentialPattern(Method[] patternLoop, Method[] preLoop,
            Hitbox trigger, int? minCount, int? count, int? goTo, ControllerDelegates delegates)
            : AttackPattern(patternLoop, trigger, minCount, count, goTo, delegates)
        {
            private readonly Method[] PrePatternMethods = preLoop;

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

            protected override int UpdateLoop()
            {
                return loop += (base.UpdateLoop() % StatePatternOrder.Length == 0) ? 1 : 0;
            }
        }
    }
}
