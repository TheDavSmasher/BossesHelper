﻿using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.Entities;
using System.Collections;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/AddHealthSystemTrigger")]
    public class AddHealthSystemTrigger(EntityData data, Vector2 offset, EntityID id) : Trigger(data, offset)
    {
        private readonly EntityID id = id;

        private readonly EntityData data = data;

        private readonly bool onlyOnce = data.Bool("onlyOnce", true);

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Add(new Coroutine(RemoveAndReadd()));
        }

        private IEnumerator RemoveAndReadd()
        {
            Scene.Tracker.GetEntity<HealthSystemManager>()?.RemoveSelf();
            yield return null;
            SceneAs<Level>().Add(new HealthSystemManager(data, Vector2.Zero));
            if (onlyOnce)
            {
                SceneAs<Level>().Session.DoNotLoad.Add(id);
            }
            RemoveSelf();
        }
    }
}
