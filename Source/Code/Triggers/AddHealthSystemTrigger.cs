using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.Entities;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/AddHealthSystemTrigger")]
    public class AddHealthSystemTrigger : Trigger
    {
        private EntityID id;

        private readonly bool onlyOnce;

        public AddHealthSystemTrigger(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset)
        {
            this.id = id;
            onlyOnce = data.Bool("onlyOnce", true);
            new HealthSystemManager(data, offset);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            SceneAs<Level>().Add(HealthSystemManager.mapHealthSystemManager);
            if (onlyOnce)
            {
                SceneAs<Level>().Session.DoNotLoad.Add(id);
            }
            RemoveSelf();
        }
    }
}
