--#region Bosses Helper functions

--- Bosses Helper Attack specific functions and helper methods for the player

---@alias Entity Entity A Monocle Entity object.
---@alias Vector2 Vector2 A Vector2 object.
---@alias Component Component A Monocle Component object.
---@alias Collider Collider A Monocle Collider object.
---@alias ColliderList ColliderList A Monocle ColliderList object, combining multiple Colliders.
---@alias EntityData EntityData An Everest EntityData object.
---@alias Easer Easer A Monocle Easer, used for Tweens.
---@alias Action Action A C# Action, a delegate void object.
---@alias Func Func A C# Func, a delegate object object.
---A specific Easer can be obtained by calling "monocle.Ease.{name}" which returns the desired Easer.

local function getEaserByName(name, invert)
  local ease = require("#monocle.ease")
  local typ = type(name)
  if (typ ~= "string") then
      return nil
  end
  local value = string.lower(name)
  local easers = {
      linear = ease.Linear,
      sinein = ease.SineIn,
      sineout = ease.SineOut,
      sineinout = ease.SineInOut,
      quadin = ease.QuadIn,
      quadout = ease.QuadOut,
      quadinout = ease.QuadInOut,
      cubein = ease.CubeIn,
      cubeout = ease.CubeOut,
      cubeinout = ease.CubeInOut,
      quintin = ease.QuintIn,
      quintout = ease.QuintOut,
      QuintInOut = ease.QuintInOut,
      expoin = ease.ExpoIn,
      expoout = ease.ExpoOut,
      expoinout = ease.ExpoInOut,
      backin = ease.BackIn,
      backout = ease.BackOut,
      backinout = ease.BackInOut,
      bigbackin = ease.BigBackIn,
      bigbackout = ease.BigBackOut,
      bigbackinout = ease.BigBackInOut,
      elasticin = ease.ElasticIn,
      elasticout = ease.ElasticOut,
      elasticinout = ease.ElasticInOut,
      bouncein = ease.BounceIn,
      bounceout = ease.BounceOut,
      bounceinout = ease.BounceInOut,
      default = nil
  }
  if easers[value] then
      if invert or false then
          return ease.Invert(easers[value])
      end
      return easers[value]
  else
      return easers["default"]
  end
end

--#region Delegates

--- These will not work outside of the files specified due to being passed by reference.
--- Since no reference to the Controller is given, these function delegates are necessary.

--#region Attack Delegates

--- The following Delegates will only work on Attack files.

function helpers.bossSeededRandom()
  return boss.Random:Next()
end

--- Adds the provided entity onto the scene, as well as into the Boss' tracked entities.
---@param entity Entity The entity to add
function helpers.addEntity(entity)
  boss:AddEntity(entity)
end

--- Calls RemoveSelf on the entity provided, as well as removing it from the tracked entities.
---@param entity Entity The entity to destroy.
function helpers.destroyEntity(entity)
  boss:DestroyEntity(entity)
end

--- Calls RemoveSelf on all active tracked entities.
function helpers.destroyAll()
  boss:DestroyAll()
end

--#endregion


--#region Interrupt Delegates

--- The following Delegates will only work on the Interruption functions, such as onHit()

function helpers.seededRandom()
  return boss.Random:Next()
end

---Get the Boss' current health value
---@return number health The Boss's current health value
function helpers.getHealth()
  return boss:GetHealth()
end

---Set the Boss' health value to a new value.
---@param health number The value to set the health to.
function helpers.setHealth(health)
  boss:SetHealth(health)
end

---Decrease the Boss' health by the given value
---@param health? number The amount of health lost. Defaults to 1.
function helpers.decreaseHealth(health)
  boss:DecreaseHealth(health or 1)
end

--- Wait for the current attack coroutine to end
function helpers.waitForAttackToEnd()
  return coroutine.yield(boss:WaitForAttack())
end

---Interrupt the current boss action pattern
function helpers.interruptPattern()
  boss:InterruptPattern()
end

---Gets the currently set pattern index
---@return number ID The current pattern's index, base 0
function helpers.getCurrentPatternID()
  return boss:GetCurrentPatternIndex()
end

---Start a new boss action pattern.
---@param goTo? number The pattern index to start executing. Defaults to -1, which will start the currently set pattern again.
function helpers.startAttackPattern(goTo)
  boss:StartAttackPattern(goTo or -1)
end

---Start the next action pattern in index order.
function helpers.startNextAttackPattern()
  helpers.startAttackPattern(helpers.getCurrentPatternID() + 1)
end

---Force the next attack to be the attack of the given index. Index is based off of position within the Pattern.
---Currently only supported in Random Patterns. The index is always ran past a modulo on the pattern attacks' count to avoid an out-of-bounds issue.
---@param index number The attack index to select next. Will only take effect once per call.
function helpers.forceNextAttackIndex(index)
  boss:ForceNextAttackIndex(index)
end

---Saves certain values to the Mod's Session so they are stored on Retry and even on Save and Quit. These values will be fetched by the controller automatically when loaded back into the level.
---@param health number The health value to save and set back upon reload.
---@param index number The pattern index the boss should start with upon reload.
---@param startImmediately? boolean If the Boss should start the defined action pattern immediately instead of waiting for the player to move. Defaults to false.
function helpers.savePhaseChangeToSession(health, index, startImmediately)
  boss:SavePhaseChangeToSession(health or helpers.getHealth(), index or helpers.GetCurrentPatternID(), startImmediately or false)
end

---Removes the Boss from the scene, alongside its puppet and any Entities spawned by it.
---This function also Works in Cutscene files
---@param permanent boolean If the boss should not be loaded again. False will spawn the Boss every time the room is loaded.
function helpers.removeBoss(permanent)
  boss:RemoveBoss(permanent or false)
end

--#endregion



--#region Other Helper Functions

--- Additional, non-delegate helper shorthand methods
--- These can be used anywhere within the Mod.

---Plan an animation on the Boss' given sprite
---@param anim string The animation to play
function helpers.playPuppetAnim(anim)
  puppet:PlayBossAnim(anim)
end

--#region Position and Movement

--- Set the gravity multiplier to the fiven value. Gravity constant is 900.
--- @param mult number The multiplier to apply to the Gravity constant which the Boss will use.
function helpers.setEffectiveGravityMult(mult)
  puppet:SetGravityMult(mult)
end

---Set the Boss' x speed to the given value
---@param value number The value to set the Boss' speed x component to.
function helpers.setXSpeed(value)
  puppet:SetXSpeed(value)
end

---Set the Boss' y speed to the given value
---@param value number The value to set the Boss' speed y component to.
function helpers.setYSpeed(value)
  puppet:SetYSpeed(value)
end

---Set the Boss' speed to the given values
---@param x number The value to set the Boss' speed x component to.
---@param y number The value to set the Boss' speed y component to.
function helpers.setSpeed(x, y)
  puppet:SetSpeed(x, y)
end

---Set the Boss' x speed to the given value, kept constant during the given time.
---@param value number The value to set the Boss' speed x component to.
---@param time number The time to hold the value for.
---@return number time The time given from the Tween
function helpers.setXSpeedDuring(value, time)
  return puppet:SetXSpeedDuring(value, time)
end

---Set the Boss' y speed to the given value, kept constant during the given time.
---@param value number The value to set the Boss' speed y component to.
---@param time number The time to hold the value for.
---@return number time The time given from the Tween
function helpers.setYSpeedDuring(value, time)
  return puppet:SetYSpeedDuring(value, time)
end

---Set the Boss' speed to the given values, kept constant during the given time.
---@param x number The value to set the Boss' speed x component to.
---@param y number The value to set the Boss' speed y component to.
---@param time number The time to hold the values for.
---@return number time The time given from the Tween
function helpers.setSpeedDuring(x, y, time)
  return puppet:SetSpeedDuring(x, y, time)
end

---Keep the Boss' current x speed constant during the given time.
---@param time number The time to hold the value for.
---@return number time The time given from the Tween
function helpers.keepXSpeedDuring(time)
  return puppet:SetXSpeedDuring(puppet.Speed.X, time)
end

---Keep the Boss' current y speed constant during the given time.
---@param time number The time to hold the value for.
---@return number time The time given from the Tween
function helpers.keepYSpeedDuring(time)
  return puppet:SetYSpeedDuring(puppet.Speed.Y, time)
end

---Keep the Boss' current speed constant during the given time.
---@param time number The time to hold the values for.
---@return number time The time given from the Tween
function helpers.keepSpeedDuring(time)
  return puppet:SetSpeedDuring(puppet.Speed.X, puppet.Speed.Y, time)
end

---Create a new Position Tween, which will slowly move the Boss to the target.
---@param target Vector2 The vector2 target position the Boss will move towards.
---@param time number The time the Boss will take to reach the target.
---@param easer? string|Easer The easer to apply to the motion. Defaults to nil.
---@param invert? boolean If the easer should be inverted
---@return number time The time given from the Tween
function helpers.positionTween(target, time, easer, invert)
  return puppet:PositionTween(target, time, getEaserByName(easer, invert) or easer)
end

---Create a new Tween for the Boss' x speed.
---@param start number The initial value of the Tween, which the Boss' speed x component will set to at the start.
---@param target number The value the Boss' speed x component will slowly change to.
---@param time number The time the Boss will take to reach the target x speed.
---@param easer? string|Easer The easer to applt to the x speed value. Defaults to nil.
---@param invert? boolean If the easer should be inverted
---@return number time The time given from the Tween
function helpers.speedXTween(start, target, time, easer, invert)
  return puppet:SpeedXTween(start, target, time, getEaserByName(easer, invert) or easer)
end

---Create a new Tween for the Boss' y speed.
---@param start number The initial value of the Tween, which the Boss' speed y component will set to at the start.
---@param target number The value the Boss' speed y component will slowly change to.
---@param time number The time the Boss will take to reach the target y speed.
---@param easer? string|Easer The easer to applt to the y speed value. Defaults to nil.
---@param invert? boolean If the easer should be inverted
---@return number time The time given from the Tween
function helpers.speedYTween(start, target, time, easer, invert)
  return puppet:SpeedYTween(start, target, time, getEaserByName(easer, invert) or easer)
end

---Create a new Tween for the Boss' speed.
---@param xStart number The initial value of the Tween, which the Boss' speed x component will set to at the start.
---@param xTarget number The value the Boss' speed x component will slowly change to.
---@param yStart number The initial value of the Tween, which the Boss' speed y component will set to at the start.
---@param yTarget number The value the Boss' speed y component will slowly change to.
---@param time number The time the Boss will take to reach the target x speed.
---@param easer? string|Easer The easer to applt to the x speed value. Defaults to nil.
---@param invert? boolean If the easer should be inverted
---@return number time The time given from the Tween
function helpers.speedTween(xStart, yStart, xTarget, yTarget, time, easer, invert)
  return puppet:SpeedTween(xStart, yStart, xTarget, yTarget, time, getEaserByName(easer, invert) or easer)
end

---Create a new Tween for the Boss' x speed from its current x speed value.
---@param target number The value the Boss' speed x component will slowly change to.
---@param time number The time the Boss will take to reach the target x speed.
---@param easer? string|Easer The easer to applt to the x speed value. Defaults to nil.
---@param invert? boolean If the easer should be inverted
---@return number time The time given from the Tween
function helpers.speedXTweenTo(target, time, easer, invert)
  return puppet:SpeedXTween(puppet.Speed.X, target, time, getEaserByName(easer, invert) or easer)
end

---Create a new Tween for the Boss' x speed from its current y speed value.
---@param target number The value the Boss' speed y component will slowly change to.
---@param time number The time the Boss will take to reach the target y speed.
---@param easer? string|Easer The easer to applt to the y speed value. Defaults to nil.
---@param invert? boolean If the easer should be inverted
---@return number time The time given from the Tween
function helpers.speedYTweenTo(target, time, easer, invert)
  return puppet:SpeedYTween(puppet.Speed.Y, target, time, getEaserByName(easer, invert) or easer)
end

---Create a new Tween for the Boss'  speed from its current x speed value.
---@param xTarget number The value the Boss' speed x component will slowly change to.
---@param yTarget number The value the Boss' speed y component will slowly change to.
---@param time number The time the Boss will take to reach the target x speed.
---@param easer? string|Easer The easer to applt to the x speed value. Defaults to nil.
---@param invert? boolean If the easer should be inverted
---@return number time The time given from the Tween
function helpers.speedTweenTo(xTarget, yTarget, time, easer, invert)
  return puppet:SpeedTween(puppet.Speed.X, puppet.Speed.Y, xTarget, yTarget, time, getEaserByName(easer, invert) or easer)
end

--#endregion

--#region Collisions and Colliders

--- Enable the Boss' Collision checks from other entities.
function helpers.enableCollisions()
  puppet.Collidable = true
end

--- Disable the Boss' Collision checks from other entities.
function helpers.disableCollisions()
  puppet.Collidable = false
end

--- Enable the Boss' Collision checks with solids.
function helpers.enableSolidCollisions()
  puppet.SolidCollidable = true
end

--- Disable the Boss' Collision checks with solids.
function helpers.disableSolidCollisions()
  puppet.SolidCollidable = false
end

---Set the Boss' hit cooldown to the given value
---@param value number The timer to set the cooldown to
function helpers.setHitCooldown(value)
  puppet:SetBossHitCooldown(value)
end

---Set the Boss' hit cooldown back to the default value defined.
function helpers.resetHitCooldown()
  puppet:ResetBossHitCooldown()
end

---Change the Boss' hitboxes to those stored under the given tag.
---@param tag string The hitbox group tag to use.
function helpers.changeBaseHitboxTo(tag)
  puppet:ChangeHitboxOption(tag)
end

---Change the Boss' hurtboxes to those stored under the given tag.
---@param tag string The hurtbox group tag to use.
function helpers.changeHurtboxTo(tag)
  puppet:ChangeHurtboxOption(tag)
end

---Change the Boss' bouncebox to that stored under the given tag.
---@param tag string The bouncebox tag to use.
function helpers.changeBounceboxTo(tag)
  puppet:ChangeHitboxOption(tag)
end

---Change the Boss' Sidekick Target to that stored under the given tag.
---@param tag string The Sidekick Target tag to use.
function helpers.changeTargetTo(tag)
  puppet:ChangeHurtboxOption(tag)
end

---Create a new Rectangular Hitbox Collider
---@param width number The width of the collider.
---@param height number The height of the collider.
---@param x number The x offset of the hitbox. Defaults to 0.
---@param y number The y offest of the Hitbox. Defaults to 0.
---@return Collider hitbox The created Hitbox Collider
function helpers.getHitbox(width, height, x, y)
  return monocle.Hitbox(width, height, x or 0, y or 0)
end

---Create a new Circle Collider
---@param radius number The radius of the collider.
---@param x number The x offset of the hitbox. Defaults to 0.
---@param y number The y offest of the Hitbox. Defaults to 0.
---@return Collider circle The created Hitbox Collider
function helpers.getCircle(radius, x, y)
  return monocle.Circle(radius, x or 0, y or 0)
end

---Create a ColliderList object from the provided colliders.
---@param ... Collider All the colliders to combine into a ColliderList
---@return ColliderList colliderList The combined ColliderList object.
function helpers.getColliderList(...)
  return celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper.GetColliderListFromLuaTable({...})
end

--#endregion

--#region Boss Components and Entities

---Add a component to the Boss.
---@param component Component The component to add.
function helpers.addComponentToBoss(component)
  puppet:Add(component)
end

---Wrap a function in another function to call the inner one with parameters but the outer one without.
---@param func function The function to wrap.
---@param ... any The parameters to call func with.
---@return function function Function that will wrap the passed function with the arguements passed.
local function callFunc(func, ...)
  local args = {...}
  if select("#", ...) < 1 then
      return func
  end
  return function ()
      return func(table.unpack(args))
  end
end

---Add a function that will run in the background.
---@param func fun(...) The function that will run in the background. Will run to completion or loop as defined.
---@param ... any Parameters to pass to the wrapped function, if any
function helpers.addConstantBackgroundCoroutine(func, ...)
  celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper.AddConstantBackgroundCoroutine(puppet, callFunc(func, {...}))
end

local function killPlayer(entity, player)
  helpers.die(helpers.normalize(player.Position - entity.Position))
end

---Returns an EntityChecker Component that will execute the second passed function when the first function's return value matches the state required.
---@param checker fun() The function that will be called every frame to test its value.
---@param func? fun(entity: Entity) The function that will execute once the timer ends. Takes an entity parameter, which will be the Entity the component is added to. Defaults to the DestroyEntity function.
---@param state? boolean The state the checker function's return value must match. Defaults to true.
---@param remove? boolean If the component should remove itself after it calls the func function. Defaults to true
---@return Component checker The Entity Checker that can be added to any Entity.
function helpers.getEntityChecker(checker, func, state, remove)
  return celeste.Mod.BossesHelper.Code.Components.EntityChecker(checker, func or helpers.destroyEntity, state or state == nil, remove or remove == nil)
end

---Returns an EntityTimer Component that will execute the passed function when the timer ends.
---Can be added to any Entity.
---@param timer number The amount of time that must pass for the timer to execute.
---@param func? fun(entity: Entity) The function that will execute once the timer ends. Takes an entity parameter, which will be the Entity the component is added to. Defaults to the DestroyEntity function.
---@return Component timer The Entity Timer that can be added to any Entity.
function helpers.getEntityTimer(timer, func)
  return celeste.Mod.BossesHelper.Code.Components.EntityTimer(timer, func or helpers.destroyEntity)
end

---Returns an EntityFlagger Component that will execute the passed function when the given session flag's state matches the required state.
---Can be added to any Entity.
---@param flag string The session flag the entity will use to activate its function.
---@param func? fun(entity: Entity) The function that will execute once the session flag state is the same as the state parameter. Takes an entity parameter, which will the Entity the component is added to. Defaults to the destroyEntity function.
---@param state? boolean The state the flag must match to activate the passed function. Defaults to true.
---@param resetFlag? boolean If the flag should return to its previous state once used by the Flagger. Defaults to true
---@return Component flagger The Entity Flagger that can be added to any Entity.
function helpers.getEntityFlagger(flag, func, state, resetFlag)
  return celeste.Mod.BossesHelper.Code.Components.EntityFlagger(flag, func or helpers.destroyEntity, state or state == nil, resetFlag or resetFlag == nil)
end

---Returns an EntityChain component that will keep another entity's position chained to the Entity this component is added to.
---@param entity Entity The entity to chain, whose position will change as the base Entity moves.
---@param startChained? boolean Whether the entity should start chained immediately. Defaults to true.
---@param remove? boolean Whether the chained entity should be removed if the chain component is also removed.
---@return Component the Entity Chain component that can be added to any Entity.
function helpers.getEntityChain(entity, startChained, remove)
  return celeste.Mod.BossesHelper.Code.Components.EntityChain(entity, startChained or startChained == nil, remove or false)
end

---Create and return a basic entity to use in attacks.
---@param position Vector2 The position the entity will be at.
---@param hitboxes Collider The collider the entity will use.
---@param spriteName string The sprite the entity will use.
---@param startCollidable? boolean If the entity should spawn with collisions active. Defaults to true.
---@param funcOnPlayer? fun(self, player) The function that will be called when the entity "self" collides with the Player. Defaults to killing the Player.
---@param xScale? number The horizontal sprite scale. Defaults to 1.
---@param yScale? number The vertical sprite scale. Defaults to 1.
function helpers.getNewBasicAttackEntity(position, hitboxes, spriteName, startCollidable, funcOnPlayer, xScale, yScale)
  return celeste.Mod.BossesHelper.Code.Entities.AttackEntity(position, hitboxes, funcOnPlayer or killPlayer, startCollidable or startCollidable==nil, spriteName, xScale or 1, yScale or 1)
end

---Create and return a basic entity to use in attacks.
---@param position Vector2 The position the entity will be at.
---@param hitboxes Collider The collider the entity will use.
---@param spriteName string The sprite the entity will use.
---@param gravMult? number The multiplier to the Gravity constant the Actor should use.
---@param maxFall? number The fastest the Boss will fall naturally due to gravity.
---@param startCollidable? boolean If the entity should spawn with collisions active. Defaults to true.
---@param funcOnPlayer? fun(self, player) The function that will be called when the entity "self" collides with the Player. Defaults to killing the Player.
---@param xScale? number The horizontal sprite scale. Defaults to 1.
---@param yScale? number The vertical sprite scale. Defaults to 1.
function helpers.getNewBasicAttackActor(position, hitboxes, spriteName, gravMult, maxFall, startCollidable, funcOnPlayer,  xScale, yScale)
  return celeste.Mod.BossesHelper.Code.Entities.AttackActor(position, hitboxes, funcOnPlayer or killPlayer, startCollidable or startCollidable==nil, spriteName, gravMult or 1, maxFall or 90, xScale or 1, yScale or 1)
end

--#endregion

--#region Component Retreival

--- Gets all tracked components by class name.
--- @param name string Class name of the component, relative to "Celeste." by default.
--- @param prefix? string Overrides the global class name prefix.
--- @return Component[] components Tracked components of given class.
function helpers.getComponents(name, prefix)
  return celeste.Mod[modName].Code.Helpers.LuaMethodWrappers.GetComponents(name, prefix or classNamePrefix)
end

--- Gets the first tracked component by class name.
--- @param name string Class name of the component, relative to "Celeste." by default.
--- @param prefix? string Overrides the global class name prefix.
--- @return Component component First tracked component of given class.
function helpers.getComponent(name, prefix)
  return celeste.Mod[modName].Code.Helpers.LuaMethodWrappers.GetComponent(name, prefix or classNamePrefix)
end

--- Gets all components by class name.
--- @param name string Class name of the component, relative to "Celeste." by default.
--- @param prefix? string Overrides the global class name prefix.
--- @return Component[] components All components of given class on scene.
function helpers.getAllComponents(name, prefix)
  return celeste.Mod[modName].Code.Helpers.LuaMethodWrappers.GetAllComponents(name, prefix or classNamePrefix)
end

--- Gets the first component by class name.
--- @param name string Class name of the component, relative to "Celeste." by default.
--- @param prefix? string Overrides the global class name prefix.
--- @return Component component First component of given class.
function helpers.getFirstComponent(name, prefix)
  return celeste.Mod[modName].Code.Helpers.LuaMethodWrappers.GetFirstComponent(name, prefix or classNamePrefix)
end

--- Gets all components by class name added to an entity of given class name.
--- @param name string Class name of the component, relative to "Celeste." by default.
--- @param entity string Class name of the entity, relative to "Celeste." by default.
--- @param prefix? string Overrides the global class name prefix for the Component class.
--- @param entityPre? string Overrides the global class name prefix for the Entity class.
--- @return Component[] components All components of given class on scene attached to the entity type.
function helpers.getAllComponentsOnType(name, entity, prefix, entityPre)
  return celeste.Mod[modName].Code.Helpers.LuaMethodWrappers.GetAllComponentsOnType(name, entity, prefix or classNamePrefix, entityPre or classNamePrefix)
end

--- Gets the first component by class name added to an entity of the given class name.
--- @param name string Class name of the component, relative to "Celeste." by default.
--- @param entity string Class name of the entity, relative to "Celeste." by default.
--- @param prefix? string Overrides the global class name prefix for the Component class.
--- @param entityPre? string Overrides the global class name prefix for the Entity class.
--- @return Component component First component of given class attached to the entity type.
function helpers.getFirstComponentOnType(name, entity, prefix, entityPre)
  return celeste.Mod[modName].Code.Helpers.LuaMethodWrappers.GetFirstComponentOnType(name, entity, prefix or classNamePrefix, entityPre or classNamePrefix)
end

--- Returns all the components of the given class name from the entity given, if any.
--- @param entity Entity The entity to check.
--- @param name string Class name of the component, relative to "Celeste." by default.
--- @param prefix? string Overrides the global class name prefix.
--- @return Component[] components All components of given class on scene sored on the entity, if any.
function helpers.getComponentsFromEntity(entity, name, prefix)
  return celeste.Mod[modName].Code.Helpers.LuaMethodWrappers.GetComponentsFromEntity(entity, name, prefix or classNamePrefix)
end

--- Returns the component of the given class name from the entity given, if any.
--- @param entity Entity The entity to check.
--- @param name string Class name of the component, relative to "Celeste." by default.
--- @param prefix? string Overrides the global class name prefix.
--- @return Component component First component of given class stored on the entity, if any.
function helpers.getComponentFromEntity(entity, name, prefix)
  return celeste.Mod[modName].Code.Helpers.LuaMethodWrappers.GetComponentFromEntity(entity, name, prefix or classNamePrefix)
end

--- Checks if the entity given has a component of the given class name.
--- @param entity Entity The entity to check.
--- @param name string Class name of the component, relative to "Celeste." by default.
--- @param prefix? string Overrides the global class name prefix.
--- @return boolean componentFound If the Entity does have a Component of the type specified.
function helpers.entityHasComponent(entity, name, prefix)
  return celeste.Mod[modName].Code.Helpers.LuaMethodWrappers.EntityHasComponent(entity, name, prefix or classNamePrefix)
end

--#endregion

--#region Health System

---Get the Player's current health value on the active Health System
---@return number health The player's health value, or -1 if there's no active Health System
function helpers.getPlayerHealth()
  local controller = celeste.Mod.BossesHelper.BossesHelperModule.Session.mapDamageController
  if controller then
     return controller.health
  end
  return -1
end

---Gives additional time where the player is invincible to taking damage.
---@param time number The time to add to the invincible timer.
function helpers.giveInvincibleFrames(time)
  celeste.Mod.BossesHelper.BossesHelperModule.GiveIFrames(time)
end

--#endregion

--#region Misc. Functions

---Get a new EntityData object
---@param position Vector2 The vector2 position the entityData will hold.
---@param width? number The width the EntityData will hold. Defaults to 0.
---@param height? number The height the EntityData will hold. Defaults to 0.
---@param id? number The id the EntityData will hold. Defaults to 1000.
---@return EntityData entityData The formed EntityData object with the Values dictionary initialized empty.
function helpers.getNewEntityData(position, width, height, id)
  newData = celeste.Mod.BossesHelper.BossesHelperModule.MakeEntityData()
  newData.ID = id or 1000
  newData.Level = engine.Scene
  newData.Position = position or vector2(0,0)
  newData.Width = width or 0
  newData.Height = height or 0
  return newData
end

---Set a list of attributes to the provided EntityData object's Values dictionary.
---@param entityData EntityData The EntityData to update.
---@param attributes table<string,any> The list of attributes to add.
function helpers.setEntityDataAttributes(entityData, attributes)
  for k,v in pairs(attributes) do
      entityData.Values:Add(k,v)
  end
end

---Store any object within the Boss under a specific key, to be retreived later.
---@param key string The key to store the object with. Must be unique, or the object will not be stored.
---@param object any The object to store
function helpers.storeObjectInBoss(key, object)
  puppet:StoreObject(key, object)
end

---Return an item that was stored within the Boss by key.
---@param key string The key the object is stored under.
---@return nil|any object The object stored, or nil if key is not found.
function helpers.getStoredObjectFromBoss(key)
  return puppet:GetStoredObject(key)
end

---Remove the object stored under the specified key from the Boss' stored objects.
---@param key string The key the object is stored under.
function helpers.deleteStoredObjectFromBoss(key)
  puppet:DeleteStoredObject(key)
end

---Set a method that will execute after a given delay.
---@param func fun() The function to execute. Takes no parameters.
---@param delay number The time in seconds the function will be called after.
function helpers.doMethodAfterDelay(func, delay)
  celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper.DoMethodAfterDelay(func, delay)
end

---Return a Lua function as a C# delegate
---@param func function Function to return as a delegate
---@return Action Action The delegate that will call the function when invoked
function helpers.functionToAction(func)
  return celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper.LuaFunctionToAction(func)
end

---Return a Lua function as a C# delegate
---@param func function Function to return as a delegate
---@return Func Action The delegate that will call the function when invoked
function helpers.functionToFunc(func)
  return celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper.LuaFunctionToFunc(func)
end

---Get the length of the provided vector2
---@param vector Vector2 Vector to get length of
---@return number length The length of the vector2
function helpers.v2L(vector)
  return math.sqrt(vector.X * vector.X + vector.Y * vector.Y)
end

---Normalizes the vector provided to the given length or 1.
---@param vector Vector2 The vector to normalize
---@param length? number The new length of the vector or 1
---@return Vector2 normal The normalized vector2
function helpers.normalize(vector, length)
  local len = helpers.v2L(vector)
  if length and length <= 0 then return vector2(0, 0) end

  if len == 0 then return vector end
  return vector2(vector.X / len, vector.Y / len) * (length or 1)
end

--#endregion

--#endregion

--#endregion

--#endregion
