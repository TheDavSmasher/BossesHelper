using System.Collections.Generic;
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
            if (Everest.Content.TryGet("Assets/Bosses/" + BossName + "/Patterns", out ModAsset xml))
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
            string AttacksPath = "Assets/Bosses/" + BossName + "/Attacks";
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
            Dictionary<string, Collider> baseHitboxOptions = null;
            Dictionary<string, Collider> baseHurtboxOptions = null;
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
    }
}
