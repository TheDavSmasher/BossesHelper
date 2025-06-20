using Monocle;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    public class PlayerHealthBar : HealthIconList
    {
        private static BossesHelperSession.HealthSystemData HealthData => BossesHelperModule.Session.healthData;

        internal PlayerHealthBar() : base(HealthData.globalController)
        {
            Visible = HealthData.startVisible;
        }

        public override void Awake(Scene scene)
        {
            if (scene.GetEntity<PlayerHealthBar>() != this)
            {
                RemoveSelf();
                return;
            }
            Clear();
            base.Awake(scene);
        }
    }
}
