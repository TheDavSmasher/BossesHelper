using Microsoft.Xna.Framework;
using Monocle;
using System;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    [Tracked(false)]
    public partial class GlobalSavePointChanger(object levelNameSrc, Vector2 spawnPoint, Player.IntroTypes spawnType = Player.IntroTypes.Respawn)
        : Component(active: false, visible: false)
    {
        public readonly string spawnLevel = LevelName(levelNameSrc);

        public Vector2 spawnPoint = spawnPoint;

        private static string LevelName(object source)
        {
            return source switch
            {
                EntityID id => id.Level,
                LevelData ld => ld.Name,
                Session session => session.Level,
                Scene sc => LevelName((sc as Level).Session),
                Entity e => LevelName(e.Scene),
                _ => throw new Exception("Object type cannot be used to get a Level Name.")
            };
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            if (Scene != null)
            {
                AddedToScene();
            }
        }

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            AddedToScene();
        }

        private void AddedToScene()
        {
            Vector2 newSpawn = SceneAs<Level>().GetSpawnPoint(spawnPoint);
            if (newSpawn != spawnPoint && DistanceBetween(spawnPoint, newSpawn) <= 80)
                spawnPoint = newSpawn;
        }

        public override void Update()
        {
            BossesHelperModule.Session.savePointLevel = spawnLevel;
            BossesHelperModule.Session.savePointSpawn = spawnPoint;
            BossesHelperModule.Session.savePointSpawnType = spawnType;
            BossesHelperModule.Session.savePointSet = true;
            Active = false;
        }
    }
}
