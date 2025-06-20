using Monocle;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    public class PlayerHealthBar : HudEntity
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

        private readonly HealthIconList healthIcons;

        internal PlayerHealthBar() : base(HealthData.globalController)
        {
            healthIcons = new(IsGlobal);
            Visible = HealthData.startVisible;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(healthIcons);
        }

        public override void Awake(Scene scene)
        {
            if (scene.GetEntity<PlayerHealthBar>() != this)
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

        public void RefillHealth(int? upTo = null)
        {
            healthIcons.RefillHealth(upTo);
        }

        public void IncreaseHealth(int amount = 1)
        {
            healthIcons.IncreaseHealth(amount);
        }

        public void DecreaseHealth(int amount = 1)
        {
            healthIcons.DecreaseHealth(amount);
        }
    }
}
