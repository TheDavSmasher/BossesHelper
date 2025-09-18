---@meta System

---@class System.Collections.IEnumerator

---@class System.Collections.Generic.HashSet<T>
---@field Add fun(self: System.Collections.Generic.HashSet, value: `T`)
---@field Remove fun(self: System.Collections.Generic.HashSet, value: `T`)

---@class System.Collections.Generic.Dictionary<K, V>
---@field Add fun(self: System.Collections.Generic.Dictionary, key: `K`, value: `V`)

---@class System.Random
---@field Next fun(self: System.Random): integer
