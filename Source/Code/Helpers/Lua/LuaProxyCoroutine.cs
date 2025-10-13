﻿using NLua;
using System;
using System.Collections;

namespace Celeste.Mod.BossesHelper.Code.Helpers.Lua
{
	public class LuaProxyCoroutine(LuaFunction func)
		: LuaCoroutine(LuaBossHelper.cutsceneHelper.GetProxyTable(func)), IEnumerator
	{
		private object _Current;

		public new object Current => _Current;

		private bool SafeMoveNext()
		{
			try
			{
				return Proxy != null && base.MoveNext();
			}
			catch (Exception e)
			{
				Logger.Error("Bosses Helper", "Failed to resume coroutine");
				Logger.LogDetailed(e);
				return false;
			}
		}

		public new bool MoveNext()
		{
			if (!SafeMoveNext())
				return false;

			_Current = base.Current is double || base.Current is long ?
				Convert.ToSingle(base.Current) : base.Current;

			return true;
		}
	}
}
