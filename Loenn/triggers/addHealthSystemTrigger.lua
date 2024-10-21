local addHealthSystemTrigger = {}

local crushModeOpts = {
    {"Push Out", "pushOut"},
    {"Solid On Invincible Player", "invincibleSolid"},
    {"Instant Death", "instantDeath"}
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
        isGlobal = true,
        globalHealth = false,
        applySystemInstantly = true,
        playerStagger = true,
        playerBlink = true,
        onlyOnce = true
    }
}

return addHealthSystemTrigger