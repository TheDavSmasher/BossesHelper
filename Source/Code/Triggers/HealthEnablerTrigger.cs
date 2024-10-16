﻿using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/HealthEnableTrigger")]
    public class HealthEnablerTrigger : Trigger
    {
        private readonly bool enableState;

        public HealthEnablerTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            enableState = data.Bool("enableState");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (enableState)
            {
                SceneAs<Level>().Tracker.GetEntity<HealthSystemManager>()?.EnableHealthSystem();
            }
            else
            {
                SceneAs<Level>().Tracker.GetEntity<HealthSystemManager>()?.DisableHealthSystem();
            }
        }
    }
}
