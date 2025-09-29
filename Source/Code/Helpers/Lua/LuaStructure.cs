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

		#region Function Structure
		public class FunctionWrapper(LuaFunction _base) : LuaWrapper<LuaFunction>(_base);

		public class LuaPreparer(LuaFunction _base) : FunctionWrapper(_base)
		{
			public object[] Call(LuaTable env, LuaFunction loadFunc)
				=> Base.Call(env, loadFunc);

			public LuaFunction Func => Base;

			public static implicit operator LuaPreparer(LuaFunction f) => new(f);
		}
		#endregion

		#region Table Structure
		public class TableWrapper(LuaTable _base) : LuaWrapper<LuaTable>(_base);

		public class LuaPreparers(LuaTable _base) : TableWrapper(_base)
		{
			public LuaPreparer this[LuaCommand key]
				=> Base[key.Name] as LuaFunction;

			public static implicit operator LuaPreparers(LuaTable baseTable) => new(baseTable);
		}

		public class CutsceneHelper(LuaTable _base) : TableWrapper(_base)
		{
			public object[] GetLuaData(string content, LuaTable data, LuaPreparer preparer)
				=> (Base["getLuaData"] as LuaFunction).Call(content, data, preparer.Func);

			public LuaTable GetProxyTable(LuaFunction func)
				=> (Base["getProxyTable"] as LuaFunction).Call(func).ElementAtOrDefault(0) as LuaTable;

			public LuaPreparers Commands => Base["luaPreparers"] as LuaTable;

			public static implicit operator CutsceneHelper(LuaTable baseTable) => new(baseTable);
		}
		#endregion
	}
}
