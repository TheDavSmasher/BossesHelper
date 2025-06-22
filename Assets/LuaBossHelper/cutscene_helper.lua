--Lua Files obtained from Lua Cutscenes mod, reformatted to fit Bosses Helper

local cutsceneHelper = {}

local celesteMod = require("#celeste.mod")

local function threadProxyResume(self, ...)
    local thread = self.value

    if coroutine.status(thread) == "dead" then
        return false, nil
    end

    local success, message = coroutine.resume(thread)

    -- The error message should be returned as an exception and not a string
    if not success then
        return success, celesteMod[modName].Code.Helpers.LuaException(message)
    end

    return success, message
end

local function prepareCutscene(env, func)
    local success, onBegin, onEnd = pcall(func)

    if success then
        onEnd = onEnd or env.onEnd
        onBegin = onBegin or env.onBegin

        return onBegin, onEnd

    else
        celesteMod.logger.log(celesteMod.logLevel.error, "Bosses Helper", "Failed to load cutscene in Lua: " .. onBegin)

        return success
    end
end

local function prepareAttack(env, func)
    local success, onBegin, onEnd, onComplete, onInterrupt, onDeath = pcall(func)

    if success then
        onBegin = onBegin or env.onBegin
        onEnd = onEnd or env.onEnd
        onComplete = onComplete or env.onComplete
        onInterrupt = onInterrupt or env.onInterrupt
        onDeath = onDeath or env.onDeath

        return onBegin, onEnd, onComplete, onInterrupt, onDeath
    else
        celesteMod.logger.log(celesteMod.logLevel.error, "Bosses Helper", "Failed to load attack in Lua: " .. onBegin)
        return success
    end
end

local function prepareInterruption(env, func)
    local success, onHit, onContact, onDash, onBounce, onLaser, setup = pcall(func)

    if success then
        onHit = onHit or env.onHit
        onContact = onContact or env.onContact
        onDash = onDash or env.onDash
        onBounce = onBounce or env.onBounce
        onLaser = onLaser or env.onLaser
        setup = setup or env.setup

        return onHit, onContact, onDash, onBounce, onLaser, setup
    else
        celesteMod.logger.log(celesteMod.logLevel.error, "Bosses Helper", "Failed to load interrupt data in Lua: " .. onHit)
        return success
    end
end

local function prepareFunction(env, func)
    local success, onDamage, onRecover = pcall(func)

    if success then
        onDamage = onDamage or env.onDamage
        onRecover = onRecover or env.onRecover

        return onDamage, onRecover
    else
        celesteMod.logger.log(celesteMod.logLevel.error, "Bosses Helper", "Failed to load on damage function in Lua: " .. onDamage)
        return success
    end
end

local function prepareSavePoint(env, func)
    local success, onTalk = pcall(func)

    if success then
        onTalk = onTalk or env.onTalk
        
        return onTalk
    else
        celesteMod.logger.log(celesteMod.logLevel.error, "Bosses Helper", "Failed to load on save point function in Lua: " .. onTalk)
        return success
    end
end

function cutsceneHelper.setFuncAsCoroutine(func)
    return func and celesteMod.LuaCoroutine({value = coroutine.create(func), resume = threadProxyResume})
end

function cutsceneHelper.readFile(filename, modName)
    return celesteMod[modName].Code.Helpers.LuaBossHelper.GetFileContent(filename)
end

local function addHelperFunctions(data, env)
    local helperContent = cutsceneHelper.readFile(data.modMetaData.Name .. ":/Assets/LuaBossHelper/helper_functions", data.modMetaData.Name)
    local helperFunctions = load(helperContent, nil, nil, env)()

    for k, v in pairs(helperFunctions) do
        env[k] = v
    end

    env.helpers = helperFunctions
end

function cutsceneHelper.getLuaEnv(data)
    local env = data or {}

    setmetatable(env, {__index = _G})

    return env
end

function cutsceneHelper.getLuaData(filename, data, preparationFunc)
    preparationFunc = preparationFunc or function() end

    local env = cutsceneHelper.getLuaEnv(data)
    local content = cutsceneHelper.readFile(filename, data.modMetaData.Name)

    if content then
        addHelperFunctions(data, env)

        local func = load(content, nil, nil, env)

        return env, preparationFunc(env, func)
    end
end

function cutsceneHelper.getCutsceneData(filename, data)
    return cutsceneHelper.getLuaData(filename, data, prepareCutscene)
end

function cutsceneHelper.getAttackData(filename, data)
    return cutsceneHelper.getLuaData(filename, data, prepareAttack)
end

function cutsceneHelper.getInterruptData(filename, data)
    return cutsceneHelper.getLuaData(filename, data, prepareInterruption)
end

function cutsceneHelper.getFunctionData(filename, data)
    return cutsceneHelper.getLuaData(filename, data, prepareFunction)
end

function cutsceneHelper.getSavePointData(filename, data)
    return cutsceneHelper.getLuaData(filename, data, prepareSavePoint)
end

return cutsceneHelper
