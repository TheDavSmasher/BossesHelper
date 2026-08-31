local mods = require("mods")
---@module "HealthSystemShared
local healthObj = mods.requireFromPlugin("libraries.healthSystemShared")

local healthSystemManager = healthObj.createData("HealthSystemManager")

healthSystemManager.texture = "loenn/BossesHelper/HealthController"

healthSystemManager.fieldOrder = {
    "x", "y",
    "playerHealth", "damageCooldown",
    "crushEffect", "offscreenEffect",
    "activationFlag", "onDamageFunction",
    "frameSprite", "__Boss_pad",
    "healthIcons", "healthIconsCreateAnim",
    "healthIconsSeparation", "healthIconsRemoveAnim",
    "healthIconsScreenX", "healthIconsScreenY",
    "healthIconsScaleX", "healthIconsScaleY",
    "isGlobal", "globalHealth", "applySystemInstantly", "startVisible",
    "playerBlink", "playerStagger"
}

healthSystemManager.placements = {
    name = "Health System Manager",
    data = {
        activationFlag = "",
        healthIcons = "",
        frameSprite = "",
        healthIconsCreateAnim = "",
        healthIconsRemoveAnim = "",
        healthIconsScreenX = "160",
        healthIconsScreenY = "950",
        healthIconsScaleX = "1",
        healthIconsScaleY = "1",
        healthIconsSeparation = "20.0",
        playerHealth = "3",
        damageCooldown = "1",
        crushEffect = "instantDeath",
        offscreenEffect = "instantDeath",
        onDamageFunction = "",
        isGlobal = true,
        globalHealth = false,
        applySystemInstantly = true,
        startVisible = true,
        playerStagger = true,
        playerBlink = true,
        removeOnDamage = true
    }
}

return healthSystemManager