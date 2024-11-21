using Monocle;
using NLua;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    [Tracked(false)]
    public class EntityCollider<T>(Action<T> onEntityAction, Collider collider = null) : Component(active: true, visible: false) where T : Entity
    {
        public Action<T> OnEntityAction = onEntityAction;

        public readonly string entityType = typeof(T).Name;

        public Collider Collider = collider;

        public EntityCollider(LuaFunction onEntityLua, Collider collider = null)
            : this((T entity) => onEntityLua.Call(entity), collider)
        {
        }

        public override void Update()
        {
            base.Update();
            List<T> list = base.Scene.Tracker.IsEntityTracked<T>() ?
                base.Scene.Tracker.GetEntities<T>().Cast<T>().ToList() :
                base.Scene.Entities.FindAll<T>();

            foreach (T entity in list)
            {
                if (OnEntityAction != null)
                {
                    Collider collider = base.Entity.Collider;
                    if (Collider != null)
                    {
                        base.Entity.Collider = Collider;
                    }

                    if (Entity.CollideCheck(entity))
                    {
                        OnEntityAction(entity);
                    }

                    base.Entity.Collider = collider;
                }
            }
        }
    }
}
