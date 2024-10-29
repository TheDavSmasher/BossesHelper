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

        public readonly bool IsEvent;

        public enum FinishModes
        {
            ContinueLoop,
            LoopCountGoTo,
            PlayerPositionWithin
        }

        public FinishModes FinishMode { get; private set; }

        public int? IterationCount { get; private set; }

        public int? GoToPattern {  get; private set; }

        public Rectangle PlayerPositionTrigger { get; private set; }

        public readonly bool RandomPattern;

        public Method[] PrePatternMethods {  get; private set; }

        public Method[] StatePatternOrder { get; private set; }

        public string FirstAction
        {
            get
            {
                return StatePatternOrder[0].ActionName;
            }
        }

        public BossPattern(string eventName, int goTo)
        {
            FinishMode = FinishModes.LoopCountGoTo;
            StatePatternOrder = [new Method(eventName, null)];
            IsEvent = true;

            GoToPattern = goTo;
            IterationCount = 0;
            PlayerPositionTrigger = Rectangle.Empty;
            RandomPattern = false;
        }

        public BossPattern(Method[] statePatternOrder, Method[] prePatternMethods)
        {
            FinishMode = FinishModes.ContinueLoop;
            PrePatternMethods = prePatternMethods;
            StatePatternOrder = statePatternOrder;
            IsEvent = false;

            PlayerPositionTrigger = Rectangle.Empty;
            RandomPattern = false;
        }

        public BossPattern(Method[] statePatternOrder, Method[] prePatternMethods, int x, int y, int width, int height, int goTo, Vector2 offset)
        {
            FinishMode = FinishModes.PlayerPositionWithin;
            PrePatternMethods = prePatternMethods;
            StatePatternOrder = statePatternOrder;
            IsEvent = false;

            PlayerPositionTrigger = new Rectangle(x + (int)offset.X, y + (int)offset.Y, width, height);
            GoToPattern = goTo;
            RandomPattern = false;
        }

        public BossPattern(Method[] statePatternOrder, Method[] prePatternMethods, int count, int goTo)
        {
            FinishMode = FinishModes.LoopCountGoTo;
            PrePatternMethods = prePatternMethods;
            StatePatternOrder = statePatternOrder;
            IsEvent = false;

            PlayerPositionTrigger = Rectangle.Empty;
            IterationCount = count;
            GoToPattern = goTo;
            RandomPattern = false;
        }

        public BossPattern(Method[] randomPattern)
        {
            FinishMode = FinishModes.ContinueLoop;
            RandomPattern = true;
            StatePatternOrder = randomPattern;
            IsEvent = false;

            PlayerPositionTrigger = Rectangle.Empty;
        }
    }
}
