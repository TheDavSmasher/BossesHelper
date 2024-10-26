using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    [Tracked(false)]
    public class EntityCollider<T> : EntityColliderComponent where T : Entity
    {
        public Action<T> OnEntityAction;

        public EntityCollider(Action<T> onEntity, Collider collider = null)
            : base(collider)
        {
            OnEntityAction = onEntity;
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
