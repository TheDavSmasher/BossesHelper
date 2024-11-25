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

        public EntityCollider(LuaFunction onEntityLua, Collider collider = null)
            : this((T entity) => onEntityLua.Call(entity), collider)
        {
        }

        public override void Added(Entity entity)
        {
            if (!Engine.Scene.Tracker.IsEntityTracked<T>())
            {
                BossesHelperModule.AddEntityToTracker(typeof(T));
            }
            base.Added(entity);
        }

        public override void Update()
        {
            if (OnEntityAction == null) return;

            Collider collider = base.Entity.Collider;
            if (Collider != null)
            {
                base.Entity.Collider = Collider;
            }

            Entity.CollideDo(OnEntityAction);

            base.Entity.Collider = collider;
        }
    }
}
