# [Bosses Helper](README.md): Lua Helper Functions

## [Document Layout](boss_helper_functions_layout.md#bosses-helper-lua-helper-functions-layout)

Find the actual Lua file [here](Assets/LuaBossHelper/helper_functions.lua).

## Entity Adding

### helpers.addEntity (entity)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Adds the provided entity onto the scene, as well as into the Boss' tracked entities.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`entity` (`Entity`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The entity to add  

---

### helpers.destroyEntity (entity)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Calls RemoveSelf on the entity provided, as well as removing it from the tracked entities.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`entity` (`Entity`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The entity to destroy.  

---

### helpers.destroyAll ()

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Calls RemoveSelf on all active tracked entities.

---

## Fight Logic

### helpers.playPuppetAnim (anim)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Plan an animation on the Boss's given sprite

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`anim` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The animation to play  

---

### helpers.playAndWaitPuppetAnim (anim)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Play an animation on the Boss's given sprite and wait for it to complete one full cycle.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`anim` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The animation to play  

---

### helpers.seededRandom ()

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Get a random number based on the boss's random seed.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`next` (`integer`): A seeded-random integer.

---

### helpers.getHealth ()

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Get the Boss' current health value

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`health` (`integer`): The Boss's current health value

---

### helpers.setHealth (health)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Set the Boss' health value to a new value.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`health` (`integer`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The value to set the health to.  

---

### helpers.decreaseHealth ([health=1])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Decrease the Boss' health by the given value

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`health` (`integer`) (default `1`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The amount of health lost. Defaults to 1.  

---

### helpers.waitForAttackToEnd ()

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Wait for the current attack coroutine to end

---

### helpers.interruptPattern ()

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Interrupt the current boss action pattern

---

### helpers.getCurrentPatternID ()

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Gets the currently set pattern index

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`ID` (`integer`): The current pattern's index, base 0

---

### helpers.startAttackPattern ([goTo=-1])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Start a new boss action pattern.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`goTo` (`integer`) (default `-1`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The pattern index to start executing. Defaults to -1, which will start the currently set pattern again.  

---

### helpers.startNextAttackPattern ()

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Start the next action pattern in index order.

---

### helpers.forceNextAttackIndex (index)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Force the next attack to be the attack of the given index. Index is based off of position within the Pattern.
Currently only supported in Random Patterns. The index is always ran past a modulo on the pattern attacks' count to avoid an out-of-bounds issue.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`index` (`integer`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The attack index to select next. Will only take effect once per call.  

---

### helpers.savePhaseChangeToSession (health, index[, startImmediately=true])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Saves certain values to the Mod's Session so they are stored on Retry and even on Save and Quit. These values will be fetched by the controller automatically when loaded back into the level.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`health` (`integer`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The health value to save and set back upon reload.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`index` (`integer`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The pattern index the boss should start with upon reload.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`startImmediately` (`boolean`) (default `true`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;If the Boss should start the defined action pattern immediately instead of waiting for the player to move. Defaults to false.  

---

### helpers.removeBoss (permanent)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Removes the Boss from the scene, alongside its puppet and any Entities spawned by it.
This function also Works in Cutscene files

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`permanent` (`boolean`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;If the boss should not be loaded again. False will spawn the Boss every time the room is loaded.  

---

## Position and Movement

### helpers.setEffectiveGravityMult (mult)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Set the gravity multiplier to the fiven value. Gravity constant is 900.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`mult` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The multiplier to apply to the Gravity constant which the Boss will use.  

---

## helpers.setGroundFriction (friction)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Set the Boss's horizontal ground friction deceleration rate.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`friction` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The deceleration rate to set.  

---

## helpers.setAirFriction (friction)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Set the Boss's horizontal air friction deceleration rate.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`friction` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The deceleration rate to set.  

---

### helpers.setXSpeed (value)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Set the Boss' x speed to the given value

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`value` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The value to set the Boss' speed x component to.  

---

### helpers.setYSpeed (value)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Set the Boss' y speed to the given value

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`value` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The value to set the Boss' speed y component to.  

---

### helpers.setSpeed (x, y)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Set the Boss' speed to the given values

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`x` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The value to set the Boss' speed x component to.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`y` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The value to set the Boss' speed y component to.  

---

### helpers.setXSpeedDuring (value, time)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Set the Boss' x speed to the given value, kept constant during the given time.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`value` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The value to set the Boss' speed x component to.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The time to hold the value for.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`): The time given from the Tween

---

### helpers.setYSpeedDuring (value, time)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Set the Boss' y speed to the given value, kept constant during the given time.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`value` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The value to set the Boss' speed y component to.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The time to hold the value for.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`): The time given from the Tween

---

### helpers.setSpeedDuring (x, y, time)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Set the Boss' speed to the given values, kept constant during the given time.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`x` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The value to set the Boss' speed x component to.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`y` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The value to set the Boss' speed y component to.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The time to hold the values for.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`): The time given from the Tween

---

### helpers.keepXSpeedDuring (time)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Keep the Boss' current x speed constant during the given time.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The time to hold the value for.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`): The time given from the Tween

---

### helpers.keepYSpeedDuring (time)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Keep the Boss' current y speed constant during the given time.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The time to hold the value for.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`): The time given from the Tween

---

### helpers.keepSpeedDuring (time)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Keep the Boss' current speed constant during the given time.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The time to hold the values for.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`): The time given from the Tween

---

### helpers.positionTween (target, time[, easer=nil[, invert=false]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Create a new Position Tween, which will slowly move the Boss to the target.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`target` (`Vector2`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The vector2 target position the Boss will move towards.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The time the Boss will take to reach the target.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`easer` (`string|Easer`) (default `nil`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The easer to apply to the motion. If a string is provided, it will call [helpers.getEaserByName](#helpersgeteaserbyname-name-invert). Defaults to nil.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`invert` (`boolean`) (default `false`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;If the easer should be inverted. Defaults to false.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`): The time given from the Tween

---

### helpers.speedXTween (start, target, time[, easer=nil[, invert=false]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Create a new Tween for the Boss' x speed.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`start` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The initial value of the Tween, which the Boss' speed x component will set to at the start.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`target` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The value the Boss' speed x component will slowly change to.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The time the Boss will take to reach the target x speed.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`easer` (`string|Easer`) (default `nil`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The easer to applt to the x speed value. If a string is provided, it will call [helpers.getEaserByName](#helpersgeteaserbyname-name-invert). Defaults to nil.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`invert` (`boolean`) (default `false`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;If the easer should be inverted. Defaults to false.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`): The time given from the Tween

---

### helpers.speedYTween (start, target, time[, easer=nil[, invert=false]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Create a new Tween for the Boss' y speed.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`start` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The initial value of the Tween, which the Boss' speed y component will set to at the start.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`target` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The value the Boss' speed y component will slowly change to.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The time the Boss will take to reach the target y speed.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`easer` (`string|Easer`) (default `nil`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The easer to applt to the y speed value. If a string is provided, it will call [helpers.getEaserByName](#helpersgeteaserbyname-name-invert). Defaults to nil.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`invert` (`boolean`) (default `false`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;If the easer should be inverted. Defaults to false.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`): The time given from the Tween

---

### helpers.speedTween (xStart, xTarget, yStart, yTarget, time[, easer=nil[, invert=false]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Create a new Tween for the Boss' speed.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`xStart` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The initial value of the Tween, which the Boss' speed x component will set to at the start.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`xTarget` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The value the Boss' speed x component will slowly change to.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`yStart` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The initial value of the Tween, which the Boss' speed y component will set to at the start.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`yTarget` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The value the Boss' speed y component will slowly change to.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The time the Boss will take to reach the target x speed.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`easer` (`string|Easer`) (default `nil`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The easer to applt to the x speed value. If a string is provided, it will call [helpers.getEaserByName](#helpersgeteaserbyname-name-invert). Defaults to nil.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`invert` (`boolean`) (default `false`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;If the easer should be inverted. Defaults to false.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`): The time given from the Tween

---

### helpers.speedXTweenTo (target, time[, easer=nil[, invert=false]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Create a new Tween for the Boss' x speed from its current x speed value.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`target` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The value the Boss' speed x component will slowly change to.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The time the Boss will take to reach the target x speed.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`easer` (`string|Easer`) (default `nil`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The easer to applt to the x speed value. If a string is provided, it will call [helpers.getEaserByName](#helpersgeteaserbyname-name-invert). Defaults to nil.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`invert` (`boolean`) (default `false`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;If the easer should be inverted. Defaults to false.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`): The time given from the Tween

---

### helpers.speedYTweenTo (target, time[, easer=nil[, invert=false]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Create a new Tween for the Boss' x speed from its current y speed value.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`target` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The value the Boss' speed y component will slowly change to.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The time the Boss will take to reach the target y speed.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`easer` (`string|Easer`) (default `nil`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The easer to applt to the y speed value. If a string is provided, it will call [helpers.getEaserByName](#helpersgeteaserbyname-name-invert). Defaults to nil.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`invert` (`boolean`) (default `false`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;If the easer should be inverted. Defaults to false.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`): The time given from the Tween

---

### helpers.speedTweenTo (xTarget, yTarget, time[, easer=nil[, invert=false]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Create a new Tween for the Boss'  speed from its current x speed value.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`xTarget` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The value the Boss' speed x component will slowly change to.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`yTarget` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The value the Boss' speed y component will slowly change to.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The time the Boss will take to reach the target x speed.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`easer` (`string|Easer`) (default `nil`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The easer to applt to the x speed value. If a string is provided, it will call [helpers.getEaserByName](#helpersgeteaserbyname-name-invert). Defaults to nil.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`invert` (`boolean`) (default `false`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;If the easer should be inverted. Defaults to false.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`): The time given from the Tween

---

## Collisions and Colliders

### helpers.enableCollisions ()

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Enable the Boss' Collision checks from other entities.

---

### helpers.disableCollisions ()

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Disable the Boss' Collision checks from other entities.

---

### helpers.enableSolidCollisions ()

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Enable the Boss' Collision checks with solids.

---

### helpers.disableSolidCollisions ()

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Disable the Boss' Collision checks with solids.

---

### helpers.setHitCooldown (value)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Set the Boss' hit cooldown to the given value

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`value` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The timer to set the cooldown to  

---

### helpers.resetHitCooldown ()

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Set the Boss' hit cooldown back to the default value defined.

---

### helpers.changeBaseHitboxTo (tag)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Change the Boss' hitboxes to those stored under the given tag.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`tag` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The hitbox group tag to use.  

---

### helpers.changeHurtboxTo (tag)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Change the Boss' hurtboxes to those stored under the given tag.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`tag` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The hurtbox group tag to use.  

---

### helpers.changeBounceboxTo (tag)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Change the Boss' bouncebox to that stored under the given tag.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`tag` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The bouncebox tag to use.  

---

### helpers.changeTargetTo (tag)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Change the Boss' Sidekick Target to that stored under the given tag.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`tag` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The Sidekick Target tag to use.  

---

### helpers.getHitbox (width, height[, x=0[, y=0]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Create a new Rectangular Hitbox Collider

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`width` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The width of the collider.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`height` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The height of the collider.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`x` (`number`) (default `0`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The x offset of the hitbox. Defaults to 0.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`y` (`number`) (default `0`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The y offest of the Hitbox. Defaults to 0.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`hitbox` (`Collider`): The created Hitbox Collider

---

### helpers.getCircle (radius[, x=0[, y=0]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Create a new Circle Collider

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`radius` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The radius of the collider.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`x` (`number`) (default `0`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The x offset of the hitbox. Defaults to 0.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`y` (`number`) (default `0`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The y offest of the Hitbox. Defaults to 0.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`circle` (`Collider`): The created Hitbox Collider

---

### helpers.getColliderList (...)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Create a ColliderList object from the provided colliders.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`...` (`Collider`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;All the colliders to combine into a ColliderList  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`colliderList` (`ColliderList`): The combined ColliderList object.

---

## Boss Components and Entities

### helpers.addComponentToBoss (component)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Add a component to the Boss.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`component` (`Component`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The component to add.  

---

### helpers.addConstantBackgroundCoroutine (func, ...)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Add a function that will run in the background.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`func` (`fun(...)`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The function that will run in the background. Will run to completion or loop as defined.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`...` (`any`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Parameters to pass to the wrapped function, if any  

---

### helpers.getEntityChecker (checker[, func=helpers.destroyEntity[, state=true[, remove=true]]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns an EntityChecker Component that will execute the second passed function when the first function's return value matches the state required.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`checker` (`fun()`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The function that will be called every frame to test its value.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`func` (`fun(entity: Entity)`) (default `helpers.destroyEntity`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The function that will execute once the timer ends. Takes an entity parameter, which will be the Entity the component is added to. Defaults to the DestroyEntity function.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`state` (`boolean`) (default `true`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The state the checker function's return value must match. Defaults to true.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`remove` (`boolean`) (default `true`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;If the component should remove itself after it calls the func function. Defaults to true  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`checker` (`Component`): The Entity Checker that can be added to any Entity.

---

### helpers.getEntityTimer (timer[, func=helpers.destroyEntity])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns an EntityTimer Component that will execute the passed function when the timer ends.
Can be added to any Entity.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`timer` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The amount of time that must pass for the timer to execute.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`func` (`fun(entity: Entity)`) (default `helpers.destroyEntity`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The function that will execute once the timer ends. Takes an entity parameter, which will be the Entity the component is added to. Defaults to the DestroyEntity function.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`timer` (`Component`): The Entity Timer that can be added to any Entity.

---

### helpers.getEntityFlagger (flag[, func=helpers.destroyEntity[, state=true[, resetFlag=true]]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns an EntityFlagger Component that will execute the passed function when the given session flag's state matches the required state.
Can be added to any Entity.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`flag` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The session flag the entity will use to activate its function.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`func` (`fun(entity: Entity)`) (default `helpers.destroyEntity`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The function that will execute once the session flag state is the same as the state parameter. Takes an entity parameter, which will the Entity the component is added to. Defaults to the destroyEntity function.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`state` (`boolean`) (default `true`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The state the flag must match to activate the passed function. Defaults to true.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`resetFlag` (`boolean`) (default `true`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;If the flag should return to its previous state once used by the Flagger. Defaults to true  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`flagger` (`Component`): The Entity Flagger that can be added to any Entity.

---

### helpers.getEntityChain (entity[, startChained=true[, remove=false]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns an EntityChain component that will keep another entity's position chained to the Entity this component is added to.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`entity` (`Entity`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The entity to chain, whose position will change as the base Entity moves.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`startChained` (`boolean`) (default `true`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Whether the entity should start chained immediately. Defaults to true.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`remove` (`boolean`) (default `false`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Whether the chained entity should be removed if the chain component is also removed.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`the` (`Component`): Entity Chain component that can be added to any Entity.

---

### helpers.getNewBasicAttackEntity (position, hitboxes, spriteName[, startCollidable=true[, funcOnPlayer=killPlayer[, xScale=1[, yScale=1]]]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Create and return a basic entity to use in attacks.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`position` (`Vector2`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The position the entity will be at.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`hitboxes` (`Collider`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The collider the entity will use.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`spriteName` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The sprite the entity will use.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`startCollidable` (`boolean`) (default `true`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;If the entity should spawn with collisions active. Defaults to true.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`funcOnPlayer` (`fun(self, player)`) (default `killPlayer`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The function that will be called when the entity "self" collides with the Player. Defaults to killing the Player.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`xScale` (`number`) (default `1`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The horizontal sprite scale. Defaults to 1.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`yScale` (`number`) (default `1`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The vertical sprite scale. Defaults to 1.  

---

### helpers.getNewBasicAttackActor (position, hitboxes, spriteName[, gravMult=1[, maxFall=90[, startCollidable=true[, startSolidCollidable=true[, funcOnPlayer=killPlayer[, xScale=1[, yScale=1]]]]]]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Create and return a basic entity to use in attacks.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`position` (`Vector2`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The position the entity will be at.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`hitboxes` (`Collider`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The collider the entity will use.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`spriteName` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The sprite the entity will use.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`gravMult` (`number`) (default `1`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The multiplier to the Gravity constant the Actor should use. Defaults to 1.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`maxFall` (`number`) (default `90`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The fastest the Boss will fall naturally due to gravity. Defaults to 90.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`startCollidable` (`boolean`) (default `true`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;If the entity should spawn with collisions active. Defaults to true.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`startSolidCollidable` (`boolean`) (default `true`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;If the entity should spawn with solid collisions active. Defaults to true.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`funcOnPlayer` (`fun(self, player)`) (default `killPlayer`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The function that will be called when the entity "self" collides with the Player. Defaults to killing the Player.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`xScale` (`number`) (default `1`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The horizontal sprite scale. Defaults to 1.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`yScale` (`number`) (default `1`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The vertical sprite scale. Defaults to 1.  

---

## Component Retreival

### helpers.getComponents (name[, prefix])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Gets all tracked components by class name.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`name` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Class name of the component, relative to "Celeste." by default.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`prefix` (`string`) (optional)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Overrides the global class name prefix.  

---

### helpers.getComponent (name[, prefix])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Gets the first tracked component by class name.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`name` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Class name of the component, relative to "Celeste." by default.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`prefix` (`string`) (optional)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Overrides the global class name prefix.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`component` (`Component`): First tracked component of given class.

---

### helpers.getAllComponents (name[, prefix])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Gets all components by class name.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`name` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Class name of the component, relative to "Celeste." by default.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`prefix` (`string`) (optional)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Overrides the global class name prefix.  

---

### helpers.getFirstComponent (name[, prefix])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Gets the first component by class name.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`name` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Class name of the component, relative to "Celeste." by default.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`prefix` (`string`) (optional)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Overrides the global class name prefix.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`component` (`Component`): First component of given class.

---

### helpers.getAllComponentsOnType (name, entity[, prefix[, entityPre]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Gets all components by class name added to an entity of given class name.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`name` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Class name of the component, relative to "Celeste." by default.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`entity` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Class name of the entity, relative to "Celeste." by default.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`prefix` (`string`) (optional)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Overrides the global class name prefix for the Component class.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`entityPre` (`string`) (optional)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Overrides the global class name prefix for the Entity class.  

---

### helpers.getFirstComponentOnType (name, entity[, prefix[, entityPre]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Gets the first component by class name added to an entity of the given class name.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`name` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Class name of the component, relative to "Celeste." by default.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`entity` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Class name of the entity, relative to "Celeste." by default.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`prefix` (`string`) (optional)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Overrides the global class name prefix for the Component class.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`entityPre` (`string`) (optional)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Overrides the global class name prefix for the Entity class.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`component` (`Component`): First component of given class attached to the entity type.

---

### helpers.getComponentsFromEntity (entity, name[, prefix])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns all the components of the given class name from the entity given, if any.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`entity` (`Entity`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The entity to check.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`name` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Class name of the component, relative to "Celeste." by default.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`prefix` (`string`) (optional)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Overrides the global class name prefix.  

---

### helpers.getComponentFromEntity (entity, name[, prefix])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns the component of the given class name from the entity given, if any.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`entity` (`Entity`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The entity to check.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`name` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Class name of the component, relative to "Celeste." by default.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`prefix` (`string`) (optional)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Overrides the global class name prefix.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`component` (`Component`): First component of given class stored on the entity, if any.

---

### helpers.entityHasComponent (entity, name[, prefix])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Checks if the entity given has a component of the given class name.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`entity` (`Entity`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The entity to check.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`name` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Class name of the component, relative to "Celeste." by default.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`prefix` (`string`) (optional)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Overrides the global class name prefix.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`componentFound` (`boolean`): If the Entity does have a Component of the type specified.

---

## Health System

### helpers.getPlayerHealth ()

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Get the Player's current health value on the active Health System

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`health` (`number`): The player's health value, or -1 if there's no active Health System

---

### helpers.giveInvincibleFrames (time)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Gives additional time where the player is invincible to taking damage.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`time` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The time to add to the invincible timer.  

---

## Misc. Functions

### helpers.sayExt (dialog, ...)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Display textbox with dialog. Any provided functions will be passed as Triggers accessible to Dialog.txt triggers.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`dialog` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Dialog ID used for the conversation.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`...` (`function`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Functions that will be called whenever a trigger is activated through dialogue.  

---

### helpers.getNewEntityData (position[, width=0[, height=0[, id=1000]]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Get a new EntityData object

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`position` (`Vector2`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The vector2 position the entityData will hold.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`width` (`number`) (default `0`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The width the EntityData will hold. Defaults to 0.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`height` (`number`) (default `0`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The height the EntityData will hold. Defaults to 0.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`id` (`integer`) (default `1000`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The id the EntityData will hold. Defaults to 1000.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`entityData` (`EntityData`): The formed EntityData object with the Values dictionary initialized empty.

---

### helpers.setEntityDataAttributes (entityData)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Set a list of attributes to the provided EntityData object's Values dictionary.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`entityData` (`EntityData`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The EntityData to update.  

---

### helpers.storeObjectInBoss (key, object)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Store any object within the Boss under a specific key, to be retreived later.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`key` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The key to store the object with. Must be unique, or the object will not be stored.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`object` (`any`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The object to store  

---

### helpers.getStoredObjectFromBoss (key)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Return an item that was stored within the Boss by key.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`key` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The key the object is stored under.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`object` (`nil|any`): The object stored, or nil if key is not found.

---

### helpers.deleteStoredObjectFromBoss (key)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Remove the object stored under the specified key from the Boss' stored objects.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`key` (`string`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The key the object is stored under.  

---

### helpers.doMethodAfterDelay (func, delay)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Set a method that will execute after a given delay.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`func` (`fun()`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The function to execute. Takes no parameters.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`delay` (`number`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The time in seconds the function will be called after.  

---

### helpers.v2L (vector)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Get the length of the provided vector2

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`vector` (`Vector2`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Vector to get length of  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`length` (`number`): The length of the vector2

---

### helpers.normalize (vector[, length=1])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Normalizes the vector provided to the given length or 1.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`vector` (`Vector2`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The vector to normalize  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`length` (`number`) (default `1`)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The new length of the vector or 1  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`normal` (`Vector2`): The normalized vector2

---

### helpers.getEaserByName ([name[, invert]])

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;A specific Easer can be obtained by calling "monocle.Ease.{name}" which returns the desired Easer.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`name` (`string`) (optional)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The name of the Easer to get.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`invert` (`boolean`) (optional)  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;If the easer returned should be inverted.  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns:  

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`easer` (`nil|Easer`): The Easer found or nil if not found.

---
