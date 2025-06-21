using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Celeste.Mod.Entities;
using System.Collections;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
    [CustomEntity("BossesHelper/AddHealthSystemTrigger")]
    public class AddHealthSystemTrigger(EntityData data, Vector2 offset, EntityID id)
        : SingleUseTrigger(data, offset, id, data.Bool("onlyOnce", true))
    {
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Add(new Coroutine(RemoveAndReadd()));
        }

        private IEnumerator RemoveAndReadd()
        {
            if (Scene.GetEntity<HealthSystemManager>() is HealthSystemManager manager)
            {
                manager.UpdateSessionData(entityData);
                yield return null;
            }
            else
            {
                Scene.Add(new HealthSystemManager(entityData, Vector2.Zero));
            }
            RemoveSelf();
        }
    }
}
