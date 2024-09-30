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
        fieldType = "integer" 
    },
    damageCooldown = {
        fieldType = "number"
    },
    healthIconScreenX = {
        fieldType = "number"
    },
    healthIconScreenY = {
        fieldType = "number"
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