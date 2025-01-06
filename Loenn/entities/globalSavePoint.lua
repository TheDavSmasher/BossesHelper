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
globalSavePoint.nodeLineRenderType = "line"
globalSavePoint.fieldInformation = {
    respawnType = {
        options = respawnOpts,
        editable = false
    },
    luaFile = {
        fieldType = "path",
        allowedExtensions = {"lua"},
        allowMissingPath = false
    }
}
--globalSavePoint.fieldOrder = {}
globalSavePoint.placements = {
    name = "Player Save Point",
    data = {
        luaFile = "",
        savePointSprite = "",
        respawnType = "Respawn"
    }
}

return globalSavePoint