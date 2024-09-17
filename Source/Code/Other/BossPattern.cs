using System;
using System.Drawing;
using System.Linq;
using System.Numerics;

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
            OnHealthNum,
            LoopCountGoTo,
            PlayerPositionWithin
        }

        public FinishModes FinishMode { get; private set; }

        public int? IterationCount { get; private set; }

        public int? GoToPattern {  get; private set; }

        public int? HealthThreshold { get; private set; }

        public Rectangle PlayerPositionTrigger { get; private set; }

        private static Rectangle Empty = new ();

        public int CurrentAction;

        public Method[] PrePatternMethods {  get; private set; }

        public Method[] StatePatternOrder { get; private set; }

        public BossPattern(string[] actions, float?[] durations, FinishModes finishMode = FinishModes.ContinueLoop)
        {
            FinishMode = finishMode;
            CurrentAction = 0;
            int loopSpot = Array.IndexOf(actions, "loop");
            if (loopSpot != -1)
            {
                PrePatternMethods = ArraysToMethods(actions.Take(loopSpot).ToArray(), durations.Take(loopSpot).ToArray());
                StatePatternOrder = ArraysToMethods(actions.Skip(loopSpot + 1).ToArray(), durations.Skip(loopSpot + 1).ToArray());
            }
            else
            {
                StatePatternOrder = ArraysToMethods(actions, durations);
            }
        }

        public void SetInterruptOnHealthBelow(int threshold)
        {
            FinishMode = FinishModes.OnHealthNum;
            HealthThreshold = threshold;

            IterationCount = null;
            GoToPattern = null;
            PlayerPositionTrigger = Empty;
        }

        public void SetInterruptOnLoopCountGoTo(int loop, int target)
        {
            FinishMode = FinishModes.LoopCountGoTo;
            IterationCount = loop;
            GoToPattern = target;

            HealthThreshold = null;
            PlayerPositionTrigger = new Rectangle();
        }

        public void SetInterruptWhenPlayerBetween(int topLeftX, int topLeftY, int bottomRightX, int bottomRightY, int goTo)
        {
            FinishMode = FinishModes.PlayerPositionWithin;
            PlayerPositionTrigger = new Rectangle(topLeftX, topLeftY, bottomRightX - topLeftX, bottomRightY - topLeftY);
            GoToPattern = goTo;

            HealthThreshold = null;
            IterationCount = null;
        }

        private static Method[] ArraysToMethods(string[] names, float?[] durations)
        {
            int length = Math.Min(names.Length, durations.Length);
            Method[] methods = new Method[length];
            for (int i = 0; i < length; i++)
            {
                methods[i] = new Method(names[i], durations[i]);
            }
            return methods;
        }

        /*Patter Format:
         * Pattern {id}
         * Attack/Method + Duration/Pause
         * Attack/Method + Duration/Pause
         * 
         * Goto target / Interrupt when {Health < num / Repeat count GoTo target / Player Between x1 y1 x2 y2 goto target}
         */
    }
}
