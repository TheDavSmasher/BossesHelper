using NLua;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Helpers.Lua
{
	public static class LuaStructure
	{
		public abstract class LuaWrapper<T>(T _base) where T : LuaBase
		{
			protected readonly T Base = _base;
		}

		#region Table Structure
		public class TableWrapper(LuaTable _base) : LuaWrapper<LuaTable>(_base);

		public class CutsceneHelper(LuaTable _base) : TableWrapper(_base)
		{
			public object[] GetLuaData(string content, LuaTable data, string preparer)
				=> (Base["getLuaData"] as LuaFunction).Call(content, data, preparer);

			public LuaTable GetProxyTable(LuaFunction func)
				=> (Base["getProxyTable"] as LuaFunction).Call(func).ElementAtOrDefault(0) as LuaTable;

			public static implicit operator CutsceneHelper(LuaTable baseTable) => new(baseTable);
		}
		#endregion
	}
}
