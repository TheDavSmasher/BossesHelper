local bossMasterController = {}

local hurtModeOpts = {
    {"Player Contact", "playerContact"},
    {"Player Dash", "playerDash"},
    {"Head Bonk", "headBonk"},
    {"Sidekick Attack", "sidekickAttack"},
    {"Custom", "custom"}
}

bossMasterController.name = "BossesHelper/BossController"
bossMasterController.depth = 0
bossMasterController.nodeLineRenderType = "line"
bossMasterController.nodeLimits = {0, -1}
bossMasterController.fieldInformation = {
    bossHealthMax = {
        fieldType = "integer"
    },
    hurtMode = {
        options = hurtModeOpts,
        editable = false
    },
    bossHitCooldown = {
        fieldType = "number",
        minimumValue = 0
    },
    maxFall = {
        fieldType = "number"
    },
    baseGravityMultiplier = {
        fieldType = "number"
    },
    sidekickCooldown = {
        fieldType = "number"
    }
}
bossMasterController.placements = {
    name = "Boss Controller",
    data = {
        bossName = "boss",
        bossSprite = "",
        bossHealthMax = 3,
        hurtMode = "playerContact",
        bossHitCooldown = "0.5",
        maxFall = "90",
        baseGravityMultiplier = "1.0",
        sidekickCooldown = "5.0",
        dynamicFacing = true,
        mirrorSprite = false,
        killOnContact = false,
        startAttackingImmediately = false,
        sidekickFreeze = false
    }
}

return bossMasterController