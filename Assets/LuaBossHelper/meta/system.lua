---@meta System

local system = {}

--#region Random
---@class Random
system.Random = {}

---Return the next random integer
---@return integer
function system.Random:Next() end
--#endregion

--#region Collections
system.Collections = {}

---@class IEnumerator
system.Collections.IEnumerator = {}

--#region Generic
system.Collections.Generic = {}

--#region Dictionary
---@class Dictionary<K, V>
system.Collections.Generic.Dictionary = {}

---Add an element to the Dictionary with the given key.
---@generic K, V
---@param self Dictionary<K, V>
---@param key K The key of the new value.
---@param value V The value to add.
function system.Collections.Generic.Dictionary:Add(key, value) end
--#endregion

--#region HashSet
---@class HashSet<T>
system.Collections.Generic.HashSet = {}

---Add a value to the HashSet.
---@generic T
---@param self HashSet<T>
---@param value T
function system.Collections.Generic.HashSet:Add(value) end

---Remove a value from the HashSet.
---@generic T
---@param self HashSet<T>
---@param value T
function system.Collections.Generic.HashSet:Remove(value) end
--#endregion

--#endregion

--#endregion

return system