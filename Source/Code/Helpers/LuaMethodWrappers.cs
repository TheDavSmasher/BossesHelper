using System;
using System.Collections;
using System.Reflection;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using NLua;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    internal static class LuaMethodWrappers
    {
        private static readonly MethodInfo getEntityMethodInfo = Engine.Scene.Tracker.GetType().GetMethod("GetEntity");

        private static readonly MethodInfo getEntitiesMethodInfo = Engine.Scene.Tracker.GetType().GetMethod("GetEntities");

        private static readonly MethodInfo entitiesFindFirstMethodInfo = Engine.Scene.Entities.GetType().GetMethod("FindFirst");

        private static readonly MethodInfo entitiesFindAll = Engine.Scene.Entities.GetType().GetMethod("FindAll");

        public static Type GetTypeFromString(string name, string prefix = "Celeste.")
        {
            return FakeAssembly.GetFakeEntryAssembly().GetType(prefix + name);
        }

        public static object GetEntity(string name, string prefix = "Celeste.")
        {
            return GetEntity(GetTypeFromString(name, prefix));
        }

        public static object GetEntity(Type type)
        {
            try
            {
                return getEntityMethodInfo.MakeGenericMethod(type).Invoke(Engine.Scene.Tracker, null);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", "Failed to get entity: Requested type does not exist");
            }
            catch (TargetInvocationException)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to get entity: '{type}' is not trackable");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to get entity: {arg}");
            }
            return null;
        }

        public static LuaTable GetEntities(string name, string prefix = "Celeste.")
        {
            return GetEntities(GetTypeFromString(name, prefix));
        }

        public static LuaTable GetEntities(Type type)
        {
            try
            {
                return LuaBossHelper.ListToLuaTable(getEntitiesMethodInfo.MakeGenericMethod(type).Invoke(Engine.Scene.Tracker, null) as IList);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", "Failed to get entities: Requested type does not exist");
            }
            catch (TargetInvocationException)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to get entities: '{type}' is not trackable");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to get entities: {arg}");
            }
            return null;
        }

        public static object GetFirstEntity(string name, string prefix = "Celeste.")
        {
            return GetFirstEntity(GetTypeFromString(name, prefix));
        }

        public static object GetFirstEntity(Type type)
        {
            try
            {
                return entitiesFindFirstMethodInfo.MakeGenericMethod(type).Invoke(Engine.Scene.Entities, null);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", "Failed to get entities: Requested type does not exist");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to get entity: {arg}");
            }
            return null;
        }

        public static object GetAllEntities(string name, string prefix = "Celeste.")
        {
            return GetAllEntities(GetTypeFromString(name, prefix));
        }

        public static object GetAllEntities(Type type)
        {
            try
            {
                return LuaBossHelper.ListToLuaTable(entitiesFindAll.MakeGenericMethod(type).Invoke(Engine.Scene.Entities, null) as IList);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", "Failed to get entities: Requested type does not exist");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to get entity: {arg}");
            }
            return null;
        }

        public static void TeleportTo(Scene scene, Player player, string room, Player.IntroTypes introType = Player.IntroTypes.Transition, Vector2? nearestSpawn = null)
        {
            if (scene is Level level)
            {
                level.OnEndOfFrame += delegate
                {
                    level.TeleportTo(player, room, introType, nearestSpawn);
                };
            }
        }

        public static void InstantTeleport(Scene scene, Player player, string room, bool sameRelativePosition, float positionX, float positionY)
        {
            if (scene is not Level level)
            {
                return;
            }
            if (string.IsNullOrEmpty(room))
            {
                Vector2 vector = new Vector2(positionX, positionY) - player.Position;
                player.Position = new Vector2(positionX, positionY);
                level.Camera.Position += vector;
                player.Hair.MoveHairBy(vector);
                return;
            }
            level.OnEndOfFrame += delegate
            {
                Vector2 levelOffset = level.LevelOffset;
                Vector2 vector2 = player.Position - level.LevelOffset;
                Vector2 vector3 = level.Camera.Position - level.LevelOffset;
                Facings facing = player.Facing;
                level.Remove(player);
                level.UnloadLevel();
                level.Session.Level = room;
                level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
                level.Session.FirstLevel = false;
                level.LoadLevel(Player.IntroTypes.Transition);
                if (sameRelativePosition)
                {
                    level.Camera.Position = level.LevelOffset + vector3;
                    level.Add(player);
                    player.Position = level.LevelOffset + vector2;
                    player.Facing = facing;
                    player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
                }
                else
                {
                    Vector2 vector4 = new Vector2(positionX, positionY) - level.LevelOffset - vector2;
                    level.Camera.Position = level.LevelOffset + vector3 + vector4;
                    level.Add(player);
                    player.Position = new Vector2(positionX, positionY);
                    player.Facing = facing;
                    player.Hair.MoveHairBy(level.LevelOffset - levelOffset + vector4);
                }
                level.Wipe?.Cancel();
            };
        }
    }

}
