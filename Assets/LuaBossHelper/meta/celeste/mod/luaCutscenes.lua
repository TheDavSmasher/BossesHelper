---@meta Celeste.Mod.LuaCutscenes

---@class LuaCutscenes
local luaCutscenes = {}

---@class ChoicePrompt : Entity
---@field Choice integer
luaCutscenes.ChoicePrompt = {}

---Create a prompt dialog with multiple choice IDs.
---@param ... string
---@return System.Collections.IEnumerator
function luaCutscenes.ChoicePrompt.Prompt(...) end

return luaCutscenes