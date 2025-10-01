global using LuaCommand = (string Name, int Count);
global using LuaTableItem = (object Key, object Value);
using Monocle;
using NLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Helpers.Lua
{
	using LuaData = (LuaTable Env, LuaFunction[] Funcs);

	public interface ILuaLoader
	{
		LuaCommand Command { get; }

		List<LuaTableItem> Values { get; }

		Scene Scene { get; }
	}

	#region Lua Structure
	public class CutsceneHelper(string filepath)
	{
		private readonly LuaTable Base = Everest.LuaLoader.Require(filepath) as LuaTable;

		public LuaData GetLuaData(string content, LuaTable data, string preparer)
		{
			object[] luaData = (Base["getLuaData"] as LuaFunction).Call(content, data, preparer);
			return (luaData[0] as LuaTable, [.. luaData.Skip(1).OfType<LuaFunction>()]);
		}

		public LuaTable GetProxyTable(LuaFunction func)
			=> (Base["getProxyTable"] as LuaFunction).Call(func).ElementAtOrDefault(0) as LuaTable;
	}
	#endregion

	internal static class LuaBossHelper
	{
		private static readonly string FilesPath = "Assets/LuaBossHelper";

		private static readonly string LuaAssetsPath = $"{BossesHelperModule.Instance.Metadata.Name}:/{FilesPath}";

		public static readonly string HelperFunctions = GetFileContent($"{LuaAssetsPath}/helper_functions");

		public static readonly CutsceneHelper cutsceneHelper = new($"{LuaAssetsPath}/cutscene_helper");

		public static void WarmUp()
		{
			Logger.Log("Bosses Helper", "Warming up Lua cutscenes");
			foreach (var func in LoadCommand($"{FilesPath}/warmup_cutscene", ("getCutsceneData", 2)))
				func.Call();
		}

		public static string GetFileContent(string path)
		{
			if (Everest.Content.Get(path)?.Stream is not Stream stream)
				return null;
			using StreamReader streamReader = new(stream);
			return streamReader.ReadToEnd();
		}

		public static LuaTable ToLuaTable(this IDictionary<object, object> dict)
		{
			LuaTable luaTable = GetEmptyTable();
			foreach (KeyValuePair<object, object> item in dict)
			{
				luaTable[item.Key] = item.Value;
			}
			return luaTable;
		}

		public static LuaTable ToLuaTable(this IList list)
		{
			LuaTable luaTable = GetEmptyTable();
			int num = 1;
			foreach (object item in list)
			{
				luaTable[num++] = item;
			}
			return luaTable;
		}

		public static LuaFunction[] LoadFile<T>(this T self, string filename) where T : ILuaLoader
		{
			Dictionary<object, object> passedVals = self.Values.ToDictionary();
			passedVals.Add("player", self.Scene.GetPlayer());
			return LoadCommand(filename, self.Command, passedVals);
		}

		public static LuaFunction[] LoadCommand(string filename, LuaCommand command, Dictionary<object, object> passedVals = null)
		{
			passedVals ??= [];
			LuaFunction[] funcs = null;
			if (!string.IsNullOrEmpty(filename))
			{
				passedVals.Add("modMetaData", BossesHelperModule.Instance.Metadata);
				try
				{
					if (GetFileContent(filename) is string content && !content.IsWhiteSpace() &&
						cutsceneHelper.GetLuaData(content, passedVals.ToLuaTable(), command.Name) is LuaData data)
					{
						funcs = data.Funcs;
					}
					else
					{
						Logger.Log("Bosses Helper", "Failed to load Lua Cutscene, target file does not exist: \"" + filename + "\"");
					}
				}
				catch (Exception e)
				{
					Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to execute cutscene in C#: {e}");
				}
			}
			Array.Resize(ref funcs, command.Count);
			return funcs;
		}

		public static LuaTable GetEmptyTable()
		{
			return Everest.LuaLoader.Context.DoString("return {}").FirstOrDefault() as LuaTable;
		}
	}
}
