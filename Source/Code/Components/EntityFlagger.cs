using NLua;
using Monocle;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class EntityFlagger(string flag, Action<Entity> action, bool stateNeeded, bool resetFlag) : Component(active: true, visible: false)
    {
        public string flag = flag;

        public bool stateNeeded = stateNeeded;

        public bool resetFlag = resetFlag;

        public Action<Entity> action = action;

        public EntityFlagger(string flag, LuaFunction action, bool stateNeeded, bool resetFlag)
            : this(flag, (e) => action.Call(e), stateNeeded, resetFlag)
        {
        }

        public override void Update()
        {
            if (SceneAs<Level>().Session.GetFlag(flag) == stateNeeded)
            {
                action.Invoke(Entity);
                if (resetFlag)
                {
                    SceneAs<Level>().Session.SetFlag(flag, !stateNeeded);
                }
                RemoveSelf();
            }
        }
    }
}
