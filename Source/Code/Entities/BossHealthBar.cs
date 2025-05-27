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
        private readonly Vector2 BarPosition;

        private readonly Vector2 BarScale;

        private Func<int> BossHealth;

        private enum BarTypes
        {
            BarLeft = -1,
            BarRight,
            BarCentered,
            Icons,
            Countdown
        }

        private readonly BarTypes barType;

        private HealthDisplay barEntity;

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
            AddTag(Tags.HUD);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = SceneAs<Level>();
            if (level.Tracker.GetNearestComponent<BossHealthTracker>(entityData.Nodes[0]) is not { } component)
            {
                RemoveSelf();
                return;
            }
            BossHealth = component.Health;
            Color baseColor = entityData.HexColor("baseColor", Color.White);
            level.Add(barEntity = barType switch
            {
                BarTypes.Icons => new HealthIconList(entityData, BarPosition, BarScale, BossHealth),
                BarTypes.Countdown => new HealthNumber(BarPosition, BarScale, BossHealth, baseColor),
                _ => new HealthBar(BarPosition, BarScale, BossHealth, baseColor, (Alignment) barType)
            });
            Visible = entityData.Bool("startVisible");
        }

        public override void Update()
        {
            base.Update();
            if (barEntity is HealthIconList healthIcons)
            {
                healthIcons.DecreaseHealth(healthIcons.Count - BossHealth());
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            barEntity.RemoveSelf();
        }
    }
}
