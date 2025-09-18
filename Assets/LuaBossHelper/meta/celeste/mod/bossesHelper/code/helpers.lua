---@meta Celeste.Mod.BossesHelper.Code.Helpers

---@class Helpers
local helpers = {}

---@class BossesHelperUtils
---@field PlayAnim fun(self: Sprite, anim: string): System.Collections.IEnumerator
---@field PositionTween fun(self: Entity, target: Vector2, time: number, easer?: Ease.Easer)
helpers.BossesHelperUtils = {}

---@class LuaBossHelper
---@field GetFileContent fun(file: string): string
---@field GetColliderListFromLuaTable fun(...: Collider): ColliderList
---@field AddConstantBackgroundCoroutine fun(entity: Entity, func: function)
---@field Say fun(dialog: string, funcs: function[]): System.Collections.IEnumerator
---@field DoMethodAfterDelay fun(func: function, delay: number)
helpers.LuaBossHelper = {}

---@module "Celeste.Mod.BossesHelper.Code.Helpers.Lua"
helpers.Lua = {}

return helpers