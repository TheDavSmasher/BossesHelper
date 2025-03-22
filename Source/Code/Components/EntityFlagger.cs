using NLua;
using Monocle;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class EntityFlagger(string flag, Action<Entity> action, bool stateNeeded, bool resetFlag)
        : StateChecker(action, stateNeeded)
    {
        private readonly string flag = flag;

        private readonly bool resetFlag = resetFlag;

        public EntityFlagger(string flag, LuaFunction action, bool stateNeeded, bool resetFlag)
            : this(flag, (e) => action.Call(e), stateNeeded, resetFlag) { }

        protected override bool StateCheck()
        {
            bool result = SceneAs<Level>().Session.GetFlag(flag);
            if (result && resetFlag)
            {
                SceneAs<Level>().Session.SetFlag(flag, !stateNeeded);
            }
            return result;
        }
    }
}
