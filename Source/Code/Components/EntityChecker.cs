using NLua;
using Monocle;
using System;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class EntityChecker(Func<bool> checker, Action<Entity> action, bool stateNeeded = true, bool removeOnComplete = true)
        : StateChecker(action, stateNeeded, removeOnComplete)
    {
        private readonly Func<bool> checker = checker;

        public EntityChecker(LuaFunction checker, LuaFunction action, bool stateNeeded = true, bool removeOnComplete = true)
            : this(() => (bool) checker.Call().FirstOrDefault(), (e) => action.Call(e), stateNeeded, removeOnComplete) { }

        protected override bool StateCheck() => checker();
    }
}
