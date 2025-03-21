using NLua;
using Monocle;
using System;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class EntityChecker(Func<bool> checker, Action<Entity> action, bool stateNeeded, bool removeOnComplete = true)
        : Component(active: true, visible: false)
    {
        public Func<bool> checker = checker;

        public bool stateNeeded = stateNeeded;

        public Action<Entity> action = action;

        private readonly bool removeOnComplete = removeOnComplete;

        public EntityChecker(LuaFunction checker, LuaFunction action, bool stateNeeded)
            : this(() => (bool) checker.Call().FirstOrDefault(), (e) => action.Call(e), stateNeeded) { }

        public override void Update()
        {
            if (checker() == stateNeeded)
            {
                action.Invoke(Entity);
                if (removeOnComplete)
                    RemoveSelf();
            }
        }
    }
}
