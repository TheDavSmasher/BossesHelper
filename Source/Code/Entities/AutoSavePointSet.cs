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

        private readonly bool onlyOnce;

        private GlobalSavePointChanger Changer;

        public AutoSavePointSet(EntityData data, Vector2 offset, EntityID id)
            : base()
        {
            ID = id;
            spawnType = data.Enum<Player.IntroTypes>("respawnType");
            if (data.FirstNodeNullable() is Vector2 spawn)
            {
                spawnPosition = spawn + offset;
            }
            onlyOnce = data.Bool("onlyOnce");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Session session = (scene as Level).Session;
            if (spawnPosition is Vector2 node)
            {
                Add(Changer = new(ID, node, spawnType));
            }
            else if (session.RespawnPoint is Vector2 spawn)
            {
                Add(Changer = new(ID, spawn, spawnType));
            }
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
                (scene as Level).Session.DoNotLoad.Add(ID);
        }
    }
}
