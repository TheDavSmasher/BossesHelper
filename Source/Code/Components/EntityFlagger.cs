using NLua;
using Monocle;
using System.Linq;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class EntityFlagger(string flag, Action<Entity> action, bool stateNeeded, bool resetFlag) : Component(active: true, visible: false)
    {
        public string flag = flag;

        public bool stateNeeded = stateNeeded;

        public bool resetFlag = resetFlag;

        public Action<Entity> action = action;
        
        private Level Level => Entity.SceneAs<Level>();

        public EntityFlagger(string flag, LuaFunction action, bool stateNeeded, bool resetFlag)
            : this(flag, (e) => action.Call(e), stateNeeded, resetFlag)
        {
        }

        public override void Update()
        {
            if (Level.Session.GetFlag(flag) == stateNeeded && Level.Entities.Contains(Entity) && !Level.Entities.ToAdd.Contains(Entity))
            {
                action.Invoke(Entity);
                if (resetFlag)
                {
                    Level.Session.SetFlag(flag, !stateNeeded);
                }
                RemoveSelf();
            }
        }
    }
}
