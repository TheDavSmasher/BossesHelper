---@meta CelesteMod

---@class _G
---@field luanet any Luanet server

---@alias Easer Easer A Monocle Easer, used for Tweens.

---@class Component A Monocle Component object.

---@class Collider : Component A Monocle Collider object.

---@class ColliderList : Collider A Monocle ColliderList object, combining multiple Colliders.

---@class Sprite : Component
---@field PlayAnim fun(self: Sprite, anim: string): any

---@class Stopwatch : Component
---@field TimeLeft number
---@field Reset fun(self: Stopwatch)

---@class Random
---@field Next fun(self: Random): integer

---@class Vector2 A Vector2 object.
---@field X number The x component of the vector
---@field Y number The y component of the vector
---@field Length fun(self: Vector2): number
---@field SafeNormalize fun(self: Vector2): Vector2
---@field SafeNormalize fun(self: Vector2, length: number): Vector2
---@operator sub(Vector2): Vector2

---@class EntityData An Everest EntityData object.
---@field Values any

---@class Entity A Monocle Entity object
---@field Add fun(self: Entity, component: Component) Adds a component to the Entity
---@field Position Vector2
---@field Collidable boolean
---@field PositionTween fun(self: Entity, target: Vector2, time: number, easer: Easer?)

---@class BossController : Entity
---@field Health integer
---@field IsActing boolean
---@field CurrentPatternIndex integer
---@field CurrentPatternName string
---@field Random Random
---@field AddEntity fun(self: BossController, entity: Entity)
---@field DestroyEntity fun(self: BossController, entity: Entity)
---@field DestroyAll fun(self: BossController)
---@field InterruptPattern fun(self: BossController)
---@field GetPatternIndex fun(self: BossController, name: string): integer
---@field StartAttackPattern fun(self: BossController, index: number)
---@field ForceNextAttack fun(self: BossController, index: number)
---@field SavePhaseChangeToSession fun(self: BossController, health: integer, index: integer, startImmediately: boolean)
---@field RemoveBoss fun(self: BossController, permanent: boolean)
---@field StoreObject fun(self: BossController, key: string, value: any)
---@field GetStoredObject fun(self: BossController, key: string): any
---@field DeleteStoredObject fun(self: BossController, key: string)

---@class BossPuppet : Entity
---@field Speed Vector2
---@field gravityMult number
---@field groundFriction number
---@field airFriction number
---@field Sprite Sprite
---@field SolidCollidable boolean
---@field BossHitCooldown Stopwatch
---@field PlayBossAnim fun(self : BossPuppet, anim: string)
---@field Set1DSpeedDuring fun(self: BossPuppet, speed: number, isX: boolean, time: number)
---@field Speed1DTween fun(self: BossPuppet, start: number, target: number, time: number, isX: boolean, easer: Easer?)
---@field ChangeHitboxOption fun(self : BossPuppet, tag: string)
---@field ChangeHurtboxOption fun(self : BossPuppet, tag: string)
---@field ChangeBounceboxOption fun(self : BossPuppet, tag: string)
---@field ChangeTargetOption fun(self : BossPuppet, tag: string)


---@diagnostic disable: missing-fields

---@type BossPuppet
puppet = {}

---@type BossController
boss = {}