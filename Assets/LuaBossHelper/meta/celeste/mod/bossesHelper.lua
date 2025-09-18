---@meta Celeste.Mod.BossesHelper

---@class BossesHelper
local bossesHelper = {}

---@class BossesHelperModule
---@field PlayerHealth integer
---@field GiveIFrames fun(time: number)
---@field MakeEntityData fun(): EntityData
bossesHelper.BossesHelperModule = {}

---@module "Celeste.Mod.BossesHelper.Code"
bossesHelper.Code = {}

return bossesHelper