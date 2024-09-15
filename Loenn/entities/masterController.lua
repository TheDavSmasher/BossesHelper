local bossMasterController = {}

local moveModeOpts = { "nodes", "screenEdge", "playerScreenEdge", "playerPos", "static", "freeroam" }

local hurtModeOpts = { "playerContact", "playerDash", "explosion", "headBonk", "sidekickAttack" }

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