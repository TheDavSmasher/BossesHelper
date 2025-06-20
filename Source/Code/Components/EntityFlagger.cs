using NLua;
using Monocle;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class EntityFlagger(string flag, Action<Entity> action, bool stateNeeded = true, bool resetFlag = false)
        : StateChecker(action, stateNeeded)
    {
        public EntityFlagger(string flag, LuaFunction action, bool stateNeeded = true, bool resetFlag = false)
            : this(flag, e => action.Call(e), stateNeeded, resetFlag) { }

        protected override bool StateCheck()
        {
            Session session = SceneAs<Level>().Session;
            bool result = session.GetFlag(flag);
            if (result && resetFlag)
            {
                session.SetFlag(flag, !state);
            }
            return result;
        }
    }
}
