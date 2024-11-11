using Monocle;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

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
            healthIcons = new();
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
            healthIcons.Clear();
            healthIcons.RefillHealth();
            base.Awake(scene);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            healthIcons.RemoveSelf();
        }
    }
}
