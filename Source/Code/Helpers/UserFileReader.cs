﻿using System.Collections.Generic;
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
                        new EventCutscene(patternNode.GetValue("file"), patternNode.GetValueOrDefaultNullI("goto"), delegates)
                    );
                    continue;
                }

                int? goTo = patternNode.GetValueOrDefaultNullI("goto");
                Hitbox trigger = GetHitboxFromXml(patternNode, offset);
                int? minCount = patternNode.GetValueOrDefaultNullI("minRepeat");
                int? count = patternNode.GetValueOrDefaultNullI("repeat") ?? minCount ?? (goTo != null ? 0 : null);
                minCount ??= count;
                if (count < minCount)
                    count = minCount;
                
                if (patternNode.LocalName.ToLower().Equals("random"))
                {
                    foreach (XmlNode action in patternNode.ChildNodes)
                    {
                        methodList.AddRange(Enumerable.Repeat(new Method(action.GetValue("file"),
                            action.GetValueOrDefaultNullF("wait")), Math.Max(action.GetValueOrDefaultInt("weight"), 1)));
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
                            methodList.Add(new Method("wait", action.GetValueOrDefaultNullF("time")));
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
        public static void ReadLuaFilesInto(string attacksPath, string eventsPath, out Dictionary<string, IBossAction> actions,
            Player playerRef, BossController controller)
        {
            actions = new();
            string[] paths = { attacksPath, eventsPath };
            for (int i = 0; i < 2; i++)
            {
                string path = paths[i];
                if (!Everest.Content.TryGet(path, out ModAsset luaFiles))
                {
                    Logger.Log(LogLevel.Info, "Bosses Helper", $"No Lua files were found in ${path}.");
                    return;
                }
                foreach (ModAsset luaFile in luaFiles.Children)
                {
                    IBossAction action = i == 0
                        ? new BossAttack(luaFile.PathVirtual, playerRef, controller)
                        : new BossEvent(luaFile.PathVirtual, playerRef, controller);
                    if (!actions.TryAdd(luaFile.PathVirtual.Substring(path.Length + 1), action))
                        Logger.Log(LogLevel.Warn, "Bosses Helper", "Dictionary cannot have duplicate keys.\nTwo Lua files with the same name were given.");
                }
            }
        }

        public static void ReadCustomCodeFileInto(string filepath, Player playerRef, BossController controller)
        {
            string path = CleanPath(filepath, ".lua");
            if (!Everest.Content.TryGet(path, out ModAsset onHitFile))
            {
                Logger.Log(LogLevel.Info, "Bosses Helper", "No Lua file found for custom setup.");
                return;
            }
            controller.Puppet.SetPuppetFunctions(new BossFunctions(onHitFile.PathVirtual, playerRef, controller));
        }

        public static void ReadSavePointFunction(this GlobalSavePoint savePoint, string filepath, Player playerRef)
        {
            string path = CleanPath(filepath, ".lua");
            if (!Everest.Content.TryGet(path, out ModAsset file))
            {
                Logger.Log(LogLevel.Info, "Bosses Helper", "No Lua file found for Save Point.");
                return;
            }
            savePoint.LoadFunction(file.PathVirtual, playerRef);
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
