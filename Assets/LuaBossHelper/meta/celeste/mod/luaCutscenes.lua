---@meta Celeste.Mod.LuaCutscenes

---@class LuaCutscenes
local luaCutscenes = {}

--#region ChoicePrompt
---@class ChoicePrompt : Entity
---@field Choice integer
luaCutscenes.ChoicePrompt = {}

---Create a prompt dialog with multiple choice IDs.
---@param ... string
---@return IEnumerator
function luaCutscenes.ChoicePrompt.Prompt(...) end
--#endregion

return luaCutscenes