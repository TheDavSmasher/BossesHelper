---@meta Celeste.Mod.BossesHelper.Code.Helpers

---@class Helpers
local helpers = {}

--#region BossesHelperUtils
---@class BossesHelperUtils
helpers.BossesHelperUtils = {}

---Play an animation on the Sprite and wait for its loop to finish
---@param tself Sprite
---@param anim string
---@return IEnumerator # The enumerator to wait.
function helpers.BossesHelperUtils.PlayAnim(tself, anim) end

---Create a Position Tween on the given entity.
---@param tself Entity The Entity to move.
---@param target Vector2 The target position.
---@param time number The time to take.
---@param easer? Ease.Easer The Easer for the motion.
function helpers.BossesHelperUtils.PositionTween(tself, target, time, easer) end
--#endregion

--#region LuaBossHelper
---@class LuaBossHelper
helpers.LuaBossHelper = {}

---Get the string contents of the file.
---@param file string The file path.
---@return string contents
function helpers.LuaBossHelper.GetFileContent(file) end

---Create a ColliderList from the given Colliders.
---@param ... Collider
---@return ColliderList
function helpers.LuaBossHelper.GetColliderListFromLuaTable(...) end

---Add the given function as a Coroutine on the Entity passed.
---@param entity Entity The Entity to add the Coroutine to.
---@param func function The function to execute in the Coroutine.
function helpers.LuaBossHelper.AddConstantBackgroundCoroutine(entity, func) end

---Pass the functions to be used as triggers while saying the given dialog ID in a Textbox.
---@param dialog string The dialog ID to use for the Textbox.
---@param funcs function[] The functions to use as triggers.
---@return IEnumerator
function helpers.LuaBossHelper.Say(dialog, funcs) end

---Execute the given function in the background after the given delay.
---@param func function The function to call.
---@param delay number The amount of time to wait to execute the function.
function helpers.LuaBossHelper.DoMethodAfterDelay(func, delay) end
--#endregion

---@module "Celeste.Mod.BossesHelper.Code.Helpers.Lua"
helpers.Lua = {}

return helpers