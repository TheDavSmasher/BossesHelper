--Lua Files obtained from Lua Cutscenes mod, reformatted to fit Bosses Helper
--Created by Cruor, modified and expanded by Dav

--- Helper functions that can be used in cutscenes.
-- Please do not edit this file to add your own helper functions, put them in your own cutscenes.
-- Optionally load them with `loadCelesteAsset` and put it in a common library.
-- When using from a cutscene "helpers." is not required.
-- For example "helpers.say" will be just "say".
-- Return values starting with # are from C#.
-- @module helper_functions

local luanet = _G.luanet

local celeste = require("#celeste")
local celesteMod = celeste.mod
local csharpVector2 = require("#microsoft.xna.framework.vector2")
local engine = require("#monocle.engine")

local modName = modMetaData.Name
local classNamePrefix = "Celeste."

local helpers = {}

helpers.celeste = celeste
helpers.engine = engine

function helpers.vector2(x, y)
    local typ = type(x)

    if typ == "table" and not y then
        return csharpVector2(x[1], x[2])

    elseif typ == "userdata" and not y then
        return x

    else
        return csharpVector2(x, y)
    end
end

local function simpleSplit(s, sep)
    local res = {}

    for part in s:gmatch("[^" .. sep .. "]+") do
        table.insert(res, part)
    end

    return res
end

local function getClassAndField(full)
    local parts = simpleSplit(full, "%.")
    local field = parts[#parts]

    table.remove(parts)

    local class = table.concat(parts, ".")

    return class, field
end

--- Set the prefix for getting Celeste classes.
-- By default this is "Celeste.".
-- @string prefix The new prefix.
function helpers.setClassNamePrefix(prefix)
	classNamePrefix = prefix
end

--- Get the prefix for getting Celeste classes.
-- By default this is "Celeste.".
-- @treturn #Monocle.Entity First entity of given class.
function helpers.getClassNamePrefix()
	return classNamePrefix
end

--- Get the content of a file from a Celeste asset.
-- @string filename Filename to load. Filename should not have a extention.
function helpers.readCelesteAsset(filename)
    return celesteMod[modName].Code.Helpers.LuaBossHelper.GetFileContent(filename)
end

--- Loads and returns the result of a Lua asset.
-- @string filename Filename to load. Filename should not have a extention.
function helpers.loadCelesteAsset(filename)
    local content = helpers.readCelesteAsset(filename)

    if not content then
        celesteMod.logger.log(celesteMod.LogLevel.Error, "Bosses Helper", "Failed to require asset in Lua: file '" .. filename .. "' not found")

        return
    end

    local env = {}

    setmetatable(env, {__index = _ENV})

    local func = load(content, nil, nil, env)
    local success, result = pcall(func)

    if success then
        return result

    else
        celesteMod.logger.log(celesteMod.LogLevel.Error, "Bosses Helper", "Failed to require asset in Lua: " .. result)
    end
end

--- Put debug message in the Celeste console.
-- @string message The debug message.
-- @string[opt="Bosses Helper"] tag The tag in the console.
function helpers.log(message, tag)
    celesteMod.logger.log(celesteMod.LogLevel.Info, tag or "Bosses Helper", tostring(message))
end

--- Gets enum value.
-- @string enum String name of enum.
-- @tparam any value string name or enum value to get.
function helpers.getEnum(enum, value)
    local enumValue = luanet.enum(luanet.import_type(enum), value)

    -- Check for enum on class
    if not enumValue then
        local class, field = getClassAndField(enum)

        enumValue = luanet.enum(luanet.import_type(class)[field], value)
    end

	return enumValue
end

--- Pause code exection for duration seconds.
-- @number duration Duration to wait (in seconds).
function helpers.wait(duration)
    return coroutine.yield(duration)
end

--- Gets the current room the player is in.
-- @treturn #Celeste.Level The current room.
function helpers.getRoom()
    return engine.Scene
end

helpers.getLevel = helpers.getRoom

--- Gets the current session.
-- @treturn #Celeste.Session The current session.
function helpers.getSession()
    return engine.Scene.Session
end

--- Display textbox with dialog.
-- @string dialog Dialog ID used for the conversation.
function helpers.say(dialog)
    coroutine.yield(celeste.Textbox.Say(tostring(dialog)))
end

--- Display minitextbox with dialog.
-- @string dialog Dialog ID used for the textbox.
function helpers.miniTextbox(dialog)
    engine.Scene:Add(celeste.MiniTextbox(dialog))
end

--- Allow the user to select one of several minitextboxes, similar to intro cutscene of Reflection.
-- @string ... Dialog IDs for each of the textboses as varargs. First argument can be a table of dialog ids instead.
-- @treturn number The index of the option the player selected.
function helpers.choice(...)
    local choices = {...}

    if type(choices[1]) == "table" then
        choices = choices[1]
    end

    coroutine.yield(celesteMod[modName].ChoicePrompt.Prompt(table.unpack(choices)))

    return celesteMod[modName].ChoicePrompt.Choice + 1
end

-- Used by helpers.choiceDialog to store the current dialog table.
-- When this gets set to null, the dialog ends.
local currentDialog

-- Used by helpers.choiceDialog to find an entry in the dialogTable based on its dialog key
local function findDialogByKey(dialogTable, key)
    for i, value in ipairs(dialogTable) do
        local dialogKey = value[1]

        if dialogKey == key then
            return value
        end
    end
end

-- Used by helpers.choiceDialog to check whether the requirements for a choice are met
local function requirementsMet(requires, ctx)
    if type(requires) ~= "table" then
        requires = { requires }
    end

    for _, required in ipairs(requires) do
        local t = type(required)

        if t == "function" then
            if not required(ctx) then
                return false
            end

        elseif t == "string" then
            if not ctx.usedDialogs[required] then
                return false
            end
        end
    end

    return true
end

local function prepareDialogTable(dialogTable)
    local dialogs = {}

    for _, dialog in ipairs(dialogTable) do
        if type(dialog) == "string" then
            table.insert(dialogs, {dialog, repeatable = true})

        else
            table.insert(dialogs, dialog)
        end
    end

    return dialogs
end

---Displays a choice dialog, similar to intro cutscene of Reflection.
---Unlike helpers.choice, this function also handles displaying dialogs and keeping track of which choices were already picked.
---Check the 'example_talker.lua' file for a usage example.
-- @tparam table Table describing all choices and requirements, etc.
function helpers.choiceDialog(dialogs)
    local dialogTable = prepareDialogTable(dialogs)

    currentDialog = dialogTable

    local ctx = {
        usedDialogs = {}
    }

    while currentDialog do
        local currChoices = {}

        -- determine which choices are valid
        for _, value in ipairs(dialogTable) do
            local dialogKey = value[1]

            if not value.repeatable and ctx.usedDialogs[dialogKey] then
                goto next
            end

            if value.requires and not requirementsMet(value.requires, ctx) then
                goto next
            end

            table.insert(currChoices, dialogKey)
            ::next::
        end

        local chosenKey = currChoices[helpers.choice(currChoices)]
        local chosen = findDialogByKey(dialogTable, chosenKey)

        if chosen.onChosen then
            chosen.onChosen(ctx)

        else
            helpers.say(chosenKey .. "_SAY")
        end

        if chosen.onEnd then
            chosen.onEnd(ctx)
        end

        ctx.usedDialogs[chosenKey] = true
    end
end

---Closes the choice dialog previously opened by helpers.choiceDialog
function helpers.closeChoiceDialog()
    currentDialog = nil
end

--- Display postcard.
-- @string dialog Dialog ID or message to show in the postcard.
-- @tparam any sfxIn effect when opening the postcard or area ID.
-- @string[opt=nil] sfxOut Sound effect when closing the postcard. If not used then second argument is assumed to be area ID.
function helpers.postcard(dialog, sfxIn, sfxOut)
    local message = celeste.Dialog.Get(dialog) or dialog
    local postcard

    if sfxOut then
        postcard = celeste.Postcard(message, sfxIn, sfxOut)

    else
        postcard = celeste.Postcard(message, sfxIn)
    end

    getRoom():add(postcard)
    postcard:BeforeRender()

    coroutine.yield(postcard:DisplayRoutine())
end

--- Player walks to the given X coordinate. This is in pixels and uses map based coordinates.
-- @number x X coordinate to walk to.
-- @bool[opt=false] walkBackwards If the player should visually be walking backwards.
-- @number[opt=1.0] speedMultiplier How fast the player should move. Walking is considered a speed multiplier of 1.0.
-- @bool[opt=false] keepWalkingIntoWalls If the player should keep walking into walls.
function helpers.walkTo(x, walkBackwards, speedMultiplier, keepWalkingIntoWalls)
    coroutine.yield(player:DummyWalkTo(x, walkBackwards or false, speedMultiplier or 1, keepWalkingIntoWalls or false))
end

--- Player walks x pixels from current position.
-- @number x X offset for where player should walk.
-- @bool[opt=false] walkBackwards If the player should visually be walking backwards.
-- @number[opt=1.0] speedMultiplier How fast the player should move. Walking is considered a speed multiplier of 1.0.
-- @bool[opt=false] keepWalkingIntoWalls If the player should keep walking into walls.
function helpers.walk(x, walkBackwards, speedMultiplier, keepWalkingIntoWalls)
    helpers.walkTo(player.Position.X + x, walkBackwards, speedMultiplier, keepWalkingIntoWalls)
end

--- Player runs to the given X coordinate. This is in pixels and uses map based coordinates.
-- @number x X coordinate to run to.
-- @bool fastAnimation Whether this should use the fast animation or not.
function helpers.runTo(x, fastAnimation)
    coroutine.yield(player:DummyRunTo(x, fastAnimation or false))
end

--- Player runs x pixels from current position.
-- @number x X offset for where player should run.
-- @bool fastAnimation Whether this should use the fast animation or not.
function helpers.run(x, fastAnimation)
    helpers.runTo(player.Position.X + x, fastAnimation)
end

--- Kills the player.
-- @tparam[opt={0⸴ 0}] table direction The direction the player dies from.
-- @bool[opt=false] evenIfInvincible If the player should die even if they are invincible (assist mode).
-- @bool[opt=true] registerDeathInStats If it should count as a death in journal.
function helpers.die(direction, evenIfInvincible, registerDeathInStats)
    if player and not player.Dead then
        player:Die(vector2(direction or {0, 0}), evenIfInvincible or false, registerDeathInStats or registerDeathInStats == nil)
    end
end

--- Sets the current player state.
-- @param state Name of the state or the state number.
-- @bool[opt=false] locked If this should prevent the player for changing state afterwards.
function helpers.setPlayerState(state, locked)
    if type(state) == "string" then
        if not state:match("^St") then
            state = "St" .. state
        end

        player.StateMachine.State = celeste.Player[state]

    else
        player.StateMachine.State = state
    end

    player.StateMachine.Locked = locked or false
end

--- Gets the current state of the player.
function helpers.getPlayerState()
    return player.StateMachine.State, player.StateMachine.Locked
end

--- Disable player movement.
function helpers.disableMovement()
    helpers.setPlayerState("Dummy", false)
end

--- Enable player movement.
function helpers.enableMovement()
    helpers.setPlayerState("Normal", false)
end

--- Make the player jump.
-- @number[opt=2.0] duration How long the "jump button" would be held (in seconds).
function helpers.jump(duration)
    player:Jump(true, true)
    player.AutoJump = true
    player.AutoJumpTimer = duration or 2.0
end

--- Waits until the player is on the ground.
function helpers.waitUntilOnGround()
    while not player:OnGround(1) do
        wait()
    end
end

--- Changes the room the game thinks the player is in.
-- @string name Room name.
-- @string[opt] spawnX X coordinate for new spawn point, by default it uses bottom left of room.
-- @string[opt] spawnY Y coordinate for new spawn point, by default it uses bottom left of room.
function helpers.changeRoom(name, spawnX, spawnY)
    local level = engine.Scene

    level.Session.Level = name
    level.Session.RespawnPoint = level:GetSpawnPoint(vector2(spawnX or level.Bounds.Left, spawnY or level.Bounds.Bottom))
    level.Session:UpdateLevelStartDashes()

    -- TODO - Test
    engine.Scene = celeste.LevelLoader(level.Session, level.Session.RespawnPoint)
end

function helpers.getRoomPosition(name)
    -- TODO - Implement
    -- If name is absent use current room
end

--- Sets the player position to the absolute coordinates.
-- @number x Target x coordinate.
-- @number y Target y coordinate.
-- @string[opt] room What room the game should attempt to load. If room is specified player will land at closest spawnpoint to target location.
-- @tparam[opt] any introType intro type to use, can be either a #Celeste.Player.IntroTypes enum or a string
function helpers.teleportTo(x, y, room, introType)
    if type(introType) == "string" then
        introType = getEnum("Celeste.Player.IntroTypes", introType)
    end

    if room then
        local mapData = engine.Scene.Session.MapData
        local levelData = mapData:getAt(vector2(x, y))

        -- TeleportTo adds the new room offset for spawnpoint check, we have to remove this
        local offsetX, offsetY = 0, 0

        if levelData then
            local bounds = levelData.bounds

            offsetX, offsetY = bounds.X, bounds.Y
        end

        if x and y then
            celeste.Mod[modName].Code.Helpers.LuaMethodWrappers.TeleportTo(getLevel(), player, room, introType or player.IntroType, vector2(x - offsetX, y - offsetY))

        else
            celeste.Mod[modName].Code.Helpers.LuaMethodWrappers.TeleportTo(getLevel(), player, room, introType or player.IntroType)
        end

	else
		player.Position = vector2(x, y)
	end
end

--- Teleport the player to (x, y) pixels from current position.
-- @number x X offset on X axis.
-- @number y Y offset on Y axis.
-- @string[opt] room What room the game should attempt to load. If room is specified player will land at closest spawnpoint to target location.
-- @tparam[opt] any introType intro type to use, can be either a #Celeste.Player.IntroTypes enum or a string. Only applies if room is specified.
function helpers.teleport(x, y, room, introType)
    helpers.teleportTo(player.Position.X + x, player.Position.Y + y, room, introType)
end

--- Instantly teleport the player seamlessly.
--  Teleport player to (x, y) position, in pixels.
-- Room name as only argument will seamlessly teleport to that room at the same relative position.
-- @tparam any x X offset on X axis if number. Target room if string.
-- @number y Y offset on Y axis.
-- @string[opt] room What room the game should attempt to load. By default same room.
function helpers.instantTeleportTo(x, y, room)
    if x and y then
        -- Provide own position
        celeste.Mod[modName].Code.Helpers.LuaMethodWrappers.InstantTeleport(getLevel(), player, room or "", false, x, y)

    else
        -- Keep releative room position
        celeste.Mod[modName].Code.Helpers.LuaMethodWrappers.InstantTeleport(getLevel(), player, x or "", true, 0.0, 0.0)
    end
end

--- Instantly teleport the player to the same coordinates in another room seamlessly.
-- Teleport player (x, y) pixels from current position.
-- Room name as only argument will seamlessly teleport to that room at the same relative position.
-- @tparam any x X offset on X axis if number. Target room if string.
-- @number y Y offset on Y axis.
-- @string[opt] room What room the game should attempt to load. By default same room.
function helpers.instantTeleport(x, y, room)
    if x and y then
        helpers.instantTeleportTo(player.Position.X + x, player.Position.Y + y, room)

    else
        helpers.instantTeleportTo(x, y, room)
    end
end

--- Completes the level and returns the player to the chapter screen.
-- @bool[opt=false] spotlightWipe Whether this should be a spotlight wipe or not.
-- @bool[opt=false] skipScreenWipe Whether this wipe is skipped or not.
-- @bool[opt=false] skipCompleteScreen Whether this skips the complete screen.
function helpers.completeArea(spotlightWipe, skipScreenWipe, skipCompleteScreen)
    engine.scene:CompleteArea(spotlightWipe or false, skipScreenWipe or false, skipCompleteScreen or false)
end

--- Plays a sound.
-- @string name Event for the song.
-- @tparam[opt] table position Where the sound is played from.
-- @treturn #Celeste.Audio The audio instance of the sound.
function helpers.playSound(name, position)
    if position then
        return celeste.Audio.Play(name, position)

    else
        return celeste.Audio.Play(name)
    end
end

--- Gets all tracked entities by class name.
-- @string name Class name of the entity, relative to "Celeste." by default.
-- @string[opt] prefix Overrides the global class name prefix.
-- @treturn {#Monocle.Entity...} Tracked entities of given class.
function helpers.getEntities(name, prefix)
    return celeste.Mod[modName].Code.Helpers.LuaMethodWrappers.GetEntities(name, prefix or classNamePrefix)
end

--- Gets the first tracked entity by class name.
-- @string name Class name of the entity, relative to "Celeste." by default.
-- @string[opt] prefix Overrides the global class name prefix.
-- @treturn #Monocle.Entity First entity of given class.
function helpers.getEntity(name, prefix)
    return celeste.Mod[modName].Code.Helpers.LuaMethodWrappers.GetEntity(name, prefix or classNamePrefix)
end

--- Gets all entities by class name.
-- @string name Class name of the entity, relative to "Celeste." by default.
-- @string[opt] prefix Overrides the global class name prefix.
-- @treturn {#Monocle.Entity...} Tracked entities of given class.
function helpers.getAllEntities(name, prefix)
    return celeste.Mod[modName].Code.Helpers.LuaMethodWrappers.getAllEntities(name, prefix or classNamePrefix)
end

--- Gets the first entity by class name.
-- @string name Class name of the entity, relative to "Celeste." by default.
-- @string[opt] prefix Overrides the global class name prefix.
-- @treturn #Monocle.Entity First entity of given class.
function helpers.getFirstEntity(name, prefix)
    return celeste.Mod[modName].Code.Helpers.LuaMethodWrappers.GetFirstEntity(name, prefix or classNamePrefix)
end

--- Puts player in feather state.
function helpers.giveFeather()
    player:StartStarFly()
end

--- Get amount of deaths in current room.
-- @treturn number Current deaths in room.
function helpers.deathsInCurrentRoom()
    return engine.Scene.Session.DeathsInCurrentLevel
end

--- Play and update the current music track.
-- @string track Name of song, same as in Ahorn's room window.
-- @number progress[opt] Which progress level the music should be at. Leave empty for no change.
function helpers.playMusic(track, progress)
    engine.Scene.Session.Audio.Music.Event = celeste.SFX.EventnameByHandle(track)

    if progress then
        engine.Scene.Session.Audio.Music.Progress = progress
    end

    engine.Scene.Session.Audio:Apply()
end

--- Get the current music track name.
-- @treturn string Music track name.
function helpers.getMusic()
    return celeste.Audio.CurrentMusic
end

--- Sets music progression.
-- @number progress The new progress level.
function helpers.setMusicProgression(progress)
    engine.Scene.Session.Audio.Music.Progress = progress
    engine.Scene.Session.Audio:Apply()
end

--- Gets the current music progression.
-- @treturn number Music progress level.
function helpers.getMusicProgression()
    return engine.Scene.Session.Audio.Music.progress
end

--- Set music layer on/off.
-- @tparam any layer number or table of numbers to set.
-- @bool value The state of the layer.
function helpers.setMusicLayer(layer, value)
    if type(layer) == "table" then
        for _, index in ipairs(layer) do
            engine.Scene.Session.Audio.Music:Layer(index, value)
        end

    else
        engine.Scene.Session.Audio.Music:Layer(layer, value)
    end

    engine.Scene.Session.Audio:Apply()
end

--- Attempt to set the player spawnpoint.
-- @tparam[opt={0⸴ 0}] table target Where it should attempt to set the spawnpoint from.
-- @bool[opt=false] absolute If set uses absolute coordinates from target, otherwise it offsets from the center of the cutscene trigger.
function helpers.setSpawnPoint(target, absolute)
    local session = engine.Scene.Session
    local ct = cutsceneTrigger

    target = target or {0, 0}
    target = absolute and target or vector2(ct.Position.X + ct.Width / 2 + target[1], ct.Position.Y + ct.Height / 2 + target[2])

    if session.RespawnPoint and (session.RespawnPoint.X ~= target.X or session.RespawnPoint.Y ~= target.Y) then
        session.HitCheckpoint = true
        session.RespawnPoint = target
        session:UpdateLevelStartDashes()
    end
end

--- Shakes the camera.
-- @tparam direction Direction the screen should shake from.
-- @bool[opt] duration How long the screen should shake.
function helpers.shake(direction, duration)
    if direction and duration then
        engine.Scene:DirectionalShake(direction, duration)

    else
        engine.Scene:Shake(direction)
    end
end

--- Set player inventory
-- @param name Inventory to use. If name is string look it up in valid inventories, otherwise use the inventory.
function helpers.setInventory(inventory)
    if type(inventory) == "string" then
        engine.Scene.Session.Inventory = celeste.PlayerInventory[inventory]

    else
        engine.Scene.Session.Inventory = inventory
    end
end

--- Get player inventory
-- @string[opt] inventory If name is given get inventory by name, otherwise the current player inventory
function helpers.getInventory(inventory)
    if inventory then
        return celeste.PlayerInventory[inventory]

    else
        return engine.Scene.Session.Inventory
    end
end

--- Offset the camera by x and y like in camera offset trigger.
-- @param x X coordinate or table of coordinates to offset by.
-- @number[opt] y Y coordinate to offset by.
function helpers.setCameraOffset(x, y)
    engine.Scene.CameraOffset = y and vector2(x * 48, y * 32) or x
end

--- Get the current offset struct.
-- @treturn {number⸴ number} The camera offset.
function helpers.getCameraOffset()
    return engine.Scene.CameraOffset
end

--- Get the current room coordinates.
-- @treturn {number⸴ number} The camera offset.
function helpers.getRoomCoordinates()
  return engine.Scene.LevelOffset
end

--- Get the current room coordinates offset by x and y.
-- @param x X coordinate or table of coordinates to offset by.
-- @number[opt] y Y coordinate to offset by.
-- @treturn {number, number} The camera offset.
function helpers.getRoomCoordinatesOffset(x, y)
    if type(x) == "number" then
        return engine.Scene.LevelOffset + vector2(x, y)

    else
        return engine.Scene.LevelOffset + vector2(x[1], x[2])
    end
end

--- Set session flag.
-- @string flag Flag to set.
-- @bool value State of flag.
function helpers.setFlag(flag, value)
    engine.Scene.Session:SetFlag(flag, value)
end

--- Get session flag.
-- @string flag Flag to get.
-- @treturn bool The state of the flag.
function helpers.getFlag(flag)
    return engine.Scene.Session:GetFlag(flag)
end

-- TODO - Accept table?
-- TODO - Unhaunt, the index needs to be handled
function helpers.spawnBadeline(x, y, relativeToPlayer)
    local position = (relativeToPlayer or relativeToPlayer == nil) and vector2(player.Position.X + x, player.Position.Y + y) or vector2(x, y)
    local badeline = celeste.BadelineOldsite(position, 1)

    engine.Scene:Add(badeline)

    return badeline
end

--- Ends the current cutscene.
function helpers.endCutscene()
    cutsceneEntity:EndCutscene(engine.Scene)
end

--- Sets the current bloom strength.
-- @number amount New bloom strength.
function helpers.setBloomStrength(amount)
    engine.Scene.Bloom.Strength = amount
end

--- Returns the current bloom strength.
-- @treturn number Bloom strength.
function helpers.getBloomStrength()
    return engine.Scene.Bloom.Strength
end

--- Sets the current darkness (bloom) strength.
-- @number amount New bloom strength.
function helpers.setDarkness(amount)
    engine.Scene.Bloom.Strength = amount
end

--- Returns the current darkness (bloom) strength.
-- @treturn number Bloom strength.
function helpers.getDarkness()
    return engine.Scene.Bloom.Strength
end

--- Sets the current core mode.
-- @param mode String name for mode or Core Mode enum.
function helpers.setCoreMode(mode)
    if type(mode) == "string" then
        engine.Scene.CoreMode = engine.Scene.Session.CoreModes[mode]

    else
        engine.Scene.CoreMode = mode
    end
end

--- Returns the current core mode.
-- @treturn #Celeste.Session.CoreMode.
function helpers.getCoreMode()
    return engine.Scene.CoreMode
end

--- Changes the current colorgrade to the new one.
-- @string colorGrade Name of the color grade
-- @bool[opt=false] instant Wheter the color grade should instantly change or gradually change
function helpers.setColorGrade(colorGrade, instant)
    if instant then
        engine.Scene:SnapColorGrade(colorGrade)

    else
        engine.Scene:NextColorGrade(colorGrade)
    end
end

--- Bubble flies (cassette collection) to the target. This is in pixels and uses map based coordinates.
-- @number endX X coordinate for end point.
-- @number endY Y coordinate for end point.
-- @number[opt=endX] controllX X coordinate for controll point.
-- @number[opt=endY] controllY coordinate for controll.
function helpers.cassetteFlyTo(endX, endY, controllX, controllY)
    playSound("event:/game/general/cassette_bubblereturn", vector2(engine.Scene.Camera.Position.X + 160, engine.Scene.Camera.Position.Y + 90))

    if endX and endY and controllX and controllY then
        player:StartCassetteFly(vector2(endX, endY), vector2(controllX, controllY))

    else
        player:StartCassetteFly(vector2(endX, endY), vector2(endX, endY))
    end
end

--- Bubble flies (cassette collection) to the target relative to player. Values are in pixels and not tiles.
-- @number endX X offset for end point.
-- @number endY Y offset for end point.
-- @number[opt=endX] controllX X offset for controll point.
-- @number[opt=endY] controllY offset for controll.
function helpers.cassetteFly(endX, endY, controllX, controllY)
    local playerX = player.Position.X
    local playerY = player.Position.Y

    controllX = controllX or endX
    controllY = controllY or endY

    helpers.cassetteFlyTo(endX + playerX, endY + playerY, controllX + playerX, controllY + playerY)
end

function helpers.setLevelFlag()
    -- TODO - Implement
end

function helpers.getLevelFlag()
    -- TODO - Implement
end

--- Gives the player a key.
function helpers.giveKey()
    local level = engine.Scene
    local key = celeste.Key(player, Celeste.EntityID("unknown", 1073741823 + math.random(0, 10000)))

    level:Add(key)
    level.Session.Keys:Add(key.ID)
end

-- Test
function helpers.setWind(pattern)
    local windController = helpers.getFirstEntity("WindController")
    local level = engine.Scene

    if type(pattern) == "string" then
        pattern = windController.Patterns[pattern]
    end

    if windController then
        windController:SetPattern(pattern)

    else
        windController = celeste.WindController(pattern)
        level.Add(windController)
    end
end

-- Requires reflection :(
function helpers.getWind()
    local windController = helpers.getFirstEntity("WindController")

    if windController then
        return windController.startPattern
    end
end

-- Requires Enums
function helpers.rumble(...)
    -- TODO - Implement
end

--- Disables skip cutscene from menu.
function helpers.makeUnskippable()
    engine.Scene.InCutscene = false
    engine.Scene:CancelCutscene()
end

--- Enables retrying from menu.
function helpers.enableRetry()
    engine.Scene.CanRetry = true
end

--- Disables retrying from menu.
function helpers.disableRetry()
    engine.Scene.CanRetry = false
end

--- Prevents the player from even accessing the pause menu.
function helpers.disablePause()
    engine.Scene.PauseLock = true
end

--- Reenables the player to pause the game.
function helpers.enablePause()
    engine.Scene.PauseLock = false
end

--- Bosses Helper Attack specific functions and helper methods for the player
--- These will not work on Events and are not accessible even by direct call
--- Since no reference to the Controller is given, these function delegates are necessary

--Attack Delegates
function helpers.AddEntity(entity)
    bossAttack.addEntity:Invoke(entity)
end

function helpers.AddEntityWithTimer(entity, name, func, timer)
    bossAttack.addEntityWithTimer:Invoke(entity, name, func, timer)
end

function helpers.AddEntityWithFlagger(entity, flag, func, state, resetFlag)
    bossAttack.addEntityWithFlagger:Invoke(entity, flag, func, state or state == nil, resetFlag or resetFlag == nil)
end

function helpers.DestroyEntity(entity)
    bossAttack.destroyEntity:Invoke(entity)
end

function helpers.DestroyAll()
    bossAttack.destroyAll:Invoke()
end
--End Attack Delegates

--Interrupt Delegates
function helpers.GetHealth()
    return boss.getHealth:Invoke()
end

function helpers.SetHealth(health)
    boss.setHealth:Invoke(health)
end

function helpers.DecreaseHealth(health)
    boss.decreaseHealth:Invoke(health or 1)
end

function helpers.WaitForAttackToEnd()
    return coroutine.yield(boss.waitForAttack:Invoke())
end

function helpers.InterruptPattern()
    boss.interruptPattern:Invoke()
end

function helpers.GetCurrentPatternID()
    return boss.currentPattern:Invoke()
end

function helpers.StartAttackPattern(goTo)
    boss.startAttackPattern:Invoke(goTo or -1)
end
--End Interrupt Delegates

--- Additional, non-delegate helper shorthand methods
function helpers.PlayPuppetAnim(anim)
    puppet:PlayBossAnim(anim)
end

function helpers.AddComponentToBoss(component)
    puppet:Add(component)
end

function helpers.GetNewEntityData(position, width, height, id)
    newData = celeste.Mod.BossesHelper.BossesHelperModule.MakeEntityData()
    newData.ID = id or 1000
    newData.Level = engine.Scene
    newData.Position = position or vector2(0,0)
    newData.Width = width or 0
    newData.Height = height or 0
    return newData
end

return helpers
