using Monocle;
using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    [Tracked(false)]
    public class GlobalSavePointChanger(object level, Vector2 spawnPoint, Player.IntroTypes spawnType = Player.IntroTypes.Respawn)
        : Component(active: false, visible: false)
    {
        public readonly string spawnLevel = LevelName(level);

        public readonly Vector2 spawnPoint = spawnPoint;

        public readonly Player.IntroTypes spawnType = spawnType;

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
