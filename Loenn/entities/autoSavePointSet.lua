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
autoSavePointSet.fieldInformation = {
    respawnType = {
        options = respawnOpts,
        editable = false
    }
}
autoSavePointSet.placements = {
    name = "Auth Save Point Set",
    data = {
        respawnType = "Respawn"
    }
}

return autoSavePointSet