--Lua Files obtained from Lua Cutscenes mod, reformatted to fit Bosses Helper

local cutsceneHelper = {}

---@type Mod
local celesteMod = require("#celeste.mod")

local function getMod(modName)
    return getMod(modName) --[[@as BossesHelper]]
end

---@type ProxyResume
local function threadProxyResume(self, ...)
    if coroutine.status(self.value) == "dead" then
        return false, nil
    end

    local success, message = coroutine.resume(self.value)

    -- The error message should be returned as an exception and not a string
    if not success then
        return success, celesteMod.BossesHelper.Code.Helpers.LuaException(message)
    end

    return success, message
end

---@type LuaPreparer
local function prepareCutscene(env, func)
    local success, onBegin, onEnd = pcall(func)

    if success then
        onEnd = onEnd or env.onEnd
        onBegin = onBegin or env.onBegin

        return onBegin, onEnd

    else
        celesteMod.Logger.Error("Bosses Helper", "Failed to load cutscene in Lua: " .. onBegin)

        return success
    end
end

---@type LuaPreparer
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
        celesteMod.Logger.Error("Bosses Helper", "Failed to load attack in Lua: " .. onBegin)
        return success
    end
end

---@type LuaPreparer
local function prepareInterruption(env, func)
    local success, onHit, onContact, onDash, onBounce, onLaser, setup = pcall(func)

    if success then
        onHit = onHit or env.onHit
        onContact = onContact or env.onContact
        onDash = onDash or env.onDash
        onBounce = onBounce or env.onBounce
        onLaser = onLaser or env.onLaser
        setup = setup or env.setup

        return setup, onHit, onContact, onDash, onBounce, onLaser
    else
        celesteMod.Logger.Error("Bosses Helper", "Failed to load interrupt data in Lua: " .. onHit)
        return success
    end
end

---@type LuaPreparer
local function prepareFunction(env, func)
    local success, onDamage, onRecover = pcall(func)

    if success then
        onDamage = onDamage or env.onDamage
        onRecover = onRecover or env.onRecover

        return onDamage, onRecover
    else
        celesteMod.Logger.Error("Bosses Helper", "Failed to load on damage function in Lua: " .. onDamage)
        return success
    end
end

---@type LuaPreparer
local function prepareSavePoint(env, func)
    local success, onTalk = pcall(func)

    if success then
        onTalk = onTalk or env.onTalk
        
        return onTalk
    else
        celesteMod.Logger.Error("Bosses Helper", "Failed to load on save point function in Lua: " .. onTalk)
        return success
    end
end

function cutsceneHelper.setFuncAsCoroutine(func)
    return func and celesteMod.LuaCoroutine({value = coroutine.create(func), resume = threadProxyResume})
end

function cutsceneHelper.readFile(filename, modName)
    return getMod(modName).Code.Helpers.LuaBossHelper.GetFileContent(filename)
end

local function addHelperFunctions(modName, env)
    local helperContent = getMod(modName).Code.Helpers.LuaBossHelper.HelperFunctions
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

---@type LuaGetData
function cutsceneHelper.getLuaData(filename, data, preparationFunc)
    preparationFunc = preparationFunc or function() end

    local modName = data.modMetaData.Name
    local env = cutsceneHelper.getLuaEnv(data)
    local content = cutsceneHelper.readFile(filename, modName)

    if content then
        addHelperFunctions(modName, env)

        local func = load(content, nil, nil, env) --[[@as function]]

        return env, preparationFunc(env, func)
    end
end

---@type LuaDataGetter
function cutsceneHelper.getCutsceneData(filename, data)
    return cutsceneHelper.getLuaData(filename, data, prepareCutscene)
end

---@type LuaDataGetter
function cutsceneHelper.getAttackData(filename, data)
    return cutsceneHelper.getLuaData(filename, data, prepareAttack)
end

---@type LuaDataGetter
function cutsceneHelper.getInterruptData(filename, data)
    return cutsceneHelper.getLuaData(filename, data, prepareInterruption)
end

---@type LuaDataGetter
function cutsceneHelper.getFunctionData(filename, data)
    return cutsceneHelper.getLuaData(filename, data, prepareFunction)
end

---@type LuaDataGetter
function cutsceneHelper.getSavePointData(filename, data)
    return cutsceneHelper.getLuaData(filename, data, prepareSavePoint)
end

return cutsceneHelper
