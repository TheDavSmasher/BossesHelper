using Microsoft.Xna.Framework;

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

        public FinishModes FinishMode { get; private set; }

        public int? IterationCount { get; private set; }

        public int? GoToPattern { get; private set; }

        public Rectangle PlayerPositionTrigger { get; private set; }

        public readonly bool RandomPattern;

        public Method[] PrePatternMethods { get; private set; }

        public Method[] StatePatternOrder { get; private set; }

        public string FirstAction
        {
            get
            {
                return StatePatternOrder[0].ActionName;
            }
        }

        private BossPattern(Method[] patternLoop, Rectangle trigger, Method[] prePattern = null, FinishModes finishMode = FinishModes.ContinueLoop, int? count = null, int? goTo = null, bool random = false, bool isEvent = false)
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
            : this(patternLoop: [new Method(eventName, null)], trigger: Rectangle.Empty, finishMode: FinishModes.LoopCountGoTo, count: 0, goTo: goTo, isEvent: true)
        {
        }

        public BossPattern(Method[] statePatternOrder, Method[] prePatternMethods)
            : this(prePattern: prePatternMethods, patternLoop: statePatternOrder, trigger: Rectangle.Empty)
        {
        }

        public BossPattern(Method[] statePatternOrder, Method[] prePatternMethods, int x, int y, int width, int height, int? goTo, Vector2 offset)
            : this(prePattern: prePatternMethods, patternLoop: statePatternOrder, finishMode: FinishModes.PlayerPositionWithin, trigger: new Rectangle(x + (int)offset.X, y + (int)offset.Y, width, height), goTo: goTo)
        {
        }

        public BossPattern(Method[] statePatternOrder, Method[] prePatternMethods, int count, int? goTo)
            : this(prePattern: prePatternMethods, patternLoop: statePatternOrder, trigger: Rectangle.Empty, finishMode: FinishModes.LoopCountGoTo, count: count, goTo: goTo)
        {
        }

        public BossPattern(Method[] randomPattern)
            : this(patternLoop: randomPattern, trigger: Rectangle.Empty, random: true)
        {
        }
    }
}
