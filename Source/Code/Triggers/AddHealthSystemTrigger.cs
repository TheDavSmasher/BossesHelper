﻿using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.Entities;
using System.Collections;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/AddHealthSystemTrigger")]
    public class AddHealthSystemTrigger : Trigger
    {
        private readonly EntityID id;

        private readonly EntityData data;

        private readonly bool onlyOnce;

        public AddHealthSystemTrigger(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset)
        {
            this.id = id;
            onlyOnce = data.Bool("onlyOnce", true);
            this.data = data;
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Add(new Coroutine(RemoveAndReadd()));
        }

        private IEnumerator RemoveAndReadd()
        {
            BossesHelperModule.Session.mapHealthSystemManager?.RemoveSelf();
            yield return null;
            new HealthSystemManager(data, Vector2.Zero);
            SceneAs<Level>().Add(BossesHelperModule.Session.mapHealthSystemManager);
            if (onlyOnce)
            {
                SceneAs<Level>().Session.DoNotLoad.Add(id);
            }
            RemoveSelf();
        }
    }
}
