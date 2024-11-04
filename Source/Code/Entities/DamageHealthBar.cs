using Monocle;
using static Celeste.Mod.BossesHelper.Code.Helpers.HealthBarUtils;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    public class DamageHealthBar : Entity
    {
        private static BossesHelperSession.HealthSystemData HealthData => BossesHelperModule.Session.healthData;

        public new bool Visible
        {
            get
            {
                return healthIcons.Visible;
            }
            set
            {
                healthIcons.Visible = value;
            }
        }

        public readonly HealthIconList healthIcons;

        internal DamageHealthBar()
        {
            healthIcons = new(SeparateList(HealthData.iconSprite), SeparateList(HealthData.startAnim), SeparateList(HealthData.endAnim),
                SeparateFloatList(HealthData.iconSeparation), HealthData.playerHealthVal, HealthData.healthBarPos, HealthData.healthIconScale);
            Tag = Tags.HUD;
            Visible = HealthData.startVisible;
            if (HealthData.globalController)
                AddTag(Tags.Global);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            (scene as Level).Add(healthIcons);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            healthIcons.Clear();
            healthIcons.RefillHealth();
            healthIcons.DrawHealthBar();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            healthIcons.RemoveSelf();
        }
    }
}
