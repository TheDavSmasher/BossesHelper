local bossMasterController = {}

local moveModeOpts = {
    {"Nodes", "nodes"},
    {"Screen Edge", "screenEdge"},
    {"Player Screen Edge", "playerScreenEdge"},
    {"Player Position", "playerPos"},
    {"Static", "static"},
    {"Freeroam", "freeroam"}
}

local hurtModeOpts = {
    {"Player Contact", "playerContact"},
    {"Player Dash", "playerDash"},
    {"Explosion", "explosion"},
    {"Head Bonk", "headBonk"},
    {"Sidekick Attack", "sidekickAttack"}
}

bossMasterController.name = "BossesHelper/BossController"
bossMasterController.depth = 0
bossMasterController.nodeLineRenderType = "line"
bossMasterController.nodeLimits = {1, -1}
bossMasterController.fieldInformation = {
    bossHealthMax = {
        fieldType = "integer"
    },
    moveMode = {
        options = moveModeOpts,
        editable = false
    },
    hurtMode = {
        options = hurtModeOpts,
        editable = false
    },
    bossHitCooldown = {
        fieldType = "number"
    }
}
bossMasterController.placements = {
    name = "Boss Controller",
    data = {
        bossName = "boss",
        bossSprite = "",
        bossHealthMax = 3,
        moveMode = "nodes",
        hurtMode = "playerContact",
        bossHitCooldown = "0.5",
        dynamicFacing = true,
        mirrorSprite = false,
        killOnContact = false,
        startAttackingImmediately = false
    }
}

return bossMasterController