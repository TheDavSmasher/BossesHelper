using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
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
        public static List<BossPattern> ReadPatternFileInto(string filepath, Vector2 offset,
            Dictionary<string, IBossAction> actions, ControllerDelegates delegates)
        {
            List<BossPattern> targetOut = [];

            ReadXMLFile(filepath, "Failed to find any Pattern file.", "Patterns", patternNode =>
            {
                List<Method> methodList = new();
                if (patternNode.LocalName.ToLower().Equals("event"))
                {
                    targetOut.Add(new EventCutscene(
                        patternNode.GetValue("file"), patternNode.GetValueOrDefault<int>("goto"), actions, delegates
                    ));
                    return;
                }

                int? goTo = patternNode.GetValueOrDefault<int>("goto");
                Hitbox trigger = patternNode.GetHitbox(offset);
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
                    targetOut.Add(new RandomPattern(
                        methodList.ToArray(), trigger, minCount, count, goTo, actions, delegates
                    ));
                    return;
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
                    methodList.ToArray(), preLoopList?.ToArray() ?? [], trigger, minCount, count, goTo, actions, delegates)
                );
            });
            return targetOut;
        }

        public static Dictionary<BossPuppet.ColliderOption, Dictionary<string, Collider>> ReadMetadataFileInto(string filepath)
        {
            Dictionary<BossPuppet.ColliderOption, Dictionary<string, Collider>> dataHolder = [];
            foreach (BossPuppet.ColliderOption option in Enum.GetValues(typeof(BossPuppet.ColliderOption)))
            {
                dataHolder.Add(option, []);
            }

            ReadXMLFile(filepath, "No Hitbox Metadata file found. Boss will use all default hitboxes.", "HitboxMetadata", hitboxNode =>
            {
                BossPuppet.ColliderOption option = Enum.Parse<BossPuppet.ColliderOption>(hitboxNode.LocalName, true);
                dataHolder[option].InsertNewCollider(hitboxNode.GetValue("tag"), option switch
                {
                    BossPuppet.ColliderOption.Hitboxes or BossPuppet.ColliderOption.Hurtboxes => hitboxNode.GetAllColliders(),
                    BossPuppet.ColliderOption.Bouncebox => hitboxNode.GetHitbox(8f, 6f),
                    BossPuppet.ColliderOption.Target => hitboxNode.GetCircle(),
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
            XmlDocument document = new XmlDocument();
            document.Load(xml.Stream);
            foreach (XmlNode xmlNode in document.SelectSingleNode(node).ChildNodes)
            {
                if (xmlNode.NodeType == XmlNodeType.Comment) continue;

                nodeReader(xmlNode);
            }
        }

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

        private static ColliderList GetAllColliders(this XmlNode source)
        {
            List<Collider> baseOptions = new();
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
            baseOptions[tag] = new ColliderList(
                [.. baseOptions[tag] is ColliderList list ? list.colliders : [baseOptions[tag]], newCollider]);
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
            controller.Puppet.BossFunctions = new(setupFile.PathVirtual, playerRef, controller);
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
