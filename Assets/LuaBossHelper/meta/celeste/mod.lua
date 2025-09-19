---@meta Celeste.Mod

---@class Celeste.Mod
local mod = {}

---@class EverestModuleMetadata
---@field Name string
mod.EverestModuleMetadata = {}

--#region Logger
---@class Logger
mod.Logger = {}

---Log an Error message
---@param tag string The tag for the message.
---@param message string The message
function mod.Logger.Error(tag, message) end

---Log an Info message
---@param tag string The tag for the message.
---@param message string The message
function mod.Logger.Info(tag, message) end
--#endregion

---@module "Celeste.Mod.BossesHelper"
mod.BossesHelper = {}

---@module "Celeste.Mod.LuaCutscenes"
mod.LuaCutscenes = {}

return mod