using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    [CustomEntity("BossesHelper/BossHealthBar")]
    public class BossHealthBar : Entity
    {
        private Vector2 BarPosition;

        private Vector2 BarScale;

        private Func<int> BossHealth;

        private enum BarTypes
        {
            BarLeft = -1,
            BarRight,
            BarCentered,
            Icons,
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
            Position = data.Position;
            BarPosition = new Vector2(data.Float("healthBarX"), data.Float("healthBarY"));
            BarScale = new Vector2(data.Float("healthScaleX", 1f), data.Float("healthScaleY", 1f));
            barType = data.Enum<BarTypes>("barType", BarTypes.BarLeft);
            Tag = Tags.HUD;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = SceneAs<Level>();
            BossHealthTracker component = SceneAs<Level>().Tracker.GetNearestComponent<BossHealthTracker>(entityData.Nodes[0]);
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
                    level.Add(barEntity = new HealthIconList(entityData, BossHealth.Invoke(), BarPosition, BarScale));
                    break;
                case BarTypes.Countdown:
                    level.Add(barEntity = new HealthNumber(BarPosition, BarScale, BossHealth, baseColor));
                    break;
                default:
                    HealthBar.Alignment barAnchor = barType == BarTypes.BarCentered ? HealthBar.Alignment.Center : barType == BarTypes.BarLeft ? HealthBar.Alignment.Left : HealthBar.Alignment.Right;
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
