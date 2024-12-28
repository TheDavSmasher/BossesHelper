local autoSavePointSet = {}

local respawnOpts = {
    "Transition",
    "Respawn",
    "WalkInRight",
    "WalkInLeft",
    "Jump",
    "WakeUp",
    "Fall",
    "TempleMirrorVoid",
    "None",
    "ThinkForABit"
}

autoSavePointSet.name = "BossesHelper/AutoSavePointSet"
autoSavePointSet.depth = 0
--autoSavePointSet.texture = ""
autoSavePointSet.nodeLimits = {0, 1}
bossHealthBar.nodeLineRenderType = "line"
autoSavePointSet.fieldInformation = {
    respawnType = {
        options = respawnOpts,
        editable = false
    }
}
autoSavePointSet.placements = {
    name = "Auto Save Point Set",
    data = {
        respawnType = "Respawn",
        onlyOnce = true
    }
}

return autoSavePointSet