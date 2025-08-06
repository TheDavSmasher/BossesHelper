using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [CustomEntity("BossesHelper/AutoSavePointSet")]
    public class AutoSavePointSet(EntityData data, Vector2 pos, EntityID ID) : Entity(pos)
    {
        private readonly Player.IntroTypes spawnType = data.Enum<Player.IntroTypes>("respawnType");

        private readonly Vector2? spawnPosition = data.FirstNodeNullable();

        private readonly bool onlyOnce = data.Bool("onlyOnce");

        private GlobalSavePointChanger Changer;

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(Changer = new(ID,
                spawnPosition ?? SceneAs<Level>().Session.RespawnPoint ?? Position,
                spawnType));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Changer?.Update();
            RemoveSelf();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (onlyOnce)
                scene.DoNotLoad(ID);
        }
    }
}
