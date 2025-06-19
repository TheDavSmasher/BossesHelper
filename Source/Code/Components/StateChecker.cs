using NLua;
using Monocle;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public abstract class StateChecker(Action<Entity> action, bool stateNeeded = true, bool removeOnComplete = true)
        : Component(active: true, visible: false)
    {
        protected readonly bool stateNeeded = stateNeeded;

        internal StateChecker(LuaFunction luaAction, bool stateNeeded = true, bool removeOnComplete = true)
            : this(e => luaAction.Call(e), stateNeeded, removeOnComplete) { }

        public override void Update()
        {
            if (StateCheck() == stateNeeded)
            {
                action.Invoke(Entity);
                if (removeOnComplete)
                    RemoveSelf();
            }
        }

        protected abstract bool StateCheck();
    }
}
