using Monocle;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    public class PlayerHealthBar : Entity
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

        internal PlayerHealthBar()
        {
            healthIcons = new();
            Tag = Tags.HUD;
            Visible = HealthData.startVisible;
            if (HealthData.globalController)
                AddTag(Tags.Global);
                healthIcons.MakeGlobal();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            (scene as Level).Add(healthIcons);
        }

        public override void Awake(Scene scene)
        {
            if (scene.Tracker.GetEntity<PlayerHealthBar>() != this)
            {
                RemoveSelf();
                return;
            }
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
