local globalSavePoint = {}

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
    "ThinkForABit",
    "UseOldValue"
}

globalSavePoint.name = "BossesHelper/PlayerSavePoint"
globalSavePoint.depth = 0
--globalSavePoint.texture = ""
globalSavePoint.nodeLimits = {1, 1}
bossHealthBar.nodeLineRenderType = "line"
globalSavePoint.fieldInformation = {
    respawnType = {
        options = respawnOpts,
        editable = false
    }
}
globalSavePoint.fieldOrder = {}
globalSavePoint.placements = {
    name = "Player Save Point",
    data = {
        luaFile = "",
        savePointSprite = "",
        respawnType = "Respawn"
    }
}

return globalSavePoint