using Monocle;
using NLua;
using System;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Helpers.Lua
{
	internal static class LuaDelegates
	{
		public static Collision ToCollision(this LuaFunction f)
			=> data => f.Call(data);

		public static Ease.Easer ToEaser(this LuaFunction f)
			=> t => (float) f.Call(t)[0];

		public static DashCollision ToDashCollision(this LuaFunction f)
			=> (player, direction) => (DashCollisionResults) f.Call(player, direction)[0];

		#region Lua Structure
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
			public object[] GetLuaData(string filename, LuaTable data, LuaPreparer preparer)
				=> (Base["getLuaData"] as LuaFunction).Call(filename, data, preparer);

			public LuaTable GetProxyTable(LuaFunction func)
				=> (Base["getProxyTable"] as LuaFunction).Call(func).ElementAtOrDefault(0) as LuaTable;

			public LuaPreparers Commands => Base["luaPreparers"] as LuaTable;

			public static implicit operator CutsceneHelper(LuaTable baseTable) => new(baseTable);
		}
		#endregion
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
		public static Func<TOut> ToFunc<TOut>(this LuaFunction function) =>
			() => (TOut)function.Call().FirstOrDefault();

		public static Func<T, TOut> ToFunc<T, TOut>(this LuaFunction function) =>
			(t) => (TOut)function.Call(t).FirstOrDefault();

		public static Func<T1, T2, TOut> ToFunc<T1, T2, TOut>(this LuaFunction function) =>
			(t1, t2) => (TOut)function.Call(t1, t2).FirstOrDefault();

		public static Func<T1, T2, T3, TOut> ToFunc<T1, T2, T3, TOut>(this LuaFunction function) =>
			(t1, t2, t3) => (TOut)function.Call(t1, t2, t3).FirstOrDefault();

		public static Func<T1, T2, T3, T4, TOut> ToFunc<T1, T2, T3, T4, TOut>(this LuaFunction function) =>
			(t1, t2, t3, t4) => (TOut)function.Call(t1, t2, t3, t4).FirstOrDefault();

		public static Func<T1, T2, T3, T4, T5, TOut> ToFunc<T1, T2, T3, T4, T5, TOut>(this LuaFunction function) =>
			(t1, t2, t3, t4, t5) => (TOut)function.Call(t1, t2, t3, t4, t5).FirstOrDefault();

		public static Func<T1, T2, T3, T4, T5, T6, TOut> ToFunc<T1, T2, T3, T4, T5, T6, TOut>(this LuaFunction function) =>
			(t1, t2, t3, t4, t5, t6) => (TOut)function.Call(t1, t2, t3, t4, t5, t6).FirstOrDefault();
		#endregion
	}
}
