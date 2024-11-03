using Monocle;
using System.Linq;
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
            Position = HealthData.healthBarPos;
            healthIcons = new([HealthData.iconSprite], [HealthData.startAnim], [HealthData.endAnim], [HealthData.iconSeparation],
                HealthData.playerHealthVal, HealthData.healthIconScale);
            Tag = Tags.HUD;
            Visible = HealthData.startVisible;
            if (HealthData.globalController)
                AddTag(Tags.Global);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            healthIcons.Clear();
            healthIcons.DrawHealthBar();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            healthIcons.RemoveSelf();
        }
    }
}
