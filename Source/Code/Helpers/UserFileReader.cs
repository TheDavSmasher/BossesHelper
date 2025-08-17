using Celeste.Mod.BossesHelper.Code.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using static Celeste.Mod.BossesHelper.Code.Entities.BossPuppet;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    using LuaPathReader = (string Path, Func<string, BossController, IBossAction> Creator);

    internal static class UserFileReader
    {
        #region XML Files
        #region XML Reading
        public static List<BossPattern> ReadPatternFile(string filepath, Vector2 offset,
            Dictionary<string, IBossAction> actions, ControllerDelegates delegates)
        {
            List<BossPattern> targetOut = [];

            ReadXMLFile(filepath, "Failed to find any Pattern file.", "Patterns", patternNode =>
            {
                BossPattern newPattern = patternNode.ParseNewPattern(offset, actions, delegates);
                targetOut.Add(newPattern);
            });
            return targetOut;
        }

        private static BossPattern ParseNewPattern(this XmlNode patternNode, Vector2 offset,
            Dictionary<string, IBossAction> actions, ControllerDelegates delegates)
        {
            string nodeType = patternNode.LocalName.ToLower();
            List<Method> methodList = [];

            string patternName = patternNode.GetValue("name");
            string goTo = patternNode.GetValue("goto");
            if (nodeType.Equals("event"))
            {
                return new EventCutscene(patternName, patternNode.GetMethod(true), goTo, actions, delegates);
            }

            Hitbox trigger = patternNode.GetHitbox(offset);
            int? minCount = patternNode.GetValueOrDefault<int>("minRepeat");
            int? count = patternNode.GetValueOrDefault<int>("repeat") ?? minCount ?? (goTo is null ? null : 0);
            minCount ??= count;
            if (count < minCount)
                count = minCount;

            if (nodeType.Equals("random"))
            {
                foreach (XmlNode action in patternNode.ChildNodes)
                {
                    methodList.AddRange(Enumerable.Repeat(
                        action.GetMethod(true, true),
                        Math.Max(action.GetValueOrDefault("weight", 0), 1)
                    ));
                }
                return new RandomPattern(patternName, methodList, trigger, minCount, count, goTo, actions, delegates);
            }

            List<Method> preLoopList = [];
            foreach (XmlNode action in patternNode.ChildNodes)
            {
                switch (action.LocalName.ToLower())
                {
                    case "wait":
                        methodList.Add(action.GetMethod(false));
                        break;
                    case "loop":
                        preLoopList = [.. methodList];
                        methodList.Clear();
                        break;
                    default:
                        methodList.Add(action.GetMethod(true));
                        break;
                }
            }

            return new SequentialPattern(patternName, methodList, preLoopList, trigger, minCount, count, goTo, actions, delegates);
        }

        public static EnumDict<ColliderOption, Dictionary<string, Collider>> ReadMetadataFile(string filepath)
        {
            EnumDict<ColliderOption, Dictionary<string, Collider>> dataHolder = new(_ => []);

            ReadXMLFile(filepath, "No Hitbox Metadata file found. Boss will use all default hitboxes.", "HitboxMetadata", hitboxNode =>
            {
                ColliderOption option = Enum.Parse<ColliderOption>(hitboxNode.LocalName, true);
                dataHolder[option].InsertNewCollider(hitboxNode.GetValue("tag") ?? "main", option switch
                {
                    ColliderOption.Hitboxes or ColliderOption.Hurtboxes => hitboxNode.GetAllColliders(),
                    ColliderOption.Bouncebox => hitboxNode.GetHitbox(8f, 6f),
                    ColliderOption.Target => hitboxNode.GetCircle(),
                    _ => null
                });
            });
            return dataHolder;
        }
        #endregion

        #region XML Helper Functions
        private static void ReadXMLFile(string filepath, string error, string node, Action<XmlNode> nodeReader)
        {
            if (!Everest.Content.TryGet(CleanPath(filepath, ".xml"), out ModAsset xml))
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", error);
                return;
            }
            XmlDocument document = new();
            document.Load(xml.Stream);
            foreach (XmlNode xmlNode in document.SelectSingleNode(node).ChildNodes)
            {
                if (xmlNode.NodeType == XmlNodeType.Comment) continue;

                nodeReader(xmlNode);
            }
        }

        private static T GetValueOrDefault<T>(this XmlNode source, string tag, T value) where T : struct, IParsable<T>
        {
            return source.GetValueOrDefault<T>(tag) ?? value;
        }

        private static T? GetValueOrDefault<T>(this XmlNode source, string tag) where T : struct, IParsable<T>
        {
            return source.GetValue(tag)?.Parse<T>();
        }

        private static string GetValue(this XmlNode source, string tag)
        {
            return source.Attributes[tag]?.Value;
        }

        private static Method GetMethod(this XmlNode source, bool isFile, bool hasTime = false)
        {
            return new Method(
                isFile ? source.GetValue("file") : "wait",
                source.GetValueOrDefault<float>(!isFile || hasTime ? "time" : "")
            );
        }

        private static ColliderList GetAllColliders(this XmlNode source)
        {
            List<Collider> baseOptions = [];
            foreach (XmlElement baseOption in source.ChildNodes)
            {
                baseOptions.Add(baseOption.LocalName.ToLower().Equals("circle")
                    ? baseOption.GetCircle() : baseOption.GetHitbox(8f, 8f));
            }
            return new([.. baseOptions]);
        }

        private static void InsertNewCollider(this Dictionary<string, Collider> baseOptions, string tag, Collider newCollider)
        {
            if (newCollider == null || baseOptions.TryAdd(tag, newCollider))
                return;
            if (baseOptions[tag] is ColliderList list)
                list.Add(newCollider);
            else
                baseOptions[tag] = new ColliderList(baseOptions[tag], newCollider);
        }

        private static Hitbox GetHitbox(this XmlNode source, float defaultWidth, float defaultHeight)
        {
            return new Hitbox(
                source.GetValueOrDefault("width", defaultWidth), source.GetValueOrDefault("height", defaultHeight),
                source.GetValueOrDefault("xOffset", 0f), source.GetValueOrDefault("yOffset", 0f)
            );
        }

        private static Hitbox GetHitbox(this XmlNode source, Vector2 offset)
        {
            float width = source.GetValueOrDefault("width", 0f);
            float height = source.GetValueOrDefault("height", 0f);
            if (width <= 0 || height <= 0)
                return null;
            return new Hitbox(width, height,
                source.GetValueOrDefault("x", 0f) + offset.X, source.GetValueOrDefault("y", 0f) + offset.Y
            );
        }

        private static Circle GetCircle(this XmlNode source, float defaultRadius = 4f)
        {
            return new Circle(
                source.GetValueOrDefault("radius", defaultRadius),
                source.GetValueOrDefault("xOffset", 0f), source.GetValueOrDefault("yOffset", 0f)
            );
        }
        #endregion
        #endregion

        #region Lua Files
        public static Dictionary<string, IBossAction> ReadLuaFiles(
            this BossController controller, params LuaPathReader[] readers)
        {
            Dictionary<string, IBossAction> actions = [];
            foreach (var (path, creator) in readers)
            {
                if (ReadLuaPath(path, false, out ModAsset luaFiles))
                {
                    foreach (ModAsset luaFile in luaFiles.Children)
                    {
                        IBossAction action = creator(luaFile.PathVirtual, controller);
                        if (!actions.TryAdd(luaFile.PathVirtual[(path.Length + 1)..], action))
                            Logger.Error("Bosses Helper", "Two Lua files with the same name were given.");
                    }
                }
            }
            return actions;
        }

        public static T ReadLuaFilePath<T>(string filepath, Func<string, T> parser)
        {
            return parser(ReadLuaPath(filepath, true, out ModAsset saveFile) ? saveFile.PathVirtual : null);
        }

        private static bool ReadLuaPath(string path, bool isFile, out ModAsset asset)
        {
            asset = null;
            if (isFile)
                path = CleanPath(path, ".lua");
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
            return path.EndsWith(extension) ? path[..^extension.Length] : path;
        }
        #endregion
    }
}
