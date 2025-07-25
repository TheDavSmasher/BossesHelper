﻿using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    public abstract class SingleUseTrigger(EntityData data, Vector2 offset, EntityID id, bool permanent = false, bool removeOnUse = false)
            : Trigger(data, offset)
    {
        protected readonly EntityData entityData = data;

        protected readonly EntityID ID = id;

        protected void ConsumeAfter(Action change)
        {
            change();
            if (removeOnUse)
            {
                RemoveSelf();
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (permanent)
            {
                SceneAs<Level>().Session.DoNotLoad.Add(ID);
            }
        }
    }
}
