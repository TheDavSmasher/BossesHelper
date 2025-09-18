using Celeste.Mod.BossesHelper.Code.Entities;
using Monocle;
using NLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
	public interface ILuaLoader
	{
		LuaCommand Command { get; }

		LuaTableItem[] Values => [];
	}

	public class LuaWarmer : ILuaLoader
	{
		public LuaCommand Command => ("getCutsceneData", 2);

		public void WarmUp()
		{
			Logger.Log("Bosses Helper", "Warming up Lua cutscenes");
			_ = this.LoadFile("Assets/LuaBossHelper/warmup_cutscene")
				.Select(func => func.Call());
		}
	}

	public class LuaException(string message) : Exception(message) { }

	internal static class LuaBossHelper
	{
		private static readonly string LuaAssetsPath = $"{BossesHelperModule.Instance.Metadata.Name}:/Assets/LuaBossHelper";

		public static readonly string HelperFunctions = GetFileContent($"{LuaAssetsPath}/helper_functions");

		public static readonly LuaTable cutsceneHelper = Everest.LuaLoader.Require($"{LuaAssetsPath}/cutscene_helper") as LuaTable;

		public static string GetFileContent(string path)
		{
			if (Everest.Content.Get(path)?.Stream is not Stream stream)
				return null;
			using StreamReader streamReader = new(stream);
			return streamReader.ReadToEnd();
		}

		public static LuaTable DictionaryToLuaTable(IDictionary<object, object> dict)
		{
			LuaTable luaTable = GetEmptyTable();
			foreach (KeyValuePair<object, object> item in dict)
			{
				luaTable[item.Key] = item.Value;
			}
			return luaTable;
		}

		public static LuaTable ListToLuaTable(IList list)
		{
			LuaTable luaTable = GetEmptyTable();
			int num = 1;
			foreach (object item in list)
			{
				luaTable[num++] = item;
			}
			return luaTable;
		}

		private static bool SafeMoveNext(this LuaCoroutine enumerator)
		{
			try
			{
				return enumerator.MoveNext();
			}
			catch (Exception e)
			{
				Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to resume coroutine");
				Logger.LogDetailed(e);
				return false;
			}
		}

		public static IEnumerator ToIEnumerator(this LuaFunction func)
		{
			LuaCoroutine routine = (cutsceneHelper["setFuncAsCoroutine"] as LuaFunction)
				.Call(func).ElementAtOrDefault(0) as LuaCoroutine;
			while (routine != null && SafeMoveNext(routine))
			{
				if (routine.Current is double || routine.Current is long)
				{
					yield return Convert.ToSingle(routine.Current);
				}
				else
				{
					yield return routine.Current;
				}
			}
			yield return null;
		}

		public static void AddAsCoroutine(this LuaFunction function, Entity target)
		{
			function.ToIEnumerator().Coroutine(target);
		}

		public static LuaFunction[] LoadFile<T>(this T self, string filename) where T : ILuaLoader
		{
			Dictionary<object, object> passedVals = self.Values.ToDictionary();
			LuaFunction[] funcs = null;
			if (!string.IsNullOrEmpty(filename))
			{
				passedVals.Add("modMetaData", BossesHelperModule.Instance.Metadata);
				try
				{
					if ((cutsceneHelper[self.Command.Name] as LuaFunction)
						.Call(filename, DictionaryToLuaTable(passedVals)) is object[] array)
					{
						funcs = [.. array.Skip(1).Cast<LuaFunction>()];
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
			Array.Resize(ref funcs, self.Command.Count);
			return funcs;
		}

		public static ColliderList GetColliderListFromLuaTable(LuaTable luaTable)
		{
			return new ColliderList([.. luaTable.Values.OfType<Collider>()]);
		}

		public static IEnumerator Say(string dialog, LuaTable luaEvents)
		{
			return Textbox.Say(dialog, [.. luaEvents.Values.OfType<LuaFunction>()
				.Select<LuaFunction, Func<IEnumerator>>(luaEv => luaEv.ToIEnumerator)]);
		}

		public static LuaTable GetEmptyTable()
		{
			return Everest.LuaLoader.Context.DoString("return {}").FirstOrDefault() as LuaTable;
		}

		public static void DoMethodAfterDelay(LuaFunction func, float delay)
		{
			Alarm.Create(Alarm.AlarmMode.Oneshot, () => func.Call(), delay, true);
		}

		public static void AddConstantBackgroundCoroutine(BossPuppet puppet, LuaFunction func)
		{
			func.AddAsCoroutine(puppet);
		}
	}
}
