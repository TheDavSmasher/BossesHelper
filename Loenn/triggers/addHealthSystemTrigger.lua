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

addHealthSystemTrigger.name = "BossesHelper/HealthSystemManager"
addHealthSystemTrigger.depth = 0
addHealthSystemTrigger.nodeLimits = {0, 0}
addHealthSystemTrigger.fieldInformation = {
    healthIconSeparation = {
        fieldType = "number"
    },
    playerHealth = {
        fieldType = "integer",
        minimumValue = 1
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
addHealthSystemTrigger.placements = {
    name = "Add Health System Trigger",
    data = {
        activationFlag = "",
        healthIcon = "",
        healthIconCreateAnim = "",
        healthIconRemoveAnim = "",
        healthIconScreenX = "160",
        healthIconScreenY = "950",
        healthIconScaleX = "1",
        healthIconScaleY = "1",
        healthIconSeparation = "1.0",
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