---@meta Celeste.Mod.BossesHelper.Code.Entities

---@class Entities
local entities = {}

---@class AttackEntity : Entity
---@overload fun(position: Vector2, hitboxes: Collider, funcOnPlayer: fun(self: Entity, player: Player), startCollidable: boolean, spriteName: string, xScale?: number, yScale?: number) : AttackEntity
---@field Sprite Sprite
---@field PlayAnim fun(self: AttackEntity, anim: string)
---@field SetCollisionActive fun(self: AttackEntity, state: boolean)
entities.AttackEntity = {}

---@class AttackActor : Actor
---@overload fun(position: Vector2, hitboxes: Collider, funcOnPlayer: fun(self: Entity, player: Player), startCollidable: boolean, startSolidCollidable: boolean, spriteName: string, gravMult: number, maxFall: number, xScale?: number, yScale?: number): AttackActor
---@field Sprite Sprite
---@field PlayAnim fun(self: AttackEntity, anim: string)
---@field SetCollisionActive fun(self: AttackEntity, state: boolean)
---@field Speed Vector2
---@field GravityMult number
---@field SolidCollidable boolean
---@field Grounded boolean
---@field SetSolidCollisionActive fun(self: AttackActor, value: boolean)
---@field SetEffectiveGravityMult fun(self: AttackActor, mult: number)
entities.AttackActor = {}

---@class BossController : Entity
---@field Health integer
---@field IsActing boolean *
---@field CurrentPatternIndex integer
---@field CurrentPatternName string *
---@field Random System.Random
---@field AddEntity fun(self: BossController, entity: Entity) *
---@field DestroyEntity fun(self: BossController, entity: Entity) *
---@field DestroyAll fun(self: BossController)
---@field InterruptPattern fun(self: BossController)
---@field GetPatternIndex fun(self: BossController, name: string): integer *
---@field StartAttackPattern fun(self: BossController, index: number)
---@field ForceNextAttack fun(self: BossController, index: number) *
---@field SavePhaseChangeToSession fun(self: BossController, health: integer, index: integer, startImmediately: boolean) *
---@field RemoveBoss fun(self: BossController, permanent: boolean) *
---@field StoreObject fun(self: BossController, key: string, value: any) *
---@field GetStoredObject fun(self: BossController, key: string): any *
---@field DeleteStoredObject fun(self: BossController, key: string) *
---@field DecreaseHealth fun(self: BossController, amount?: integer)
entities.BossController = {}

---@class BossPuppet : Actor
---@field Speed Vector2
---@field Grounded boolean
---@field gravityMult number
---@field groundFriction number
---@field airFriction number
---@field Sprite Sprite
---@field SolidCollidable boolean
---@field BossHitCooldown number
---@field BossDamageCooldown Stopwatch
---@field PlayBossAnim fun(self : BossPuppet, anim: string)
---@field Set1DSpeedDuring fun(self: BossPuppet, speed: number, isX: boolean, time: number) *
---@field Speed1DTween fun(self: BossPuppet, start: number, target: number, time: number, isX: boolean, easer?: Ease.Easer) *
---@field ChangeHitboxOption fun(self : BossPuppet, tag: string) *
---@field ChangeHurtboxOption fun(self : BossPuppet, tag: string) *
---@field ChangeBounceboxOption fun(self : BossPuppet, tag: string) *
---@field ChangeTargetOption fun(self : BossPuppet, tag: string) *
entities.BossPuppet = {}

return entities