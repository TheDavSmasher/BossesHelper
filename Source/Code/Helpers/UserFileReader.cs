using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Other;
using Monocle;
using NLua;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    internal class UserFileReader : Component
    {
        private readonly string MasterFilePath;

        private readonly string AttacksPath;

        private readonly string EventsPath;

        private readonly string PatternPath;

        public UserFileReader(string bossName)
            : base(true, false)
        {
            MasterFilePath = "Assets/Bosses/" + bossName;
            AttacksPath = MasterFilePath + "/Attacks";
            EventsPath = MasterFilePath + "/Events";
            PatternPath = MasterFilePath + "/Patterns";
        }

        public void ReadPatternFilesInto(ref List<BossPattern> targetOut)
        {
            if (Everest.Content.TryGet(PatternPath, out ModAsset patternBranch))
            {
                targetOut = new List<BossPattern>(new BossPattern[patternBranch.Children.Count]);
                foreach (ModAsset pattern in patternBranch.Children)
                {
                    List<string> lines = new();
                    using (StreamReader reader = new StreamReader(pattern.Stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            lines.Add(reader.ReadLine());
                        }
                        reader.Close();
                    }
                    List<string> actions = new();
                    List<float?> floats = new();
                    int currentIndex = 0;
                    int patternID = 0;
                    BossPattern.FinishMode mode = BossPattern.FinishMode.ContinueLoop;
                    int[] vals = Enumerable.Repeat(0, 5).ToArray();
                    while (currentIndex < lines.Count)
                    {
                        string[] currentLine = lines[currentIndex].TrimStart(' ').Split(null);
                        switch (currentLine[0].ToLower())
                        {
                            case "pattern":
                                patternID = int.Parse(currentLine[1]) - 1;
                                break;
                            case "wait":
                                actions.Add(currentLine[0]);
                                floats.Add(float.Parse(currentLine[1]));
                                break;
                            case "goto":
                                mode = BossPattern.FinishMode.LoopCountGoTo;
                                vals[0] = 0;
                                vals[1] = int.Parse(currentLine[1]);
                                break;
                            case "interrupt":
                                if (currentLine[2].ToLower().Equals("repeat"))
                                {
                                    mode = BossPattern.FinishMode.LoopCountGoTo;
                                    vals[0] = int.Parse(currentLine[3]);
                                    vals[1] = int.Parse(currentLine[5]);
                                }
                                else if (currentLine[2].ToLower().Equals("health"))
                                {
                                    mode = BossPattern.FinishMode.OnHealthNum;
                                    vals[0] = int.Parse(currentLine[4]);
                                }
                                else if (currentLine[2].ToLower().Equals("player"))
                                {
                                    mode = BossPattern.FinishMode.PlayerPositionWithin;
                                    vals[0] = int.Parse(currentLine[4]);
                                    vals[1] = int.Parse(currentLine[5]);
                                    vals[2] = int.Parse(currentLine[6]);
                                    vals[3] = int.Parse(currentLine[7]);
                                    vals[4] = int.Parse(currentLine[9]);
                                }
                                break;
                            default:
                                if (!string.IsNullOrEmpty(currentLine[0]))
                                {
                                    actions.Add(currentLine[0]);
                                    floats.Add(null);
                                }
                                break;
                        }
                        currentIndex++;
                    }
                    BossPattern result = new BossPattern(actions.ToArray(), floats.ToArray());
                    switch (mode)
                    {
                        case BossPattern.FinishMode.LoopCountGoTo:
                            result.SetInterruptOnLoopCountGoTo(vals[0], vals[1]);
                            break;
                        case BossPattern.FinishMode.OnHealthNum:
                            result.SetInterruptOnHealthBelow(vals[0]);
                            break;
                        case BossPattern.FinishMode.PlayerPositionWithin:
                            result.SetInterruptWhenPlayerBetween(vals[0], vals[1], vals[2], vals[3], vals[4]);
                            break;
                        default:
                            break;
                    }
                    targetOut[patternID] = result;
                }
            }
            else
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to find any Pattern files.");
            }
        }

        public void ReadPatternOrderFileInto(ref List<int> target, int nodeCount = 1)
        {
            string patternFilePath = MasterFilePath + "/PatternOrder";
            if (Everest.Content.TryGet(patternFilePath, out ModAsset orderFile))
            {
                List<string> lines = new();
                using (StreamReader reader = new StreamReader(orderFile.Stream))
                {
                    while (!reader.EndOfStream)
                    {
                        lines.Add(reader.ReadLine());
                    }
                    reader.Close();
                }
                int currentIndex = 0;
                while (currentIndex < lines.Count)
                {
                    int patternID = int.Parse(lines[currentIndex]);
                    if (patternID > 0)
                    {
                        target.Add(patternID - 1);
                    }
                    currentIndex++;
                }
            }
            else
            {
                Logger.Log(LogLevel.Warn, "Bosses Helper", "Failed to find any Pattern Order file.\nWill use default numerical order.");
                for (int i = 0; i < nodeCount; i++)
                {
                    target.Add(i);
                }
            }
        }

        public void ReadEventFilesInto(ref Dictionary<string, BossEvent> events, Player playerRef, BossPuppet puppetRef)
        {
            if (Everest.Content.TryGet(EventsPath, out ModAsset eventFiles))
            {
                foreach (ModAsset eventFile in eventFiles.Children)
                {
                    events.Add(eventFile.PathVirtual.Substring(EventsPath.Length + 1), new BossEvent(eventFile.PathVirtual, playerRef, puppetRef));
                }
            }
            else
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to find any Event files.");
            }
        }

        public void ReadAttackFilesInto(ref Dictionary<string, BossAttack> attacks, Player playerRef, BossPuppet puppetRef, BossController.AttackDelegates delegates)
        {
            if (Everest.Content.TryGet(AttacksPath, out ModAsset attackFiles))
            {
                foreach (ModAsset attackFile in attackFiles.Children)
                {
                    attacks.Add(attackFile.PathVirtual.Substring(AttacksPath.Length + 1), new BossAttack(attackFile.PathVirtual, playerRef, puppetRef, delegates));
                }
            }
            else
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to find any Attack files.");
            }
        }

        public void ReadOnHitFileInto(ref BossInterruption onHit, Player playerRef, BossPuppet puppetRef, BossController.OnHitDelegates delegates)
        {
            if (Everest.Content.TryGet(MasterFilePath + "/OnHit", out ModAsset onHitFile))
            {
                onHit = new BossInterruption(onHitFile.PathVirtual, playerRef, puppetRef, delegates);
            }
            else
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to find an OnHit file.");
            }
        }

        public void ReadMetadataFileInto(out BossPuppet.HitboxMedatata dataHolder)
        {
            List<Collider> baseHitboxes = null;
            Hitbox bounceHitboxes = null;
            Vector2 targetOffset = Vector2.Zero;
            float radius = 4f;
            if (Everest.Content.TryGet(MasterFilePath + "/Metadata", out ModAsset metadata))
            {
                List<string> lines = new();
                using (StreamReader reader = new StreamReader(metadata.Stream))
                {
                    while (!reader.EndOfStream)
                    {
                        lines.Add(reader.ReadLine());
                    }
                    reader.Close();
                }
                int currentIndex = 0;
                while (currentIndex < lines.Count)
                {
                    string[] currentLine = lines[currentIndex].TrimStart(' ').Split(null);
                    if (currentLine[0].ToLower().Equals("base"))
                    {
                        baseHitboxes = new List<Collider>();
                        do
                        {
                            currentIndex++;
                            currentLine = lines[currentIndex].TrimStart(' ').Split(null);
                            if (currentLine[0].ToLower().Equals("hitbox"))
                            {
                                baseHitboxes.Add(new Hitbox(float.Parse(currentLine[1]), float.Parse(currentLine[2]), float.Parse(currentLine[3]), float.Parse(currentLine[4])));
                            }
                            else if (currentLine[0].ToLower().Equals("circle"))
                            {
                                baseHitboxes.Add(new Circle(float.Parse(currentLine[1]), float.Parse(currentLine[2]), float.Parse(currentLine[3])));
                            }
                        }
                        while (!string.IsNullOrEmpty(currentLine[0]));
                    }
                    else if (currentLine[0].ToLower().Equals("bounce"))
                    {
                        currentIndex++;
                        currentLine = lines[currentIndex].TrimStart(' ').Split(null);
                        if (!string.IsNullOrEmpty(currentLine[0]))
                        {
                            float height = string.IsNullOrEmpty(currentLine[3]) ? 6f : float.Parse(currentLine[3]);
                            bounceHitboxes = new Hitbox(float.Parse(currentLine[2]), height, float.Parse(currentLine[0]), float.Parse(currentLine[1]));
                        }
                    }
                    else if (!currentLine[0].ToLower().Equals("target"))
                    {
                        currentIndex++;
                        currentLine = lines[currentIndex].TrimStart(' ').Split(null);
                        if (!string.IsNullOrEmpty(currentLine[0]))
                        {
                            targetOffset.X = float.Parse(currentLine[0]);
                            targetOffset.Y = currentLine.Length > 1 ? float.Parse(currentLine[1]) : 0f;
                            radius = currentLine.Length > 2 ? float.Parse(currentLine[1]) : 4f;
                        }
                    }
                    currentIndex++;
                }
            }
            dataHolder = new(baseHitboxes, bounceHitboxes, targetOffset, radius);
        }
    }
}
