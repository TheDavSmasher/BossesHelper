---@meta Celeste.Mod.BossesHelper.Code.Helpers.Lua

---@class Lua
local lua = {}

--#region LuaMethodWrappers
---@class LuaMethodWrappers
lua.LuaMethodWrappers = {}

--#region Teleport

---Sets the player position to the absolute coordinates.
---@param scene Scene The scene where the Player is.
---@param player Player The Player to teleport.
---@param room string What room the game should attempt to load.
---@param intro? IntroTypes The IntroType to use when teleporting.
---@param nearest? Vector2 The nearest spawnpoint to use to teleport.
function lua.LuaMethodWrappers.TeleportTo(scene, player, room, intro, nearest) end

---Instantly teleport the player seamlessly.
---@param scene Scene The scene where the Player is.
---@param player Player The Player to teleport.
---@param room string What room the game should attempt to load.
---@param relative boolean Whether to keep the player's same relative positions to current room in the new room.
---@param posX number The X position offset.
---@param posY number The Y position offset.
function lua.LuaMethodWrappers.InstantTeleport(scene, player, room, relative, posX, posY) end
--#endregion

--#region Entities

---Gets all tracked entities by class name.
---@param name string Class name of the entity, relative to "Celeste." by default.
---@param prefix string? Overrides the global class name prefix.
---@return table entities Tracked entities of given class.
function lua.LuaMethodWrappers.GetEntities(name, prefix) end

---Gets the first tracked entity by class name.
---@param name string Class name of the entity, relative to "Celeste." by default.
---@param prefix string? Overrides the global class name prefix.
---@return any entity First tracked entity of given class.
function lua.LuaMethodWrappers.GetEntity(name, prefix) end

---Gets all entities by class name.
---@param name string Class name of the entity, relative to "Celeste." by default.
---@param prefix string? Overrides the global class name prefix.
---@return table entities All entities of given class.
function lua.LuaMethodWrappers.GetAllEntities(name, prefix) end

---Gets the first entity by class name.
---@param name string Class name of the entity, relative to "Celeste." by default.
---@param prefix string? Overrides the global class name prefix.
---@return any entity First entity of given class.
function lua.LuaMethodWrappers.GetFirstEntity(name, prefix) end
--#endregion

--#region Components

---Gets all tracked components by class name.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix.
---@return table components Tracked components of given class.
function lua.LuaMethodWrappers.GetComponents(name, prefix) end

---Gets the first tracked component by class name.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix.
---@return any component First tracked component of given class.
function lua.LuaMethodWrappers.GetComponent(name, prefix) end

---Gets all components by class name.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix.
---@return table components All components of given class on scene.
function lua.LuaMethodWrappers.GetAllComponents(name, prefix) end

---Gets the first component by class name.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix.
---@return any component First component of given class.
function lua.LuaMethodWrappers.GetFirstComponent(name, prefix) end

---Gets all components by class name added to an entity of given class name.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param entity string Class name of the entity, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix for the Component class.
---@param entityPre? string Overrides the global class name prefix for the Entity class.
---@return table components All components of given class on scene attached to the entity type.
function lua.LuaMethodWrappers.GetAllComponentsOnType(name, entity, prefix, entityPre) end

---Gets the first component by class name added to an entity of the given class name.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param entity string Class name of the entity, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix for the Component class.
---@param entityPre? string Overrides the global class name prefix for the Entity class.
---@return any component First component of given class attached to the entity type.
function lua.LuaMethodWrappers.GetFirstComponentOnType(name, entity, prefix, entityPre) end

---Returns all the components of the given class name from the entity given, if any.
---@param entity Entity The entity to check.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix.
---@return table components All components of given class on scene sored on the entity, if any.
function lua.LuaMethodWrappers.GetComponentsFromEntity(entity, name, prefix) end

---Returns the component of the given class name from the entity given, if any.
---@param entity Entity The entity to check.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix.
---@return any component First component of given class stored on the entity, if any.
function lua.LuaMethodWrappers.GetComponentFromEntity(entity, name, prefix) end

---Checks if the entity given has a component of the given class name.
---@param entity Entity The entity to check.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix.
---@return boolean componentFound If the Entity does have a Component of the type specified.
function lua.LuaMethodWrappers.EntityHasComponent(entity, name, prefix) end
--#endregion

--#endregion

return lua
