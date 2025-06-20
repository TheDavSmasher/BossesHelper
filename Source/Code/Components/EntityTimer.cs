using NLua;
using Monocle;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class EntityTimer(float timer, Action<Entity> action) : StateChecker(action)
    {
        public EntityTimer(float timer, LuaFunction action)
            : this(timer, e => action.Call(e)) { }

        protected override bool StateCheck() => (timer -= Engine.DeltaTime) <= 0;

    }
}
