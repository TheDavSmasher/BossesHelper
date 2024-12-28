using Monocle;
using System;
using Microsoft.Xna.Framework;
using System.Reflection;
using Celeste.Mod.BossesHelper.Code.Helpers;
using MonoMod.Cil;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    [Tracked(false)]
    public class GlobalSavePointChanger(object level, Vector2 spawnPoint, Player.IntroTypes spawnType = Player.IntroTypes.Respawn)
        : Component(active: false, visible: false)
    {
        public readonly string spawnLevel = LevelName(level);

        public Vector2 spawnPoint = spawnPoint;

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

        public override void Added(Entity entity)
        {
            base.Added(entity);
            Vector2 newSpawn = SceneAs<Level>().GetSpawnPoint(spawnPoint);
            if (BossesHelperUtils.DistanceBetween(spawnPoint, newSpawn) <= 80)
            {
                spawnPoint = newSpawn;
            }
        }

        public override void Update()
        {
            BossesHelperModule.Session.savePointLevel = spawnLevel;
            BossesHelperModule.Session.savePointSpawn = spawnPoint;
            BossesHelperModule.Session.savePointSpawnType = spawnType;
            BossesHelperModule.Session.savePointSet = true;
            Active = false;
        }

        public void AddToEntityOnMethod<T>(T entity, string method,
            BindingFlags flags = BindingFlags.Default, bool stateMethod = false) where T : Entity
        {
            entity.Add(this);
            ILHookHelper.GenerateHookOn(typeof(T), method, AddUpdateDelegate, flags, stateMethod);
        }

        private static void AddUpdateDelegate(ILContext il)
        {
            ILCursor cursor = new(il);
            cursor.EmitLdarg0();
            cursor.EmitDelegate(UpdateSavePointChanger);
        }

        private static void UpdateSavePointChanger(Entity entity)
        {
            entity.Components.Get<GlobalSavePointChanger>()?.Update();
        }
    }
}
