﻿using System.Drawing;

namespace Celeste.Mod.BossesHelper.Code.Other
{
    public class BossPattern
    {
        public struct Method(string name, float? duration, bool isEvent)
        {
            public string ActionName = name;

            public float? Duration = duration;

            public bool IsEvent = isEvent;
        }

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

        public BossPattern(Method[] statePatternOrder, Method[] prePatternMethods)
        {
            FinishMode = FinishModes.ContinueLoop;
            PrePatternMethods = prePatternMethods;
            StatePatternOrder = statePatternOrder;

            PlayerPositionTrigger = Rectangle.Empty;
            RandomPattern = false;
        }

        public BossPattern(Method[] statePatternOrder, Method[] prePatternMethods, int x, int y, int width, int height, int goTo)
        {
            FinishMode = FinishModes.PlayerPositionWithin;
            PrePatternMethods = prePatternMethods;
            StatePatternOrder = statePatternOrder;

            PlayerPositionTrigger = new Rectangle(x, y, width, height);
            GoToPattern = goTo;
            RandomPattern = false;
        }

        public BossPattern(Method[] statePatternOrder, Method[] prePatternMethods, int count, int goTo)
        {
            FinishMode = FinishModes.LoopCountGoTo;
            PrePatternMethods = prePatternMethods;
            StatePatternOrder = statePatternOrder;

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

            PlayerPositionTrigger = Rectangle.Empty;
        }
    }
}
