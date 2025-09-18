---@meta Celeste.Mod.BossesHelper.Code.Helpers.Lua

---@class Celeste.Mod.BossesHelper.Code.Helpers.Lua
local lua = {}

---@class LuaMethodWrappers
---@field TeleportTo fun(scene: Scene, player: Player, room: string, intro?: IntroTypes, nearest?: Vector2)
---@field InstantTeleport fun(scene: Scene, player: Player, room: string, relative: boolean, posX: number, postY: number)
---@field GetEntities fun(name: string, prefix?: string): table
---@field GetAllEntities fun(name: string, prefix?: string): table
---@field GetEntity fun(name: string, prefix?: string): any
---@field GetFirstEntity fun(name: string, prefix?: string): any
---@field GetComponents fun(name: string, prefix?: string): table
---@field GetAllComponents fun(name: string, prefix?: string): table
---@field GetComponent fun(name: string, prefix?: string): any
---@field GetFirstComponent fun(name: string, prefix?: string): any
---@field GetAllComponentsOnType fun(name: string, entity: string, prefix?: string, entityPrefix?: string): table
---@field GetFirstComponentOnType fun(name: string, entity: string, prefix?: string, entityPrefix?: string): any
---@field GetComponentsFromEntity fun(entity: Entity, name: string, prefix?: string): table
---@field GetComponentFromEntity fun(entity: Entity, name: string, prefix?: string): any
---@field EntityHasComponent fun(entity: Entity, name: string, prefix?: string): boolean
lua.LuaMethodWrappers = {}

return lua
