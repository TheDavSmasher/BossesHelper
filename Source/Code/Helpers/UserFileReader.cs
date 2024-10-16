﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Other;
using Monocle;
using System.Xml;
using System.IO;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    internal static class UserFileReader
    {
        public static void ReadPatternFileInto(string filepath, ref List<BossPattern> targetOut)
        {
            string path = CleanPath(filepath, ".xml");
            if (Everest.Content.TryGet(path, out ModAsset xml))
            {
                XmlDocument document = new XmlDocument();
                document.Load(xml.Stream);
                XmlNodeList patternsList = document.SelectSingleNode("Patterns").ChildNodes;
                foreach (XmlNode pattern in patternsList)
                {
                    List<BossPattern.Method> methodList = new();
                    if (pattern.LocalName.ToLower().Equals("random"))
                    {
                        foreach (XmlNode action in pattern.ChildNodes)
                        {
                            methodList.Add(new BossPattern.Method(action.Attributes["file"].Value, GetValueOrDefaultNull(action.Attributes["wait"])));
                        }
                        targetOut.Add(new BossPattern(methodList.ToArray()));
                    }
                    else
                    {
                        List<BossPattern.Method> preLoopList = null;
                        foreach (XmlNode action in pattern.ChildNodes)
                        {
                            if (action.LocalName.ToLower().Equals("wait"))
                            {
                                methodList.Add(new BossPattern.Method("wait", float.Parse(action.Attributes["time"].Value)));
                            }
                            else if (action.LocalName.ToLower().Equals("loop"))
                            {
                                preLoopList = new(methodList);
                                methodList.Clear();
                            }
                            else
                            {
                                methodList.Add(new BossPattern.Method(action.Attributes["file"].Value, null));
                            }
                        }
                        XmlAttributeCollection attributes = pattern.Attributes;
                        if (attributes.Count > 2)
                        {
                            targetOut.Add(new BossPattern(methodList.ToArray(), preLoopList?.ToArray(),GetValueOrDefaultInt(attributes["x"]), GetValueOrDefaultInt(attributes["y"]),
                                GetValueOrDefaultInt(attributes["width"]), GetValueOrDefaultInt(attributes["height"]), GetValueOrDefaultInt(attributes["goto"])));
                        }
                        else if (attributes.Count != 0)
                        {
                            targetOut.Add(new BossPattern(methodList.ToArray(), preLoopList?.ToArray(), GetValueOrDefaultInt(attributes["repeat"]), GetValueOrDefaultInt(attributes["goto"])));
                        }
                        else
                        {
                            targetOut.Add(new BossPattern(methodList.ToArray(), preLoopList?.ToArray()));
                        }
                    }
                }
            }            
            else
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to find any Pattern file.");
            }
        }

        private static float? GetValueOrDefaultNull(XmlAttribute source, float? value = null)
        {
            return source != null ? float.Parse(source.Value) : value;
        }

        private static int GetValueOrDefaultInt(XmlAttribute source, int value = 0)
        {
            return source != null ? int.Parse(source.Value) : value;
        }

        public static void ReadEventFilesInto(string path, ref Dictionary<string, BossEvent> events, Player playerRef, BossPuppet puppetRef)
        {
            if (Everest.Content.TryGet(path, out ModAsset eventFiles))
            {
                foreach (ModAsset eventFile in eventFiles.Children)
                {
                    events.Add(eventFile.PathVirtual.Substring(path.Length + 1), new BossEvent(eventFile.PathVirtual, playerRef, puppetRef));
                }
            }
            else
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to find any Event files.");
            }
        }

        public static void ReadAttackFilesInto(string path, ref Dictionary<string, BossAttack> attacks, BossController.AttackDelegates delegates)
        {
            if (Everest.Content.TryGet(path, out ModAsset attackFiles))
            {
                foreach (ModAsset attackFile in attackFiles.Children)
                {
                    attacks.Add(attackFile.PathVirtual.Substring(path.Length + 1), new BossAttack(attackFile.PathVirtual, delegates));
                }
            }
            else
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to find any Attack files.");
            }
        }

        public static void ReadCustomCodeFileInto(string filepath, ref BossFunctions functions, BossController.OnHitDelegates delegates)
        {
            string path = CleanPath(filepath, ".lua");
            if (Everest.Content.TryGet(path, out ModAsset onHitFile))
            {
                functions = new BossFunctions(onHitFile.PathVirtual, delegates);
            }
            else
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to find any Lua file.");
            }
        }

        public static void ReadMetadataFileInto(string filepath, out BossPuppet.HitboxMedatata dataHolder)
        {
            Dictionary<string, Collider> baseHitboxOptions = null;
            Dictionary<string, Collider> baseHurtboxOptions = null;
            Hitbox bounceHitboxes = null;
            Vector2 targetOffset = Vector2.Zero;
            float radiusT = 4f;

            string path = CleanPath(filepath, ".xml");
            if (Everest.Content.TryGet(path, out ModAsset xml))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xml.Stream);
                XmlNodeList hitboxList = doc.SelectSingleNode("HitboxMetadata").ChildNodes;
                foreach (XmlNode hitboxNode in hitboxList)
                {
                    switch (hitboxNode.LocalName.ToLower())
                    {
                        case "hitboxes":
                            baseHitboxOptions ??= new();
                            List<Collider> baseHitboxes = new();
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
                            if (baseHitboxes.Count > 1)
                            {
                                baseHitboxOptions.Add(GetTagOrMain(hitboxNode), new ColliderList(baseHitboxes.ToArray()));
                            }
                            else
                            {
                                baseHitboxOptions.Add(GetTagOrMain(hitboxNode), baseHitboxes[0]);
                            }
                            break;
                        case "hurtboxes":
                            baseHitboxOptions ??= new();
                            List<Collider> baseHurtboxes = new();
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
                            if (baseHurtboxes.Count > 1)
                            {
                                baseHitboxOptions.Add(GetTagOrMain(hitboxNode), new ColliderList(baseHurtboxes.ToArray()));
                            }
                            else
                            {
                                baseHitboxOptions.Add(GetTagOrMain(hitboxNode), baseHurtboxes[0]);
                            }
                            break;
                        case "bouncebox":
                            bounceHitboxes = GetHitboxFromXml(hitboxNode.Attributes, 8f, 6f);
                            break;
                        case "target":
                            XmlAttributeCollection targetData = hitboxNode.Attributes;
                            targetOffset = new Vector2(GetValueOrDefaultFloat(targetData["xOffset"]), GetValueOrDefaultFloat(targetData["yOffset"]));
                            radiusT = GetValueOrDefaultFloat(targetData["radius"], 4f);
                            break;
                    }
                }
            }
            else
            {
                Logger.Log(LogLevel.Warn, "Bosses Helper", "No Hitbox Metadata file found. Boss will use all default hitboxes.");
            }
            dataHolder = new(baseHitboxOptions, baseHurtboxOptions, bounceHitboxes, targetOffset, radiusT);
        }

        private static Hitbox GetHitboxFromXml(XmlAttributeCollection source, float defaultWidth, float defaultHeight)
        {
            return new Hitbox(GetValueOrDefaultFloat(source["width"], defaultWidth), GetValueOrDefaultFloat(source["height"], defaultHeight),
                GetValueOrDefaultFloat(source["xOffset"]), GetValueOrDefaultFloat(source["yOffset"]));
        }

        private static Circle GetCircleFromXml(XmlAttributeCollection source, float defaultRadius)
        {
            return new Circle(GetValueOrDefaultFloat(source["radius"], defaultRadius), GetValueOrDefaultFloat(source["xOffset"]), GetValueOrDefaultFloat(source["yOffset"]));
        }

        private static float GetValueOrDefaultFloat(XmlAttribute source, float defaultVal = 0f)
        {
            return source != null ? float.Parse(source.Value) : defaultVal;
        }

        private static string GetTagOrMain(XmlNode source)
        {
            return source["tag"] != null ? source["tag"].Value : "main";
        }

        private static string CleanPath(string path, string extension)
        {
            return path.EndsWith(extension) ? path.Substring(0, path.Length - 4) : path;
        }
    }
}
