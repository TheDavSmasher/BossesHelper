using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Other
{
    public struct Method(string name, float? duration)
    {
        public string ActionName = name;

        public float? Duration = duration;
    }

    public class BossPattern(Method[] patternLoop, Method[] prePattern = null, Hitbox trigger = null,
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

        public string FirstAction
        {
            get
            {
                return StatePatternOrder[0].ActionName;
            }
        }

        public BossPattern(string eventName, int? goTo)
            : this([new Method(eventName, null)], goTo: goTo, isEvent: true) { }
    }
}
