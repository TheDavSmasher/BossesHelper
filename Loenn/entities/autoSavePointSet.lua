local mods = require("mods")
---@module "SavePointShared"
local savePointObj = mods.requireFromPlugin("libraries.savePointShared")

local autoSavePointSet = savePointObj.createData("AutoSavePointSet")

autoSavePointSet.fieldInformation = {
    respawnType = {
        options = savePointObj.respawnOpts,
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