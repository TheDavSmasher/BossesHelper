local healthSystemManager = {}

local crushModeOpts = {
    {"Push Out", "pushOut"},
    {"Solid On Invincible Player", "invincibleSolid"},
    {"Instant Death", "instantDeath"}
}

healthSystemManager.name = "BossesHelper/HealthSystemManager"
healthSystemManager.depth = 0
healthSystemManager.nodeLimits = {0, 0}
healthSystemManager.fieldInformation = {
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
healthSystemManager.placements = {
    name = "Health System Manager",
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
        applySystemInstantly = true
    }
}

return healthSystemManager