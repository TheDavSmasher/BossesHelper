---@meta Celeste.Mod.BossesHelper.Code.Components

---@class Components
local components = {}

--#region Stopwatch
---@class Stopwatch : Component
---@field TimeLeft number
components.Stopwatch = {}

---Reset the Stopwatch
function components.Stopwatch:Reset() end
--#endregion

---@class EntityChecker : Component
---@overload fun(checker: (fun(): boolean), onCheck: fun(entity: Entity), state?: boolean, removeOnComplete?: boolean): EntityChecker
components.EntityChecker = {}

---@class EntityTimer : Component
---@overload fun(timer: number, onTimer: fun(entity: Entity)): EntityTimer
components.EntityTimer = {}

---@class EntityFlagger : Component
---@overload fun(flag: string, onFlag: fun(entity: Entity), state?: boolean, reset?: boolean): EntityFlagger
components.EntityFlagger = {}

---@class EntityChain : Component
---@overload fun(entity: Entity, startChained: boolean, removeTogether?: boolean): EntityChain
components.EntityChain = {}

return components