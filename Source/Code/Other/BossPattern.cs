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

        public enum FinishModes
        {
            ContinueLoop,
            LoopCountGoTo,
            PlayerPositionWithin
        }

        public readonly bool IsEvent;

        public readonly FinishModes FinishMode;

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

        private BossPattern(Method[] patternLoop, Method[] prePattern = null, FinishModes finishMode = FinishModes.ContinueLoop,
            Hitbox trigger = null, int? count = null, int? goTo = null, bool random = false, bool isEvent = false)
        {
            FinishMode = finishMode;
            IterationCount = count;
            GoToPattern = goTo;
            PlayerPositionTrigger = trigger;
            RandomPattern = random;
            IsEvent = isEvent;
            PrePatternMethods = prePattern;
            StatePatternOrder = patternLoop;
        }

        public BossPattern(string eventName, int? goTo)
            : this([new Method(eventName, null)], finishMode: FinishModes.LoopCountGoTo, count: 0, goTo: goTo, isEvent: true)
        {
        }

        public BossPattern(Method[] statePatternOrder, Method[] prePatternMethods)
            : this(statePatternOrder, prePattern: prePatternMethods)
        {
        }

        public BossPattern(Method[] statePatternOrder, Method[] prePatternMethods,
            float x, float y, float width, float height, int? goTo, Vector2 offset)
            : this(statePatternOrder, prePatternMethods, FinishModes.PlayerPositionWithin,
                  new Hitbox(x + offset.X, y + offset.Y, width, height), goTo: goTo)
        {
        }

        public BossPattern(Method[] statePatternOrder, Method[] prePatternMethods, int count, int? goTo)
            : this(statePatternOrder, prePatternMethods, FinishModes.LoopCountGoTo, count: count, goTo: goTo)
        {
        }

        public BossPattern(Method[] randomPatternLoop)
            : this(randomPatternLoop, random: true)
        {
        }
    }
}
