using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Other;
using Monocle;
using System.Xml;
using System;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    internal static class UserFileReader
    {
        #region XML Files
        #region XML Reading
        public static void ReadPatternFileInto(string filepath, ref List<BossPattern> targetOut, Vector2 offset)
        {
            string path = CleanPath(filepath, ".xml");
            if (Everest.Content.TryGet(path, out ModAsset xml))
            {
                XmlDocument document = new XmlDocument();
                document.Load(xml.Stream);
                XmlNodeList patternsList = document.SelectSingleNode("Patterns").ChildNodes;
                foreach (XmlNode patternNode in patternsList)
                {
                    List<BossPattern.Method> methodList = new();
                    if (patternNode.NodeType == XmlNodeType.Comment)
                    {
                        continue;
                    }
                    if (patternNode.LocalName.ToLower().Equals("random"))
                    {
                        foreach (XmlNode action in patternNode.ChildNodes)
                        {
                            methodList.Add(new BossPattern.Method(action.Attributes["file"].Value, GetValueOrDefaultNullF(action.Attributes["wait"])));
                        }
                        targetOut.Add(new BossPattern(methodList.ToArray()));
                    }
                    else if (patternNode.LocalName.ToLower().Equals("event"))
                    {
                        XmlAttributeCollection attributes = patternNode.Attributes;
                        targetOut.Add(new BossPattern(attributes["file"].Value, GetValueOrDefaultNullI(attributes["goto"])));
                    }
                    else
                    {
                        List<BossPattern.Method> preLoopList = null;
                        foreach (XmlNode action in patternNode.ChildNodes)
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
                        XmlAttributeCollection attributes = patternNode.Attributes;
                        if (attributes.Count > 2)
                        {
                            targetOut.Add(new BossPattern(methodList.ToArray(), preLoopList?.ToArray(),
                                GetValueOrDefaultFloat(attributes["x"]), GetValueOrDefaultFloat(attributes["y"]),
                                GetValueOrDefaultFloat(attributes["width"]), GetValueOrDefaultFloat(attributes["height"]),
                                GetValueOrDefaultNullI(attributes["goto"]), offset));
                        }
                        else if (attributes.Count != 0)
                        {
                            targetOut.Add(new BossPattern(methodList.ToArray(), preLoopList?.ToArray(),
                                GetValueOrDefaultInt(attributes["repeat"]), GetValueOrDefaultNullI(attributes["goto"])));
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

        public static void ReadMetadataFileInto(string filepath, out BossPuppet.HitboxMedatata dataHolder)
        {
            Dictionary<string, Collider> baseHitboxOptions = null;
            Dictionary<string, Collider> baseHurtboxOptions = null;
            Dictionary<string, Collider> bounceHitboxes = null;
            Dictionary<string, Collider> targetCircles = null;

            string path = CleanPath(filepath, ".xml");
            if (Everest.Content.TryGet(path, out ModAsset xml))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xml.Stream);
                XmlNodeList hitboxList = doc.SelectSingleNode("HitboxMetadata").ChildNodes;
                foreach (XmlNode hitboxNode in hitboxList)
                {
                    if (hitboxNode.NodeType == XmlNodeType.Comment)
                    {
                        continue;
                    }
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
                            baseHitboxOptions.Add(GetTagOrMain(hitboxNode), new ColliderList(baseHitboxes.ToArray()));
                            break;
                        case "hurtboxes":
                            baseHurtboxOptions ??= new();
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
                            baseHurtboxOptions.Add(GetTagOrMain(hitboxNode), new ColliderList(baseHurtboxes.ToArray()));
                            break;
                        case "bouncebox":
                            bounceHitboxes ??= new();
                            string tag = GetTagOrMain(hitboxNode);
                            if (bounceHitboxes.ContainsKey(tag))
                            {
                                if (bounceHitboxes[tag] is ColliderList list)
                                {
                                    List<Collider> currentColliders = new(list.colliders);
                                    currentColliders.Add(GetHitboxFromXml(hitboxNode.Attributes, 8f, 6f));
                                    bounceHitboxes[tag] = new ColliderList(currentColliders.ToArray());
                                }
                                else
                                {
                                    bounceHitboxes[tag] = new ColliderList(bounceHitboxes[tag], GetHitboxFromXml(hitboxNode.Attributes, 8f, 6f));
                                }
                                break;
                            }
                            bounceHitboxes.Add(tag, GetHitboxFromXml(hitboxNode.Attributes, 8f, 6f));
                            break;
                        case "target":
                            targetCircles ??= new();
                            string tag_t = GetTagOrMain(hitboxNode);
                            if (targetCircles.ContainsKey(tag_t))
                            {
                                if (targetCircles[tag_t] is ColliderList list)
                                {
                                    List<Collider> currentColliders = new(list.colliders);
                                    currentColliders.Add(GetCircleFromXml(hitboxNode.Attributes, 4f));
                                    targetCircles[tag_t] = new ColliderList(currentColliders.ToArray());
                                }
                                else
                                {
                                    targetCircles[tag_t] = new ColliderList(targetCircles[tag_t], GetCircleFromXml(hitboxNode.Attributes, 4f));
                                }
                                break;
                            }
                            targetCircles.Add(tag_t, GetCircleFromXml(hitboxNode.Attributes, 4f));
                            break;
                    }
                }
            }
            else
            {
                Logger.Log(LogLevel.Warn, "Bosses Helper", "No Hitbox Metadata file found. Boss will use all default hitboxes.");
            }
            dataHolder = new(baseHitboxOptions, baseHurtboxOptions, bounceHitboxes, targetCircles);
        }
        #endregion

        #region XML Helper Functions
        private static float GetValueOrDefaultFloat(XmlAttribute source, float defaultVal = 0f)
        {
            return source != null ? float.Parse(source.Value) : defaultVal;
        }

        private static float? GetValueOrDefaultNullF(XmlAttribute source, float? value = null)
        {
            return source != null ? float.Parse(source.Value) : value;
        }

        private static int GetValueOrDefaultInt(XmlAttribute source, int value = 0)
        {
            return source != null ? int.Parse(source.Value) : value;
        }

        private static int? GetValueOrDefaultNullI(XmlAttribute source, int? value = null)
        {
            return source != null ? int.Parse(source.Value) : value;
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

        private static string GetTagOrMain(XmlNode source)
        {
            XmlAttribute tag = source.Attributes["tag"];
            return tag != null ? tag.Value : "main";
        }
        #endregion
        #endregion

        #region Lua Files
        public static void ReadEventFilesInto(string path, ref Dictionary<string, BossEvent> events, string bossId,
            Player playerRef, BossPuppet puppetRef, BossController.CustceneDelegates custceneDelegates)
        {
            if (Everest.Content.TryGet(path, out ModAsset eventFiles))
            {
                foreach (ModAsset eventFile in eventFiles.Children)
                {
                    events.Add(eventFile.PathVirtual.Substring(path.Length + 1),
                        new BossEvent(eventFile.PathVirtual, bossId, playerRef, puppetRef, custceneDelegates));
                }
            }
            else
            {
                Logger.Log(LogLevel.Info, "Bosses Helper", "No Event files were found.");
            }
        }

        public static void ReadAttackFilesInto(string path, ref Dictionary<string, BossAttack> attacks, string bossId, BossController.AttackDelegates delegates)
        {
            if (Everest.Content.TryGet(path, out ModAsset attackFiles))
            {
                foreach (ModAsset attackFile in attackFiles.Children)
                {
                    attacks.Add(attackFile.PathVirtual.Substring(path.Length + 1), new BossAttack(attackFile.PathVirtual, bossId, delegates));
                }
            }
            else
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to find any Attack files.");
            }
        }

        public static void ReadCustomCodeFileInto(string filepath, out BossFunctions functions, string bossId, BossController.OnHitDelegates delegates)
        {
            string path = CleanPath(filepath, ".lua");
            if (Everest.Content.TryGet(path, out ModAsset onHitFile))
            {
                functions = new BossFunctions(onHitFile.PathVirtual, bossId, delegates);
            }
            else
            {
                Logger.Log(LogLevel.Info, "Bosses Helper", "No Lua file found for custom setup.");
                functions = null;
            }
        }
        #endregion

        private static string CleanPath(string path, string extension)
        {
            return path.EndsWith(extension) ? path.Substring(0, path.Length - 4) : path;
        }
    }
}
