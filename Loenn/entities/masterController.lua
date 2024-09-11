local bossMasterController = {}

local moveModeOpts = { "nodes", "screenEdge", "playerScreenEdge", "playerPos", "static", "freeroam" }

bossMasterController.name = "BossesHelper/MasterController"
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
    } 
}
bossMasterController.placements = {
    name = "Master Controller",
    data = {
        bossName = "boss",
        bossSprite = "",
        bossHealthMax = 3,
        moveMode = "Nodes",
        dynamicFacing = true,
        mirrorSprite = false,
        killOnContact = false,
        startAttackingImmediately = false
    }
}

return bossMasterController