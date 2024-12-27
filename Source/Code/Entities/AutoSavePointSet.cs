using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [CustomEntity("BossesHelper/AutoSavePointSet")]
    public class AutoSavePointSet : Entity
    {
        private readonly EntityID ID;

        private readonly Player.IntroTypes spawnType;

        private GlobalSavePointChanger Changer;

        public AutoSavePointSet(EntityData data, Vector2 _, EntityID id)
        {
            ID = id;
            spawnType = data.Enum<Player.IntroTypes>("respawnType");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Session session = (scene as Level).Session;
            if (session.RespawnPoint is Vector2 spawn)
            {
                Add(Changer = new(ID.Level, spawn, spawnType));
                Changer.Update();
            }
            RemoveSelf();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            (scene as Level).Session.DoNotLoad.Add(ID);
        }
    }
}
