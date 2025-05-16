using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Other;
using Monocle;
using System.Xml;
using System.Linq;
using System;
using static Celeste.Mod.BossesHelper.Code.Other.BossActions;
using static Celeste.Mod.BossesHelper.Code.Other.BossPatterns;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    internal static class UserFileReader
    {
        #region XML Files
        #region XML Reading
        public static void ReadPatternFileInto(string filepath, out List<BossPattern> targetOut,
            Vector2 offset, ControllerDelegates delegates)
        {
            targetOut = new List<BossPattern>();
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

                List<Method> methodList = new();
                if (patternNode.LocalName.ToLower().Equals("event"))
                {
                    targetOut.Add(
                        new EventCutscene(patternNode.GetValue("file"), patternNode.GetValueOrDefault<int>("goto"), delegates)
                    );
                    continue;
                }

                int? goTo = patternNode.GetValueOrDefault<int>("goto");
                Hitbox trigger = GetHitboxFromXml(patternNode, offset);
                int? minCount = patternNode.GetValueOrDefault<int>("minRepeat");
                int? count = patternNode.GetValueOrDefault<int>("repeat") ?? minCount ?? (goTo != null ? 0 : null);
                minCount ??= count;
                if (count < minCount)
                    count = minCount;
                
                if (patternNode.LocalName.ToLower().Equals("random"))
                {
                    foreach (XmlNode action in patternNode.ChildNodes)
                    {
                        methodList.AddRange(Enumerable.Repeat(new Method(action.GetValue("file"),
                            action.GetValueOrDefault<float>("wait")), Math.Max(action.GetValueOrDefault("weight", 0), 1)));
                    }
                    targetOut.Add(
                        new RandomPattern(methodList.ToArray(), trigger, minCount, count, goTo, delegates)
                    );
                    continue;
                }

                List<Method> preLoopList = null;
                foreach (XmlNode action in patternNode.ChildNodes)
                {
                    switch (action.LocalName.ToLower())
                    {
                        case "wait":
                            methodList.Add(new Method("wait", action.GetValueOrDefault<float>("time")));
                            break;
                        case "loop":
                            preLoopList = [.. methodList];
                            methodList.Clear();
                            break;
                        default:
                            methodList.Add(new Method(action.GetValue("file"), null));
                            break;
                    }
                }

                targetOut.Add(new SequentialPattern(
                    methodList.ToArray(), preLoopList?.ToArray() ?? [], trigger, minCount, count, goTo, delegates)
                );
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
                if (!baseOptions.TryAdd(tag, newCollider))
                    return;
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

                string tag = hitboxNode.GetValue("tag");
                switch (hitboxNode.LocalName.ToLower())
                {
                    case "hitboxes":
                        baseHitboxOptions ??= new();
                        baseHitboxOptions.Add(hitboxNode.GetValue("tag"), GetAllColliders(hitboxNode));
                        break;
                    case "hurtboxes":
                        baseHurtboxOptions ??= new();
                        baseHurtboxOptions.Add(hitboxNode.GetValue("tag"), GetAllColliders(hitboxNode));
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
        private static T? GetValueOrDefault<T>(this XmlNode source, string tag) where T : struct, IParsable<T>
        {
            return source.Attributes[tag]?.Value.Parse(T.Parse);
        }

        private static T GetValueOrDefault<T>(this XmlNode source, string tag, T value) where T : struct, IParsable<T>
        {
            return source.Attributes[tag]?.Value.Parse(T.Parse) ?? value;
        }

        private static string GetValue(this XmlNode source, string tag)
        {
            return source.Attributes[tag]?.Value ?? "main";
        }

        private static Hitbox GetHitboxFromXml(XmlNode source, float defaultWidth, float defaultHeight)
        {
            return new Hitbox(
                source.GetValueOrDefault("width", defaultWidth), source.GetValueOrDefault("height", defaultHeight),
                source.GetValueOrDefault("xOffset", 0f), source.GetValueOrDefault("yOffset", 0f)
            );
        }

        private static Hitbox GetHitboxFromXml(XmlNode source, Vector2 offset)
        {
            float width = source.GetValueOrDefault("width", 0f);
            float height = source.GetValueOrDefault("height", 0f);
            if (width <= 0 || height <= 0)
                return null;
            return new Hitbox(width, height,
                source.GetValueOrDefault("x", 0f) + offset.X, source.GetValueOrDefault("y", 0f) + offset.Y
            );
        }

        private static Circle GetCircleFromXml(XmlNode source, float defaultRadius)
        {
            return new Circle(
                source.GetValueOrDefault("radius", defaultRadius),
                source.GetValueOrDefault("xOffset", 0f), source.GetValueOrDefault("yOffset", 0f)
            );
        }
        #endregion
        #endregion

        #region Lua Files
        public static void ReadLuaFilesInto(this BossController controller, string attacksPath, string eventsPath,
            string customPath, out Dictionary<string, IBossAction> actions, Player playerRef)
        {
            actions = new();
            string[] paths = { attacksPath, eventsPath };
            for (int i = 0; i < 2; i++)
            {
                string path = paths[i];
                if (!ReadLuaPath(path, out ModAsset luaFiles)) return;
                foreach (ModAsset luaFile in luaFiles.Children)
                {
                    IBossAction action = i == 0
                        ? new BossAttack(luaFile.PathVirtual, playerRef, controller)
                        : new BossEvent(luaFile.PathVirtual, playerRef, controller);
                    if (!actions.TryAdd(luaFile.PathVirtual.Substring(path.Length + 1), action))
                        Logger.Log(LogLevel.Warn, "Bosses Helper", "Dictionary cannot have duplicate keys.\nTwo Lua files with the same name were given.");
                }
            }
            if (!ReadLuaPath(CleanPath(customPath, ".lua"), out ModAsset setupFile)) return;
            controller.Puppet.SetPuppetFunctions(new BossFunctions(setupFile.PathVirtual, playerRef, controller));
        }

        public static void ReadSavePointFunction(this GlobalSavePoint savePoint, string filepath, Player playerRef)
        {
            if (!ReadLuaPath(CleanPath(filepath, ".lua"), out ModAsset saveFile)) return;
            savePoint.LoadFunction(saveFile.PathVirtual, playerRef);
        }

        private static bool ReadLuaPath(string path, out ModAsset asset)
        {
            asset = null;
            if (!Everest.Content.TryGet(path, out ModAsset luaPath))
            {
                Logger.Log(LogLevel.Info, "Bosses Helper", $"No Lua files were found in ${path}.");
                return false;
            }
            asset = luaPath;
            return true;
        }
        #endregion

        #region Other Helper Functions
        private static string CleanPath(string path, string extension)
        {
            if (path == null) return "";
            return path.EndsWith(extension) ? path.Substring(0, path.Length - 4) : path;
        }

        private static int LCM(int a, int b)
        {
            return (a / GFC(a, b)) * b;
        }

        private static int GFC(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
        #endregion
    }
}
