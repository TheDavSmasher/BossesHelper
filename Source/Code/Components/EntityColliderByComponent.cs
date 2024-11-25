using Monocle;
using NLua;
using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    [Tracked(false)]
    public class EntityColliderByComponent<T>(Action<T> onComponentAction, Collider collider = null) : Component(active: true, visible: true) where T : Component
    {
        public Action<T> OnComponentAction = onComponentAction;

        public readonly string componentType = typeof(T).Name;

        public Collider Collider = collider;

        public EntityColliderByComponent(LuaFunction onEntityLua, Collider collider = null)
            : this((comp) => onEntityLua.Call(comp), collider)
        {
        }

        public override void Added(Entity entity)
        {
            if (!Engine.Scene.Tracker.IsComponentTracked<T>())
            {
                BossesHelperModule.AddComponentToTracker(typeof(T));
            }
            base.Added(entity);
        }

        public override void Update()
        {
            if (OnComponentAction == null) return;

            Collider collider = base.Entity.Collider;
            if (Collider != null)
            {
                base.Entity.Collider = Collider;
            }
            
            Entity.CollideDoByComponent(OnComponentAction);

            base.Entity.Collider = collider;
        }

        public override void DebugRender(Camera camera)
        {
            if (Collider != null)
            {
                Collider collider = base.Entity.Collider;
                base.Entity.Collider = Collider;
                Collider.Render(camera, Color.HotPink);
                base.Entity.Collider = collider;
            }
        }
    }
}
