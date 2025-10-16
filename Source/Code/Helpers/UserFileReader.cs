using Celeste.Mod.BossesHelper.Code.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using static Celeste.Mod.BossesHelper.Code.Entities.BossPuppet;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
	internal static class UserFileReader
	{
		#region XML Files

		#region XML Reading

		#region Patterns
		public static List<BossPattern> ReadPatternFile(string filepath, BossController controller)
		{
			if (GetXMLDocument(filepath) is not XmlDocument xml)
				throw new Exception("Failed to find any Pattern file.");

			List<BossPattern> result = [];
			foreach (XmlNode node in xml.GetChildNodes("Patterns"))
			{
				if (!Enum.TryParse(node.LocalName, true, out PatternType type))
				{
					Logger.Warn("Bosses Helper", $"{node.LocalName} is not an accepted Patterns node type.");
					continue;
				}

				result.Add(node.ParseNewPattern(controller));
			}
			return result;
		}

		private static BossPattern ParseNewPattern(this XmlNode patternNode, BossController controller)
		{
			string nodeType = patternNode.LocalName.ToLower();
			List<Method> methodList = [];

			string patternName = patternNode.GetValue("name");
			string goTo = patternNode.GetValue("goto");
			if (nodeType.Equals("event"))
			{
				return new EventCutscene(patternName, patternNode.GetMethod(true), goTo, controller);
			}

			Hitbox trigger = patternNode.GetTriggerHitbox(controller);
			int? minCount = patternNode.GetValueOrDefault<int>("minRepeat");
			int? count = patternNode.GetValueOrDefault<int>("repeat") ?? minCount ?? (goTo is null ? null : 0);
			minCount ??= count;
			if (count < minCount)
				count = minCount;

			if (nodeType.Equals("random"))
			{
				foreach (XmlNode action in patternNode.GetChildNodes())
				{
					methodList.AddRange(Enumerable.Repeat(
						action.GetMethod(true, true),
						Math.Max(action.GetValueOrDefault("weight", 0), 1)
					));
				}
				return new RandomPattern(patternName, methodList, trigger, minCount, count, goTo, controller);
			}

			List<Method> preLoopList = [];
			foreach (XmlNode action in patternNode.GetChildNodes())
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

			return new SequentialPattern(patternName, methodList, preLoopList, trigger, minCount, count, goTo, controller);
		}

		private static Method GetMethod(this XmlNode source, bool isFile, bool hasTime = false)
		{
			return new Method(
				isFile ? source.GetValue("file") : "wait",
				source.GetValueOrDefault<float>(!isFile || hasTime ? "time" : "")
			);
		}

		private static Hitbox GetTriggerHitbox(this XmlNode source, BossController controller)
		{
			float width = source.GetValueOrDefault("width", 0f);
			float height = source.GetValueOrDefault("height", 0f);
			if (width <= 0 || height <= 0)
				return null;

			Vector2 offset = controller.SceneAs<Level>().LevelOffset;
			return new Hitbox(width, height,
				source.GetValueOrDefault("x", 0f) + offset.X, source.GetValueOrDefault("y", 0f) + offset.Y
			);
		}
		#endregion

		#region Hitbox Metadata
		public static HitboxMetadata ReadMetadataFile(string filepath)
		{
			HitboxMetadata metadata = new();

			if (GetXMLDocument(filepath) is not XmlDocument xml)
			{
				Logger.Warn("Bosses Helper", "No Hitbox Metadata file found. Boss will use all default hitboxes.");
				return metadata;
			}

			foreach (XmlNode hitboxNode in xml.GetChildNodes("HitboxMetadata"))
			{
				if (!Enum.TryParse(hitboxNode.LocalName, true, out ColliderOption option))
				{
					Logger.Warn("Bosses Helper", $"{hitboxNode.LocalName} is not an accepted HitboxMetadata node type.");
					continue;
				}	

				metadata[option].InsertNewCollider(hitboxNode.GetValue("tag", "main"), option switch
				{
					ColliderOption.Hitboxes or ColliderOption.Hurtboxes or ColliderOption.KillColliders => hitboxNode.GetAllColliders(),
					ColliderOption.Bouncebox => hitboxNode.GetHitbox(6f),
					ColliderOption.Target => hitboxNode.GetCircle(),
					_ => null
				});
			};
			return metadata;
		}

		private static Hitbox GetHitbox(this XmlNode source, float defaultHeight)
		{
			return new Hitbox(
				source.GetValueOrDefault("width", 8f), source.GetValueOrDefault("height", defaultHeight),
				source.GetValueOrDefault("xOffset", 0f), source.GetValueOrDefault("yOffset", 0f)
			);
		}

		private static Circle GetCircle(this XmlNode source, float defaultRadius = 4f)
		{
			return new Circle(
				source.GetValueOrDefault("radius", defaultRadius),
				source.GetValueOrDefault("xOffset", 0f), source.GetValueOrDefault("yOffset", 0f)
			);
		}

		private static ColliderList GetAllColliders(this XmlNode source)
		{
			if (!source.HasChildNodes)
				return null;

			return new([.. source.GetChildNodes().Cast<XmlElement>().Select<XmlElement, Collider>(
				opt => opt.LocalName.ToLower().Equals("circle") ? opt.GetCircle() : opt.GetHitbox(8f)
			)]);
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
		#endregion

		#region Helper Functions
		private static XmlDocument GetXMLDocument(string filepath)
		{
			if (!Everest.Content.TryGet(CleanPath(filepath, ".xml"), out ModAsset asset))
				return null;

			XmlDocument xml = new();
			xml.Load(asset.Stream);
			return xml;
		}

		private static IEnumerable<XmlNode> GetChildNodes(this XmlNode source, string xpath = null)
		{
			if (!string.IsNullOrWhiteSpace(xpath))
				source = source.SelectSingleNode(xpath);

			foreach (XmlNode xmlNode in source.ChildNodes)
			{
				if (xmlNode.NodeType == XmlNodeType.Comment) continue;

				yield return xmlNode;
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

		private static string GetValue(this XmlNode source, string tag, string @default = null)
		{
			return source.Attributes[tag]?.Value ?? @default;
		}
		#endregion

		#endregion

		#endregion

		#region Lua Files
		public record LuaPathReader(string Path, Func<string, BossController, IBossAction> Creator);

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
			if (isFile)
				path = CleanPath(path, ".lua");
			if (!Everest.Content.TryGet(path, out asset))
			{
				Logger.Log(LogLevel.Info, "Bosses Helper", $"No Lua files were found in ${path}.");
				return false;
			}
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
