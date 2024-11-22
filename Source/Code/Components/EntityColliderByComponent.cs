using Monocle;
using NLua;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    [Tracked(false)]
    public class EntityColliderByComponent<T, K>(Action<T> onComponentAction, Collider collider = null) : Component(active: true, visible: true) where T : Component where K : Entity
    {
        public Action<T> OnComponentAction = onComponentAction;

        public readonly string componentType = typeof(T).Name;

        public readonly string entityType = typeof(K).Name;

        public Collider Collider = collider;

        private readonly bool isComponentTracked = Engine.Scene.Tracker.IsComponentTracked<T>();

        private readonly bool isEntityTracked = Engine.Scene.Tracker.IsEntityTracked<K>();

        public EntityColliderByComponent(LuaFunction onEntityLua, Collider collider = null)
            : this((comp) => onEntityLua.Call(comp), collider)
        {
        }

        public override void Update()
        {
            base.Update();
            if (OnComponentAction == null) return;

            Collider collider = base.Entity.Collider;
            if (Collider != null)
            {
                base.Entity.Collider = Collider;
            }

            if (isComponentTracked)
            {
                Entity.CollideDoByComponent(OnComponentAction);
            }
            else
            {
                List<K> entities = isEntityTracked
                    ? Scene.Tracker.GetEntities<K>().Cast<K>() as List<K>
                    : Scene.Entities.FindAll<K>();

                foreach (K entity in entities)
                {
                    if (entity.CollideCheck(Entity) && entity.Components.FirstOrDefault((c) => c is T) is T component)
                    {
                        OnComponentAction(component);
                    }
                }
            }

            base.Entity.Collider = collider;
        }
    }
}
