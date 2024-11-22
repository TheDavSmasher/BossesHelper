using Monocle;
using NLua;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    [Tracked(false)]
    public class EntityCollider<T>(Action<T> onEntityAction, Collider collider = null) : Component(active: true, visible: false) where T : Entity
    {
        public Action<T> OnEntityAction = onEntityAction;

        public readonly string entityType = typeof(T).Name;

        public Collider Collider = collider;

        private readonly bool isTracked = Engine.Scene.Tracker.IsEntityTracked<T>();

        public EntityCollider(LuaFunction onEntityLua, Collider collider = null)
            : this((T entity) => onEntityLua.Call(entity), collider)
        {
        }

        public override void Update()
        {
            base.Update();
            if (OnEntityAction == null) return;

            Collider collider = base.Entity.Collider;
            if (Collider != null)
            {
                base.Entity.Collider = Collider;
            }

            if (isTracked)
            {
                Entity.CollideDo(OnEntityAction);
            }
            else
            {
                foreach (T entity in base.Scene.Entities.FindAll<T>())
                {
                    if (Entity.CollideCheck(entity))
                    {
                        OnEntityAction(entity);
                    }
                }
            }

            base.Entity.Collider = collider;
        }
    }
}
