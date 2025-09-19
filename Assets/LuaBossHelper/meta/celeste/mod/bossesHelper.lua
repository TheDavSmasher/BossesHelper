---@meta Celeste.Mod.BossesHelper

---@class BossesHelper
local bossesHelper = {}

--#region BossesHelperModule
---@class BossesHelperModule
---@field PlayerHealth integer
bossesHelper.BossesHelperModule = {}

---Make the Player invincible to damage/death for the given amount of time, additional to any current I-Frames.
---@param time number The invincible time to add.
function bossesHelper.BossesHelperModule.GiveIFrames(time) end

---Create a new EntityData object with an initialized Values Dictionary.
---@return EntityData
function bossesHelper.BossesHelperModule.MakeEntityData() end
--#endregion

---@module "Celeste.Mod.BossesHelper.Code"
bossesHelper.Code = {}

return bossesHelper