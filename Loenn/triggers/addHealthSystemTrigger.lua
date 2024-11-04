local addHealthSystemTrigger = {}

local crushModeOpts = {
    {"Push Out", "pushOut"},
    {"Solid On Invincible Player", "invincibleSolid"},
    {"Instant Death", "instantDeath"},
    {"Use Old Value", ""}
}

local offscreenModeOpts = {
    {"Bounce Up", "bounceUp"},
    {"Bubble Back", "bubbleBack"},
    {"Instant Death", "instantDeath"},
    {"Use Old Value", ""}
}

addHealthSystemTrigger.name = "BossesHelper/AddHealthSystemTrigger"
addHealthSystemTrigger.depth = 0
addHealthSystemTrigger.nodeLimits = {0, 0}
addHealthSystemTrigger.fieldInformation = {
    healthIcons = {
        fieldType = "list",
        elementOptions = {
            fieldType = "string"
        }
    },
    healthIconsCreateAnim = {
        fieldType = "list",
        elementOptions = {
            fieldType = "string"
        }
    },
    healthIconsRemoveAnim = {
        fieldType = "list",
        elementOptions = {
            fieldType = "string"
        }
    },
    healthIconsSeparation = {
        fieldType = "list",
        elementDefault = "0.0",
        elementOptions = {}
    },
    playerHealth = {
        fieldType = "integer",
        minimumValue = 2
    },
    damageCooldown = {
        fieldType = "number",
        minimumValue = 0
    },
    healthIconsScreenX = {
        fieldType = "number",
        minimumValue = 0,
        maximumValue = 1920
    },
    healthIconsScreenY = {
        fieldType = "number",
        minimumValue = 0,
        maximumValue = 1080
    },
    crushEffect = {
        options = crushModeOpts,
        editable = false
    },
    offscreenEffect = {
        options = offscreenModeOpts,
        editable = false
    }
}

function addHealthSystemTrigger.fieldInformation.healthIconsSeparation.elementOptions.validator(string)
    res = tonumber(string)
    return res ~= nil and res >= 0
end

addHealthSystemTrigger.fieldOrder = {
    "x", "y",
    "width", "height",
    "playerHealth", "damageCooldown",
    "crushEffect", "offscreenEffect",
    "activationFlag", "onDamageFunction",
    "healthIcons", "healthIconsCreateAnim",
    "healthIconsSeparation", "healthIconsRemoveAnim",
    "healthIconsScreenX", "healthIconsScreenY",
    "healthIconsScaleX", "healthIconsScaleY",
    "isGlobal", "globalHealth", "applySystemInstantly", "playerBlink",
    "playerStagger", "startVisible", "onlyOnce"
}

addHealthSystemTrigger.placements = {
    name = "Add Health System Trigger",
    data = {
        activationFlag = "",
        healthIcons = "",
        healthIconsCreateAnim = "",
        healthIconsRemoveAnim = "",
        healthIconsScreenX = "160",
        healthIconsScreenY = "950",
        healthIconsScaleX = "1",
        healthIconsScaleY = "1",
        healthIconsSeparation = "1.0",
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
        onlyOnce = true
    }
}

return addHealthSystemTrigger