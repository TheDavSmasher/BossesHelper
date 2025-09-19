---@meta Microsoft.XNA.Framework

---@class Microsoft.XNA.Framework
local framework = {}

---@class Rectangle
---@field Left number
---@field Bottom number
---@field X number
---@field Y number
framework.Rectangle = {}

--#region Vector2
---@class Vector2
---@overload fun(x: number, y: number): Vector2
---@field X number The x component of the vector
---@field Y number The y component of the vector
---@operator add(Vector2): Vector2
---@operator sub(Vector2): Vector2
framework.Vector2 = {}

---Get the length of the Vector2
---@return number length The Vector2's length
function framework.Vector2:Length() end
--#endregion