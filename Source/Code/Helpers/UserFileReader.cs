using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Other;
using Monocle;
using System.Xml;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    internal static class UserFileReader
    {
        public static string BossName;

        public static void ReadPatternFilesInto(ref List<BossPattern> targetOut)
        {
            if (Everest.Content.TryGet("Assets/Bosses/" + BossName + "/Patterns", out ModAsset patternBranch))
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
                    BossPattern.FinishModes mode = BossPattern.FinishModes.ContinueLoop;
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
                                mode = BossPattern.FinishModes.LoopCountGoTo;
                                vals[0] = 0;
                                vals[1] = int.Parse(currentLine[1]);
                                break;
                            case "interrupt":
                                if (currentLine[2].ToLower().Equals("repeat"))
                                {
                                    mode = BossPattern.FinishModes.LoopCountGoTo;
                                    vals[0] = int.Parse(currentLine[3]);
                                    vals[1] = int.Parse(currentLine[5]);
                                }
                                else if (currentLine[2].ToLower().Equals("health"))
                                {
                                    mode = BossPattern.FinishModes.OnHealthNum;
                                    vals[0] = int.Parse(currentLine[4]);
                                }
                                else if (currentLine[2].ToLower().Equals("player"))
                                {
                                    mode = BossPattern.FinishModes.PlayerPositionWithin;
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
                        case BossPattern.FinishModes.LoopCountGoTo:
                            result.SetInterruptOnLoopCountGoTo(vals[0], vals[1]);
                            break;
                        case BossPattern.FinishModes.OnHealthNum:
                            result.SetInterruptOnHealthBelow(vals[0]);
                            break;
                        case BossPattern.FinishModes.PlayerPositionWithin:
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

        public static void ReadPatternOrderFileInto(ref List<int> target, int nodeCount = 1)
        {
            if (Everest.Content.TryGet("Assets/Bosses/" + BossName + "/PatternOrder", out ModAsset orderFile))
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

        public static void ReadEventFilesInto(ref Dictionary<string, BossEvent> events, Player playerRef, BossPuppet puppetRef)
        {
            string EventsPath = "Assets/Bosses/" + BossName + "/Events";
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

        public static void ReadAttackFilesInto(ref Dictionary<string, BossAttack> attacks, BossController.AttackDelegates delegates)
        {
            string AttacksPath = "Assets/Bosses/" + BossName + "/Events";
            if (Everest.Content.TryGet(AttacksPath, out ModAsset attackFiles))
            {
                foreach (ModAsset attackFile in attackFiles.Children)
                {
                    attacks.Add(attackFile.PathVirtual.Substring(AttacksPath.Length + 1), new BossAttack(attackFile.PathVirtual, delegates));
                }
            }
            else
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to find any Attack files.");
            }
        }

        public static void ReadOnHitFileInto(ref BossInterruption onHit, BossController.OnHitDelegates delegates)
        {
            if (Everest.Content.TryGet("Assets/Bosses/" + BossName + "/OnDamage", out ModAsset onHitFile))
            {
                onHit = new BossInterruption(onHitFile.PathVirtual, delegates);
            }
            else
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to find an OnHit file.");
            }
        }

        public static void ReadCustomSetupFile(Player player, BossPuppet puppet)
        {
            if (Everest.Content.TryGet("Assets/Bosses/" + BossName + "/Setup", out ModAsset customSetupFile))
            {
                LuaBossHelper.DoCustomSetup(customSetupFile.PathVirtual, player, puppet);
            }
            else
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to find a Setup file.");
            }
        }

        public static void ReadMetadataFileInto(out BossPuppet.HitboxMedatata dataHolder)
        {
            List<Collider> baseHitboxes = null;
            List<Collider> baseHurtboxes = null;
            Hitbox bounceHitboxes = null;
            Vector2 targetOffset = Vector2.Zero;
            float radiusT = 4f;
            if (Everest.Content.TryGet("Assets/Bosses/" + BossName + "/Metadata", out ModAsset xml))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xml.Stream);
                XmlNodeList hitboxList = doc.SelectSingleNode("HitboxMetadata").ChildNodes;
                foreach (XmlNode hitboxNode in hitboxList)
                {
                    switch (hitboxNode.LocalName.ToLower())
                    {
                        case "hitboxes":
                            baseHitboxes = new();
                            foreach (XmlElement baseHitbox in hitboxNode.ChildNodes)
                            {
                                if (baseHitbox.LocalName.ToLower().Equals("circle"))
                                {
                                    baseHitboxes.Add(GetCircleFromXml(baseHitbox.Attributes, 4f));
                                }
                                else
                                {
                                    baseHitboxes.Add(GetHitboxFromXml(baseHitbox.Attributes, 8f, 8f));
                                }
                            }
                            break;
                        case "hurtboxes":
                            baseHurtboxes = new();
                            foreach (XmlNode baseHurtbox in hitboxNode.ChildNodes)
                            {
                                if (baseHurtbox.LocalName.ToLower().Equals("circle"))
                                {
                                    baseHurtboxes.Add(GetCircleFromXml(baseHurtbox.Attributes, 4f));
                                }
                                else
                                {
                                    baseHurtboxes.Add(GetHitboxFromXml(baseHurtbox.Attributes, 8f, 8f));
                                }
                            }
                            break;
                        case "bouncebox":
                            XmlAttributeCollection bounceboxData = hitboxNode.Attributes;
                            break;
                        case "target":
                            XmlAttributeCollection targetData = hitboxNode.Attributes;
                            break;
                    }
                }
            }                                  
            dataHolder = new(baseHitboxes, baseHurtboxes, bounceHitboxes, targetOffset, radiusT);
        }

        private static Hitbox GetHitboxFromXml(XmlAttributeCollection source, float defaultWidth, float defaultHeight)
        {
            return new Hitbox(GetValueOrDefault(source["width"], defaultWidth), GetValueOrDefault(source["height"], defaultHeight),
                GetValueOrDefault(source["xOffset"]), GetValueOrDefault(source["yOffset"]));
        }

        private static Circle GetCircleFromXml(XmlAttributeCollection source, float defaultRadius)
        {
            return new Circle(GetValueOrDefault(source["radius"], defaultRadius), GetValueOrDefault(source["xOffset"]), GetValueOrDefault(source["yOffset"]));
        }

        private static float GetValueOrDefault(XmlAttribute source, float defaultVal = 0f)
        {
            return source != null ? float.Parse(source.Value) : defaultVal;
        }
    }
}
