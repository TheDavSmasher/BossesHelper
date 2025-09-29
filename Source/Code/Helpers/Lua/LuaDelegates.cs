using NLua;
using System;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Helpers.Lua
{
	internal static class LuaDelegates
	{
		public delegate void ParamAction(params object[] args);

		public static ParamAction LuaFunctionToAction(LuaFunction func)
		{
			return args => func.Call(args);
		}

		public delegate object ParamFunc(params object[] args);

		public static ParamFunc LuaFunctionToFunc(LuaFunction func)
		{
			return args => func.Call(args).First();
		}

		public abstract class LuaWrapper<T>(T _base) where T : LuaBase
		{
			protected readonly T Base = _base;
		}

		#region Table Structure
		public class LuaPreparers(LuaTable _base) : LuaWrapper<LuaTable>(_base)
		{
			public LuaFunction this[string key]
				=> Base[key] as LuaFunction;

			public static implicit operator LuaPreparers(LuaTable baseTable) => new(baseTable);
		}

		public class CutsceneHelper(LuaTable _base) : LuaWrapper<LuaTable>(_base)
		{
			public object[] GetLuaData(string filename, LuaTable data, LuaFunction preparer)
				=> (Base["getLuaData"] as LuaFunction).Call(filename, data, preparer);

			public LuaTable GetProxyTable(LuaFunction func)
				=> (Base["getProxyTable"] as LuaFunction).Call(func).ElementAtOrDefault(0) as LuaTable;

			public LuaPreparers Commands => Base["luaPreparers"] as LuaTable;

			public static implicit operator CutsceneHelper(LuaTable baseTable) => new(baseTable);
		}
		#endregion

		#region To Action
		public static Action ToAction(this LuaFunction function) =>
			() => function.Call();

		public static Action<T> ToAction<T>(this LuaFunction function) =>
			(t) => function.Call(t);

		public static Action<T1, T2> ToAction<T1, T2>(this LuaFunction function) =>
			(t1, t2) => function.Call(t1, t2);

		public static Action<T1, T2, T3> ToAction<T1, T2, T3>(this LuaFunction function) =>
			(t1, t2, t3) => function.Call(t1, t2, t3);

		public static Action<T1, T2, T3, T4> ToAction<T1, T2, T3, T4>(this LuaFunction function) =>
			(t1, t2, t3, t4) => function.Call(t1, t2, t3, t4);

		public static Action<T1, T2, T3, T4, T5> ToAction<T1, T2, T3, T4, T5>(this LuaFunction function) =>
			(t1, t2, t3, t4, t5) => function.Call(t1, t2, t3, t4, t5);

		public static Action<T1, T2, T3, T4, T5, T6> ToAction<T1, T2, T3, T4, T5, T6>(this LuaFunction function) =>
			(t1, t2, t3, t4, t5, t6) => function.Call(t1, t2, t3, t4, t5, t6);
		#endregion

		#region To Func
		public static Func<T> ToFunc<T>(this LuaFunction function) => () => (T)function.Call().FirstOrDefault();
		#endregion
	}
}
