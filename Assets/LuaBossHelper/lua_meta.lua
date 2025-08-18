---@meta CelesteMod

---@class Vector2 A Vector2 object.
---@field X number The x component of the vector
---@field Y number The y component of the vector

---@class EntityData An Everest EntityData object.
---@field Values any

---@class Entity A Monocle Entity object
---@field Add fun(Component) Adds a component to the Entity

---@alias Component Component A Monocle Component object.
---@alias Collider Collider A Monocle Collider object.
---@alias ColliderList ColliderList A Monocle ColliderList object, combining multiple Colliders.
---@alias Easer Easer A Monocle Easer, used for Tweens.

---@class _G
---@field luanet any Luanet server

---@class BossPuppet : Entity
---@field Speed Vector2
---@field gravityMult number
---@field groundFriction number
---@field airFriction number
---@field PlayBossAnim fun(self : BossPuppet, anim: string)

---@diagnostic disable: missing-fields

---@type BossPuppet
puppet = {}