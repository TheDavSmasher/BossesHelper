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
        filetype = "number"
    },
    playerHealth = {
        filetype = "integer" 
    },
    damageCooldown = {
        filetype = "number"
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
        healthIconSeparation = "1.0",
        playerHealth = "3",
        damageCooldown = "1",
        crushEffect = "instantDeath",
        isGlobal = true,
        applySystemInstantly = true
    }
}

return healthSystemManager