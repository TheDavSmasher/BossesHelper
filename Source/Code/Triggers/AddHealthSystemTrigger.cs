using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
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
            Scene.Tracker.GetEntity<HealthSystemManager>()?.RemoveSelf();
            yield return null;
            SceneAs<Level>().Add(new HealthSystemManager(data, Vector2.Zero));
            RemoveSelf();
        }
    }
}
