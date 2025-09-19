---@meta Celeste.Mod.BossesHelper.Code.Entities

---@class Entities
local entities = {}

--#region AttackEntity
---@class AttackEntity : Entity
---@overload fun(position: Vector2, hitboxes: Collider, funcOnPlayer: fun(self: Entity, player: Player), startCollidable: boolean, spriteName: string, xScale?: number, yScale?: number) : AttackEntity
---@field Sprite Sprite
entities.AttackEntity = {}

---Play an animation on the attached Sprite.
---@param anim string The animation to play
function entities.AttackEntity:PlayAnim(anim) end

---Change the Collidable state.
---@param state boolean
function entities.AttackEntity:SetCollisionActive(state) end
--#endregion

--#region AttackActor
---@class AttackActor : Actor
---@overload fun(position: Vector2, hitboxes: Collider, funcOnPlayer: fun(self: Entity, player: Player), startCollidable: boolean, startSolidCollidable: boolean, spriteName: string, gravMult: number, maxFall: number, xScale?: number, yScale?: number): AttackActor
---@field Sprite Sprite
---@field Speed Vector2
---@field GravityMult number
---@field SolidCollidable boolean
---@field Grounded boolean
entities.AttackActor = {}

---Play an animation on the attached Sprite.
---@param anim string The animation to play
function entities.AttackActor:PlayAnim(anim) end

---Change the Collidable state.
---@param state boolean
function entities.AttackActor:SetCollisionActive(state) end

---Change the SolidCollidable state.
---@param state boolean
function entities.AttackActor:SetSolidCollisionActive(state) end

---Set the Actor's active gravity multiplier.
---@param mult number
function entities.AttackActor:SetEffectiveGravityMult(mult) end
--#endregion

--#region BossController
---@class BossController : Entity
---@field Health integer
---@field IsActing boolean *
---@field CurrentPatternIndex integer
---@field CurrentPatternName string *
---@field Random Random
entities.BossController = {}

---Add an Entity to the scene tracked by the Boss.
---@param entity Entity
function entities.BossController:AddEntity(entity) end

---Remove an Entity from the scene that the Boss is tracking.
---@param entity Entity The entity to destroy/remove.
function entities.BossController:DestroyEntity(entity) end

---Remove all entities tracked by the Boss from the Scene.
function entities.BossController:DestroyAll() end

---Interrupt the currently executing Pattern.
function entities.BossController:InterruptPattern() end

---Get the index of the pattern with the given name.
---@param name string The name of the pattern to search.
---@return integer # The index of the pattern, or -1 if not found.
function entities.BossController:GetPatternIndex(name) end

---Start the pattern with the given index.
---@param index number
function entities.BossController:StartAttackPattern(index) end

---Force the next attack to be the one found by the given index in the current pattern.
---Only applicable to Random patterns.
---@param index number The attack's index to use next.
function entities.BossController:ForceNextAttack(index) end

---Save the current state of the Boss to Session, such that reloads will load the Boss with the given values.
---It allows to store the Boss's Health value, the pattern to start at, and whether it should start attacking immediately.
---@param health integer The Health value to set the Boss to on reloads.
---@param index integer The Pattern to start at on reloads.
---@param startImmediately boolean Whether the Boss should start its pattern attacks immediately.
function entities.BossController:SavePhaseChangeToSession(health, index, startImmediately) end

---Remove the Boss from the scene.
---@param permanent boolean Whether the Boss should not load again on scene reloads.
function entities.BossController:RemoveBoss(permanent) end

---Store an object/value in the Boss to reference later.
---@param key string The key to store the object under.
---@param object any The object/value to store.
function entities.BossController:StoreObject(key, object) end

---Get an object/value stored in the Boss.
---@param key string The key the object/value was stored under.
function entities.BossController:GetStoredObject(key) end

---Remove an onject/value from the Boss's storage.
---@param key string The key of the object/value to remove.
function entities.BossController:DeleteStoredObject(key) end

---Decrease the Boss's health by the amount.
---@param amount integer
function entities.BossController:DecreaseHealth(amount) end
--#endregion

--#region BossPuppet
---@class BossPuppet : Actor
---@field Speed Vector2
---@field Grounded boolean
---@field gravityMult number
---@field groundFriction number
---@field airFriction number
---@field killOnContact boolean
---@field Sprite Sprite
---@field SolidCollidable boolean
---@field BossHitCooldown number
---@field BossDamageCooldown Stopwatch
entities.BossPuppet = {}

---Play an animation on the Boss's Sprite.
---@param anim string The animation to play.
function entities.BossPuppet:PlayAnim(anim) end

---Maintain a component of the Boss's speed to the value during the time given. 
---@param speed number The speed component's value to maintain.
---@param isX boolean Whether to affect the x component of the speed.
---@param time number The time the value should be maintained.
---@return IEnumerator
function entities.BossPuppet:Keep1DSpeed(speed, isX, time) end

---Set a component of the Boss's speed to the value, kept during the time given.
---@param speed number The speed component's value to maintain.
---@param isX boolean Whether to affect the x component of the speed.
---@param time number The time the value should be maintained.
function entities.BossPuppet:Set1DSpeedDuring(speed, isX, time) end

---Create a Tween to transition one of the Boss's speed component.
---@param start number The starting speed value.
---@param target number The target speed value.
---@param time number The time the transition should take.
---@param isX boolean Whether to affect the x component of the speed.
---@param easer? Ease.Easer The Easer to transition the speed value.
function entities.BossPuppet:Speed1DTween(start, target, time, isX, easer) end

---Change the Boss's Hitbox to the one specified.
---@param tag string The Hitbox group tag to change to.
function entities.BossPuppet:ChangeHitboxOption(tag) end

---Change the Boss's Hurtbox to the one specified.
---@param tag string The Hurtbox group tag to change to.
function entities.BossPuppet:ChangeHurtboxOption(tag) end

---Change the Boss's Bouncebox to the one specified.
---@param tag string The Bouncebox group tag to change to.
function entities.BossPuppet:ChangeBounceboxOption(tag) end

---Change the Boss's Target to the one specified.
---@param tag string The Target group tag to change to.
function entities.BossPuppet:ChangeTargetOption(tag) end
--#endregion

return entities