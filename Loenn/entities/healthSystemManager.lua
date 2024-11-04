local healthSystemManager = {}

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

healthSystemManager.name = "BossesHelper/HealthSystemManager"
healthSystemManager.depth = 0
healthSystemManager.texture = "loenn/BossesHelper/HealthController"
healthSystemManager.nodeLimits = {0, 0}
healthSystemManager.fieldInformation = {
    healthIcons = {
        fieldType = "list",
        minimumElements = 1,
        elementOptions = {
            fieldType = "string"
        }
    },
    healthIconsCreateAnim = {
        fieldType = "list",
        minimumElements = 1,
        elementOptions = {
            fieldType = "string"
        }
    },
    healthIconsRemoveAnim = {
        fieldType = "list",
        minimumElements = 1,
        elementOptions = {
            fieldType = "string"
        }
    },
    healthIconsSeparation = {
        fieldType = "list",
        minimumElements = 1,
        elementDefault = "0",
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
    healthIconScreenX = {
        fieldType = "number",
        minimumValue = 0,
        maximumValue = 1920
    },
    healthIconScreenY = {
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

function healthSystemManager.fieldInformation.healthIconsSeparation.elementOptions.validator(string)
    res = tonumber(string)
    return res ~= nil and res >= 0
end

healthSystemManager.fieldOrder = {
    "x", "y",
    "playerHealth", "damageCooldown",
    "crushEffect", "offscreenEffect",
    "activationFlag", "onDamageFunction",
    "healthIcons", "healthIconsCreateAnim",
    "healthIconsSeparation", "healthIconsRemoveAnim",
    "healthIconsScreenX", "healthIconsScreenY",
    "healthIconsScaleX", "healthIconsScaleY",
    "isGlobal", "globalHealth", "applySystemInstantly", "playerBlink",
    "playerStagger", "startVisible"
}

healthSystemManager.placements = {
    name = "Health System Manager",
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
        playerBlink = true
    }
}

return healthSystemManager