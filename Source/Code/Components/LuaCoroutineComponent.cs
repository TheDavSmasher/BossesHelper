using Celeste.Mod.BossesHelper.Code.Helpers.Lua;
using Monocle;
using NLua;

namespace Celeste.Mod.BossesHelper.Code.Components
{
	public class LuaCoroutineComponent(LuaCoroutine luaCoroutine) : Coroutine(luaCoroutine)
	{
		public LuaCoroutineComponent(LuaFunction func)
			: this(new LuaProxyCoroutine(func)) { }
	}
}
