using NLua;
using Monocle;
using System.Linq;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class EntityTimer(float timer, Action<Entity> action) : Component(active: true, visible: false)
    {
        public float Timer { get; set; } = timer;

        public Action<Entity> action = action;

        private Level Level => Entity.SceneAs<Level>();

        public EntityTimer(float timer, LuaFunction action)
            : this(timer, (e) => action.Call(e))
        {
        }

        public override void Update()
        {
            if (Level.Entities.Contains(Entity) && !Level.Entities.ToAdd.Contains(Entity) && Timer <= 0)
            {
                action.Invoke(Entity);
                RemoveSelf();
            }
            else
            {
                Timer -= Engine.DeltaTime;
            }
        }
    }
}
