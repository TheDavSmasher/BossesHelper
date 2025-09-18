---@meta Monocle

---@class Monocle
local monocle = {}

---@class Calc
monocle.Calc = {}

---Normalize a Vector2 with the given length or to a unit vector
---@param tself Vector2
---@param length? number The length to give the Vector2. Defaults to 1.
---@return Vector2 # The normalized Vector2
function monocle.Calc.SafeNormalize(tself, length) end

---@class Camera
---@field Position Vector2
monocle.Camera = {}

---@class Circle : Collider
---@overload fun(radius: number, x: number, y: number): Circle
monocle.Circle = {}

---@class Collider : Component
monocle.Collider = {}

---@class ColliderList : Collider
monocle.ColliderList = {}

---@class Component
monocle.Component = {}

---@class Ease : { [string]: Ease.Easer }
monocle.Ease = {}

---@alias Ease.Easer fun(t: number): number

---Invert an [Ease.Easer](lua://Ease.Easer).
---@param easer Ease.Easer The [Ease.Easer](lua://Ease.Easer) to invert
---@return Ease.Easer inverse The inverted [Ease.Easer](lua://Ease.Easer)
function monocle.Ease.Invert(easer) end

---@class Engine
---@field Scene Scene
monocle.Engine = {}

---@class Entity
---@field Position Vector2
---@field Center Vector2
---@field Collidable boolean
monocle.Entity = {}

---Add a Component to the Entity.
---@param component Component The Component to add.
function monocle.Entity:Add(component) end

---@class Hitbox
---@overload fun(width: number, height: number, x: number, y: number): Hitbox
monocle.Hitbox = {}

---@class Sprite
monocle.Sprite = {}

---Play an animation.
---@param anim string The animation to play.
function monocle.Sprite:Play(anim) end

---@class Scene
monocle.Scene = {}

---Add an [Entity](lua://Entity) onto the [Scene](lua://Scene).
---@param entity Entity The [Entity](lua://Entity) to add.
function monocle.Scene:Add(entity) end

---Remove an [Entity](lua://Entity) from the [Scene](lua://Scene).
---@param entity Entity The [Entity](lua://Entity) to remove.
function monocle.Scene:Remove(entity) end

---@class StateMachine
---@field State integer
---@field Locked boolean
monocle.StateMachine = {}