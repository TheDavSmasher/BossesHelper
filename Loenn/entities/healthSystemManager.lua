local healthSystemManager = {}

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
    }
}
healthSystemManager.placements = {
    name = "Health System Manager",
    data = {
        healthIcon = "",
        healthIconCreateAnim = "",
        healthIconRemoveAnim = "",
        healthIconSeparation = "1.0",
        playerHealth = "3",
        damageCooldown = "1",
        isGlobal = true,
        applySystemInstantly = true
    }
}

return healthSystemManager