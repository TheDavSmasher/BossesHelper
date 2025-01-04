using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using NLua;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
    internal static class LuaMethodWrappers
    {
        #region Method Infos
        private static readonly MethodInfo getEntityMethodInfo = Engine.Scene.Tracker.GetType().GetMethod("GetEntity");

        private static readonly MethodInfo getEntitiesMethodInfo = Engine.Scene.Tracker.GetType().GetMethod("GetEntities");

        private static readonly MethodInfo entitiesFindFirstMethodInfo = Engine.Scene.Entities.GetType().GetMethod("FindFirst");

        private static readonly MethodInfo entitiesFindAll = Engine.Scene.Entities.GetType().GetMethod("FindAll");


        private static readonly MethodInfo getComponentMethodInfo = Engine.Scene.Tracker.GetType().GetMethod("GetComponent");

        private static readonly MethodInfo getComponentsMethodInfo = Engine.Scene.Tracker.GetType().GetMethod("GetComponents");

        private static readonly MethodInfo componentsGetFirst = typeof(ComponentList).GetMethod("Get");

        private static readonly MethodInfo componentsGetAll = typeof(ComponentList).GetMethod("GetAll");


        private static readonly MethodInfo entityIsTracked = Engine.Scene.Tracker.GetType().GetMethod("IsEntityTracked");
        #endregion

        public static Type GetTypeFromString(string name, string prefix = "Celeste.")
        {
            return FakeAssembly.GetFakeEntryAssembly().GetType(prefix + name);
        }

        public static object CallGenericOn(MethodInfo method, Type type, object on)
        {
            return method.MakeGenericMethod(type).Invoke(on, null);
        }

        #region Entities
        public static object GetEntity(string name, string prefix = "Celeste.")
        {
            return GetEntity(GetTypeFromString(name, prefix));
        }

        public static object GetEntity(Type type)
        {
            try
            {
                return CallGenericOn(getEntityMethodInfo, type, Engine.Scene.Tracker);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to get entity: Requested type does not exist");
            }
            catch (TargetInvocationException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get entity: '{type}' is not trackable");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get entity: {arg}");
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
                return LuaBossHelper.ListToLuaTable(CallGenericOn(getEntitiesMethodInfo, type, Engine.Scene.Tracker) as IList);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to get entities: Requested type does not exist");
            }
            catch (TargetInvocationException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get entities: '{type}' is not trackable");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get entities: {arg}");
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
                return CallGenericOn(entitiesFindFirstMethodInfo, type, Engine.Scene.Entities);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to get entities: Requested type does not exist");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get entity: {arg}");
            }
            return null;
        }

        public static LuaTable GetAllEntities(string name, string prefix = "Celeste.")
        {
            return GetAllEntities(GetTypeFromString(name, prefix));
        }

        public static LuaTable GetAllEntities(Type type)
        {
            try
            {
                return LuaBossHelper.ListToLuaTable(CallGenericOn(entitiesFindAll, type, Engine.Scene.Entities) as IList);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to get entities: Requested type does not exist");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get entity: {arg}");
            }
            return null;
        }
        #endregion

        #region Components
        public static object GetComponent(string name, string prefix = "Celeste.")
        {
            return GetComponent(GetTypeFromString(name, prefix));
        }

        public static object GetComponent(Type type)
        {
            try
            {
                return CallGenericOn(getComponentMethodInfo, type, Engine.Scene.Tracker);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to get component: Requested type does not exist");
            }
            catch (TargetInvocationException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get component: '{type}' is not trackable");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get component: {arg}");
            }
            return null;
        }

        public static LuaTable GetComponents(string name, string prefix = "Celeste.")
        {
            return GetComponents(GetTypeFromString(name, prefix));
        }

        public static LuaTable GetComponents(Type type)
        {
            try
            {
                return LuaBossHelper.ListToLuaTable(CallGenericOn(getComponentsMethodInfo, type, Engine.Scene.Tracker) as IList);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to get components: Requested type does not exist");
            }
            catch (TargetInvocationException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get components: '{type}' is not trackable");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get components: {arg}");
            }
            return null;
        }

        public static object GetFirstComponentOnType(string name, string entity, string prefix = "Celeste.", string entityPre = "Celeste.")
        {
            return GetFirstComponentOnType(GetTypeFromString(name, prefix), GetTypeFromString(entity, entityPre));
        }

        public static object GetFirstComponentOnType(Type type, Type entity)
        {
            try
            {
                IList entities = ((bool) CallGenericOn(entityIsTracked, entity, Engine.Scene.Tracker)
                    ? CallGenericOn(getEntitiesMethodInfo, entity, Engine.Scene.Tracker)
                    : CallGenericOn(entitiesFindAll, entity, Engine.Scene.Entities)) as IList;
                foreach (Entity entityEntity in entities)
                {
                    if (GetComponentFromEntity(entityEntity, type) is object res)
                        return res;
                }
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to get component: Requested type does not exist");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get component: {arg}");
            }
            return null;
        }

        public static LuaTable GetAllComponentsOnType(string name, string entity, string prefix = "Celeste.", string entityPre = "Celeste.")
        {
            return GetAllComponentsOnType(GetTypeFromString(name, prefix), GetTypeFromString(entity, entityPre));
        }

        public static LuaTable GetAllComponentsOnType(Type type, Type entity)
        {
            try
            {
                IList entities = ((bool)CallGenericOn(entityIsTracked, entity, Engine.Scene.Tracker)
                    ? CallGenericOn(getEntitiesMethodInfo, entity, Engine.Scene.Tracker)
                    : CallGenericOn(entitiesFindAll, entity, Engine.Scene.Entities)) as IList;
                LuaTable luaTable = LuaBossHelper.GetEmptyTable();
                int num = 1;
                foreach (Entity entityEntity in entities)
                {
                    foreach (object item in GetComponentsFromEntity(entityEntity, type))
                    {
                        luaTable[num++] = item;
                    }
                }
                return luaTable;
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to get components: Requested type does not exist");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get components: {arg}");
            }
            return null;
        }

        public static object GetFirstComponent(string name, string prefix = "Celeste.")
        {
            return GetFirstComponent(GetTypeFromString(name, prefix));
        }

        public static object GetFirstComponent(Type type)
        {
            try
            {
                foreach (Entity entityEntity in Engine.Scene.Entities)
                {
                    if (GetComponentFromEntity(entityEntity, type) is object res)
                        return res;
                }
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to get component: Requested type does not exist");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get component: {arg}");
            }
            return null;
        }

        public static LuaTable GetAllComponents(string name, string prefix = "Celeste.")
        {
            return GetAllComponents(GetTypeFromString(name, prefix));
        }

        public static LuaTable GetAllComponents(Type type)
        {
            try
            {
                LuaTable luaTable = LuaBossHelper.GetEmptyTable();
                int num = 1;
                foreach (Entity entity in Engine.Scene.Entities)
                {
                    foreach (object item in GetComponentsFromEntity(entity, type))
                    {
                        luaTable[num++] = item;
                    }
                }
                return luaTable;
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to get components: Requested type does not exist");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get components: {arg}");
            }
            return null;
        }

        public static object GetComponentFromEntity(Entity entity, string name, string prefix = "Celeste.")
        {
            return GetComponentFromEntity(entity, GetTypeFromString(name, prefix));
        }

        public static object GetComponentFromEntity(Entity entity, Type type)
        {
            try
            {
                return CallGenericOn(componentsGetFirst, type, entity.Components);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to get component: Requested type does not exist");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get component: {arg}");
            }
            return null;
        }

        public static LuaTable GetComponentsFromEntity(Entity entity, string name, string prefix = "Celeste.")
        {
            return GetComponentsFromEntity(entity, GetTypeFromString(name, prefix));
        }

        public static LuaTable GetComponentsFromEntity(Entity entity, Type type)
        {
            try
            {
                return LuaBossHelper.ListToLuaTable(CallGenericOn(componentsGetAll, type, entity.Components) as IList);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to get components: Requested type does not exist");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get components: {arg}");
            }
            return null;
        }

        public static bool EntityHasComponent(Entity entity, string name, string prefix = "Celeste.")
        {
            return EntityHasComponent(entity, GetTypeFromString(name, prefix));
        }

        public static bool EntityHasComponent(Entity entity, Type type)
        {
            return GetComponentFromEntity(entity, type) != null;
        }
        #endregion

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

        #region Entity Collider Creator
        /*public static object GetEntityCollider(object baseEntity, LuaFunction func, Collider collider = null)
        {
            try
            {
                Type baseType;
                if (baseEntity is string val)
                {
                    baseType = GetTypeFromString(val);
                }
                else if (baseEntity is Entity entity)
                {
                    baseType = entity.GetType();
                }
                else
                {
                    return null;
                }

                return Activator.CreateInstance(typeof(EntityCollider<>).MakeGenericType(baseType), [func, collider]);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to get component: Requested entity type does not exist");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get component: {arg}");
            }
            return null;
        }

        public static object GetEntityColliderByComponent(object baseComponent, LuaFunction func, Collider collider = null)
        {
            try
            {
                Type baseType;
                if (baseComponent is string val)
                {
                    baseType = GetTypeFromString(val);
                }
                else if (baseComponent is Component component)
                {
                    baseType = component.GetType();
                }
                else
                {
                    return null;
                }

                return Activator.CreateInstance(typeof(EntityColliderByComponent<>).MakeGenericType(baseType), [func, collider]);
            }
            catch (ArgumentNullException)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", "Failed to get component: Requested component type does not exist");
            }
            catch (Exception arg)
            {
                Logger.Log(LogLevel.Error, "Bosses Helper", $"Failed to get component: {arg}");
            }
            return null;
        }*/
        #endregion
    }
}
