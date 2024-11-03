﻿using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using static Celeste.Mod.BossesHelper.Code.Helpers.HealthBarUtils;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [CustomEntity("BossesHelper/BossHealthBar")]
    public class BossHealthBar : Entity
    {
        private Vector2 BarPosition;

        private Vector2 BarScale;

        private Func<int> BossHealth;

        private enum BarTypes
        {
            Icons,
            BarLeft,
            BarRight,
            BarCentered,
            Countdown
        }

        private Level level;

        private readonly BarTypes barType;

        private Entity barEntity;

        private readonly EntityData entityData;

        public new bool Visible
        {
            get
            {
                return barEntity.Visible;
            }
            set
            {
                barEntity.Visible = value;
            }
        }

        public BossHealthBar(EntityData data, Vector2 _)
        {
            this.entityData = data;
            BarPosition = (Position = new Vector2(data.Float("healthBarX"), data.Float("healthBarY")));
            BarScale = new Vector2(data.Float("healthScaleX", 1f), data.Float("healthScaleY", 1f));
            barType = data.Enum<BarTypes>("barType", BarTypes.BarLeft);
            Tag = Tags.HUD;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = SceneAs<Level>();
            BossHealthComponent component = SceneAs<Level>().Tracker.GetNearestComponent<BossHealthComponent>(entityData.Nodes[0]);
            if (component == null)
            {
                RemoveSelf();
                return;
            }
            BossHealth = component.Health;
            Color baseColor = entityData.HexColor("baseColor", Color.White);
            switch (barType)
            {
                case BarTypes.Icons:
                    level.Add(barEntity = new HealthIconList(entityData, BossHealth.Invoke(), BarScale));
                    break;
                case BarTypes.Countdown:
                    level.Add(barEntity = new HealthNumber(BarPosition, BarScale, BossHealth, baseColor));
                    break;
                default:
                    int barAnchor = barType == BarTypes.BarCentered ? 0 : barType == BarTypes.BarLeft ? -1 : 1;
                    level.Add(barEntity = new HealthBar(BarPosition, BarScale, BossHealth, baseColor, barAnchor));
                    break;
            }
            Visible = entityData.Bool("startVisible");
        }

        public override void Update()
        {
            base.Update();
            if (barType == BarTypes.Icons)
            {
                HealthIconList healthIcons = (HealthIconList) barEntity;
                for (int i = 0; i < (healthIcons.Count - BossHealth()); i++)
                {
                    healthIcons.DecreaseHealth();
                }
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            barEntity.RemoveSelf();
        }
    }
}
