using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Other
{
    public class BossPattern
    {
        public struct Method(string name, float? duration)
        {
            public string ActionName = name;

            public float? Duration = duration;
        }

        public readonly bool IsEvent;

        public readonly int? IterationCount;

        public readonly int? GoToPattern;

        public readonly Hitbox PlayerPositionTrigger;

        public readonly bool RandomPattern;

        public readonly Method[] PrePatternMethods;

        public readonly Method[] StatePatternOrder;

        public string FirstAction
        {
            get
            {
                return StatePatternOrder[0].ActionName;
            }
        }

        public BossPattern(Method[] patternLoop, Method[] prePattern = null, Hitbox trigger = null,
            int? count = null, int? goTo = null, bool random = false, bool isEvent = false)
        {
            IterationCount = count;
            GoToPattern = goTo;
            PlayerPositionTrigger = trigger;
            RandomPattern = random;
            IsEvent = isEvent;
            PrePatternMethods = prePattern;
            StatePatternOrder = patternLoop;
        }

        public BossPattern(string eventName, int? goTo)
            : this([new Method(eventName, null)], count: 0, goTo: goTo, isEvent: true)
        {
        }
    }
}
