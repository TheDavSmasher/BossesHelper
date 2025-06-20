﻿using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/PlayerHealthBarVisibleTrigger")]
    public class PlayerHealthBarVisibleTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
    {
        private readonly bool state = data.Bool("visible");

        private readonly bool onlyOnce = data.Bool("onlyOnce");

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (Scene.GetEntity() is PlayerHealthBar bar)
            {
                bar.Visible = state;
                if (onlyOnce)
                {
                    RemoveSelf();
                }
            }
        }
    }
}
