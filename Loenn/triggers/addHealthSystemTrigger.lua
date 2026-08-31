local mods = require("mods")
---@module "HealthSystemShared
local healthObj = mods.requireFromPlugin("libraries.healthSystemShared")

local addHealthSystemTrigger = healthObj.createData("AddHealthSystemTrigger")

addHealthSystemTrigger.fieldOrder = {
    "x", "y",
    "width", "height",
    "playerHealth", "damageCooldown",
    "crushEffect", "offscreenEffect",
    "frameSprite", "__Boss_pad",
    "activationFlag", "onDamageFunction",
    "healthIcons", "healthIconsCreateAnim",
    "healthIconsSeparation", "healthIconsRemoveAnim",
    "healthIconsScreenX", "healthIconsScreenY",
    "healthIconsScaleX", "healthIconsScaleY",
    "isGlobal", "globalHealth", "applySystemInstantly", "startVisible",
    "playerStagger", "playerBlink", "onlyOnce"
}

addHealthSystemTrigger.placements = {
    name = "Add Health System Trigger",
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
        onlyOnce = true,
        removeOnDamage = true
    }
}

return addHealthSystemTrigger