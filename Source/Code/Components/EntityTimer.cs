using NLua;
using Monocle;
using System;
using Celeste.Mod.BossesHelper.Code.Helpers;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class EntityTimer(float timer, Action<Entity> action) : StateChecker(action)
    {
        public EntityTimer(float timer, LuaFunction action)
            : this(timer, action.ToAction<Entity>()) { }

        protected override bool StateCheck() => (timer -= Engine.DeltaTime) <= 0;

    }
}
