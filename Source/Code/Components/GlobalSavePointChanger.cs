using Monocle;
using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    public class GlobalSavePointChanger(string level, Vector2 spawnPoint, Player.IntroTypes spawnType = Player.IntroTypes.Respawn)
        : Component(active: false, visible: false)
    {
        public readonly string spawnLevel = level;

        public readonly Vector2 spawnPoint = spawnPoint;

        public readonly Player.IntroTypes spawnType = spawnType;

        public GlobalSavePointChanger(object source, Vector2 spawnPoint, Player.IntroTypes spawnType = Player.IntroTypes.Respawn)
            : this(LevelName(source), spawnPoint, spawnType)
        {
        }

        private static string LevelName(object source)
        {
            return source switch
            {
                string s => s,
                EntityID id => id.Level,
                Session session => session.LevelData.Name,
                Entity e => LevelName(e.SceneAs<Level>().Session),
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
