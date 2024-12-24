using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Other;
using Monocle;
using System.Xml;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    internal static class UserFileReader
    {
        #region XML Files
        #region XML Reading
        public static void ReadPatternFileInto(string filepath, ref List<BossPattern> targetOut, Vector2 offset)
        {
            string path = CleanPath(filepath, ".xml");
            if (!Everest.Content.TryGet(path, out ModAsset xml))
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to find any Pattern file.");
                return;
            }
            XmlDocument document = new XmlDocument();
            document.Load(xml.Stream);
            XmlNodeList patternsList = document.SelectSingleNode("Patterns").ChildNodes;
            foreach (XmlNode patternNode in patternsList)
            {
                if (patternNode.NodeType == XmlNodeType.Comment) continue;

                List<BossPattern.Method> methodList = new();
                if (patternNode.LocalName.ToLower().Equals("event"))
                {
                    targetOut.Add(new BossPattern(patternNode.GetValue("file"), patternNode.GetValueOrDefaultNullI("goto")));
                    continue;
                }

                if (patternNode.LocalName.ToLower().Equals("random"))
                {
                    foreach (XmlNode action in patternNode.ChildNodes)
                    {
                        methodList.AddRange(Enumerable.Repeat(new BossPattern.Method(action.GetValue("file"),
                            action.GetValueOrDefaultNullF("wait")), action.GetValueOrDefaultInt("weight", 1)));
                    }
                    targetOut.Add(new BossPattern(methodList.ToArray()));
                    continue;
                }

                List<BossPattern.Method> preLoopList = null;
                foreach (XmlNode action in patternNode.ChildNodes)
                {
                    switch (action.LocalName.ToLower())
                    {
                        case "wait":
                            methodList.Add(new BossPattern.Method("wait", float.Parse(action.GetValue("time"))));
                            break;
                        case "loop":
                            preLoopList = new(methodList);
                            methodList.Clear();
                            break;
                        default:
                            methodList.Add(new BossPattern.Method(action.GetValue("file"), null));
                            break;
                    }
                }
                int? goTo = patternNode.GetValueOrDefaultNullI("goto");
                if (goTo != null)
                {
                    targetOut.Add(new BossPattern(methodList.ToArray(), preLoopList?.ToArray(),
                        GetHitboxFromXml(patternNode, offset), (ulong)patternNode.GetValueOrDefaultInt("repeat"), goTo));
                }
                else
                {
                    targetOut.Add(new BossPattern(methodList.ToArray(), preLoopList?.ToArray()));
                }
            }
        }

        public static void ReadMetadataFileInto(string filepath, out BossPuppet.HitboxMedatata dataHolder)
        {
            static Collider GetAllColliders(XmlNode source)
            {
                List<Collider> baseOptions = new();
                foreach (XmlElement baseOption in source.ChildNodes)
                {
                    baseOptions.Add(baseOption.LocalName.ToLower().Equals("circle")
                        ? GetCircleFromXml(baseOption, 4f) : GetHitboxFromXml(baseOption, 8f, 8f));
                }
                return baseOptions.Count > 1 ? new ColliderList(baseOptions.ToArray()) : baseOptions.First();
            }

            static void InsertNewCollider(Dictionary<string, Collider> baseOptions, string tag, Collider newCollider)
            {
                if (!baseOptions.ContainsKey(tag))
                {
                    baseOptions.Add(tag, newCollider);
                    return;
                }
                baseOptions[tag] = baseOptions[tag] is ColliderList list
                    ? new ColliderList([.. list.colliders, newCollider])
                    : new ColliderList(baseOptions[tag], newCollider);
            }

            Dictionary<string, Collider> baseHitboxOptions = null;
            Dictionary<string, Collider> baseHurtboxOptions = null;
            Dictionary<string, Collider> bounceHitboxes = null;
            Dictionary<string, Collider> targetCircles = null;

            string path = CleanPath(filepath, ".xml");
            if (!Everest.Content.TryGet(path, out ModAsset xml))
            {
                Logger.Log(LogLevel.Warn, "Bosses Helper", "No Hitbox Metadata file found. Boss will use all default hitboxes.");
                dataHolder = default;
                return;
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(xml.Stream);
            XmlNodeList hitboxList = doc.SelectSingleNode("HitboxMetadata").ChildNodes;
            foreach (XmlNode hitboxNode in hitboxList)
            {
                if (hitboxNode.NodeType == XmlNodeType.Comment) continue;

                string tag = hitboxNode.GetTagOrMain();
                switch (hitboxNode.LocalName.ToLower())
                {
                    case "hitboxes":
                        baseHitboxOptions ??= new();
                        baseHitboxOptions.Add(hitboxNode.GetTagOrMain(), GetAllColliders(hitboxNode));
                        break;
                    case "hurtboxes":
                        baseHurtboxOptions ??= new();
                        baseHurtboxOptions.Add(hitboxNode.GetTagOrMain(), GetAllColliders(hitboxNode));
                        break;
                    case "bouncebox":
                        bounceHitboxes ??= new();
                        InsertNewCollider(bounceHitboxes, tag, GetHitboxFromXml(hitboxNode, 8f, 6f));
                        break;
                    case "target":
                        targetCircles ??= new();
                        InsertNewCollider(targetCircles, tag, GetCircleFromXml(hitboxNode, 4f));
                        break;
                }
            }
            dataHolder = new(baseHitboxOptions, baseHurtboxOptions, bounceHitboxes, targetCircles);
        }
        #endregion

        #region XML Helper Functions
        private static float GetValueOrDefaultFloat(this XmlNode source, string tag, float value = 0f)
        {
            XmlAttribute attribute = source.Attributes[tag];
            return attribute != null ? float.Parse(attribute.Value) : value;
        }

        private static float? GetValueOrDefaultNullF(this XmlNode source, string tag, float? value = null)
        {
            XmlAttribute attribute = source.Attributes[tag];
            return attribute != null ? float.Parse(attribute.Value) : value;
        }

        private static int GetValueOrDefaultInt(this XmlNode source, string tag, int value = 0)
        {
            XmlAttribute attribute = source.Attributes[tag];
            return attribute != null ? int.Parse(attribute.Value) : value;
        }

        private static int? GetValueOrDefaultNullI(this XmlNode source, string tag, int? value = null)
        {
            XmlAttribute attribute = source.Attributes[tag];
            return attribute != null ? int.Parse(attribute.Value) : value;
        }

        private static string GetTagOrMain(this XmlNode source)
        {
            XmlAttribute attribute = source.Attributes["tag"];
            return attribute != null ? attribute.Value : "main";
        }

        private static string GetValue(this XmlNode source, string tag)
        {
            return source.Attributes[tag].Value;
        }

        private static Hitbox GetHitboxFromXml(XmlNode source, float defaultWidth, float defaultHeight)
        {
            return new Hitbox(source.GetValueOrDefaultFloat("width", defaultWidth), source.GetValueOrDefaultFloat("height", defaultHeight),
                source.GetValueOrDefaultFloat("xOffset"), source.GetValueOrDefaultFloat("yOffset"));
        }

        private static Hitbox GetHitboxFromXml(XmlNode source, Vector2 offset)
        {
            float width = source.GetValueOrDefaultFloat("width");
            float height = source.GetValueOrDefaultFloat("height");
            if (width <= 0 || height <= 0)
                return null;
            return new Hitbox(width, height, source.GetValueOrDefaultFloat("x") + offset.X,
                source.GetValueOrDefaultFloat("y") + offset.Y);
        }

        private static Circle GetCircleFromXml(XmlNode source, float defaultRadius)
        {
            return new Circle(source.GetValueOrDefaultFloat("radius", defaultRadius), source.GetValueOrDefaultFloat("xOffset"), source.GetValueOrDefaultFloat("yOffset"));
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
