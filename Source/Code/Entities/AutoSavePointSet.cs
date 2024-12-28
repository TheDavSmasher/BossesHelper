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

        private readonly Vector2? spawnPosition;

        private GlobalSavePointChanger Changer;

        public AutoSavePointSet(EntityData data, Vector2 _, EntityID id)
        {
            ID = id;
            spawnType = data.Enum<Player.IntroTypes>("respawnType");
            if (data.FirstNodeNullable() is Vector2 spawn)
            {
                spawnPosition = spawn;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Session session = (scene as Level).Session;
            if (spawnPosition is Vector2 node)
            {
                Add(Changer = new(ID, node, spawnType));
            }
            else if (session.RespawnPoint is Vector2 spawn)
            {
                Add(Changer = new(ID, spawn, spawnType));
            }
            Changer?.Update();
            RemoveSelf();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            (scene as Level).Session.DoNotLoad.Add(ID);
        }
    }
}
