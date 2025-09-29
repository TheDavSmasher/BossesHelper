--Lua Files obtained from Lua Cutscenes mod, reformatted to fit Bosses Helper

local cutsceneHelper = {}

---@module "Celeste.Mod"
local celesteMod = require("#celeste.mod")

local function getMod(modName)
    return celesteMod[modName] --[[@as Celeste.Mod.BossesHelper]]
end

--#region Coroutine

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

---@param func function
---@return LuaCoroutineProxy?
function cutsceneHelper.getProxyTable(func)
    return func and {value = coroutine.create(func), resume = threadProxyResume}
end

--#endregion

--#region Lua Preparers

---@class Preparers
cutsceneHelper.luaPreparers = {}

---@type LuaPreparer
function cutsceneHelper.luaPreparers.getCutsceneData(env, func)
    local success, onBegin, onEnd = pcall(func)

    if success then
        onEnd = onEnd or env.onEnd
        onBegin = onBegin or env.onBegin

        return onBegin, onEnd
    end

    celesteMod.Logger.Error("Bosses Helper", "Failed to load cutscene in Lua: " .. onBegin)
    return success
end

---@type LuaPreparer
function cutsceneHelper.luaPreparers.getAttackData(env, func)
    local success, onBegin, onEnd, onComplete, onInterrupt, onDeath = pcall(func)

    if success then
        onBegin = onBegin or env.onBegin
        onEnd = onEnd or env.onEnd
        onComplete = onComplete or env.onComplete
        onInterrupt = onInterrupt or env.onInterrupt
        onDeath = onDeath or env.onDeath

        return onBegin, onEnd, onComplete, onInterrupt, onDeath
    end

    celesteMod.Logger.Error("Bosses Helper", "Failed to load attack in Lua: " .. onBegin)
    return success
end

---@type LuaPreparer
function cutsceneHelper.luaPreparers.getInterruptData(env, func)
    local success, onHit, onContact, onDash, onBounce, onLaser, setup = pcall(func)

    if success then
        onHit = onHit or env.onHit
        onContact = onContact or env.onContact
        onDash = onDash or env.onDash
        onBounce = onBounce or env.onBounce
        onLaser = onLaser or env.onLaser
        setup = setup or env.setup

        return setup, onHit, onContact, onDash, onBounce, onLaser
    end

    celesteMod.Logger.Error("Bosses Helper", "Failed to load interrupt data in Lua: " .. onHit)
    return success
end

---@type LuaPreparer
function cutsceneHelper.luaPreparers.getFunctionData(env, func)
    local success, onDamage, onRecover = pcall(func)

    if success then
        onDamage = onDamage or env.onDamage
        onRecover = onRecover or env.onRecover

        return onDamage, onRecover
    end

    celesteMod.Logger.Error("Bosses Helper", "Failed to load on damage function in Lua: " .. onDamage)
    return success
end

---@type LuaPreparer
function cutsceneHelper.luaPreparers.getSavePointData(env, func)
    local success, onTalk = pcall(func)

    if success then
        onTalk = onTalk or env.onTalk
        
        return onTalk
    end

    celesteMod.Logger.Error("Bosses Helper", "Failed to load on save point function in Lua: " .. onTalk)
    return success
end

--#endregion

--#region Lua Data Getters

local function readFile(filename, modName)
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

local function getLuaEnv(data)
    local env = data or {}

    setmetatable(env, {__index = _G})

    return env
end

---@type LuaGetData
function cutsceneHelper.getLuaData(filename, data, preparationFunc)
    preparationFunc = preparationFunc or function() end

    local modName = data.modMetaData.Name
    local env = getLuaEnv(data)
    local content = readFile(filename, modName)

    if content then
        addHelperFunctions(modName, env)

        local func = load(content, nil, nil, env) --[[@as function]]

        return env, preparationFunc(env, func)
    end
end

--#endregion

return cutsceneHelper
