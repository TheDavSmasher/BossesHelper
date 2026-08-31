local mods = require("mods")
---@module "SavePointShared"
local savePointObj = mods.requireFromPlugin("libraries.savePointShared")

local savePointSetTrigger = savePointObj.createData("SavePointSetTrigger")

savePointSetTrigger.fieldInformation = {
    respawnType = {
        options = savePointObj.respawnOpts,
        editable = false
    }
}
savePointSetTrigger.placements = {
    name = "Save Point Set Trigger",
    data = {
        respawnType = "Respawn",
        flagTrigger = "",
        onlyOnce = true,
        invertFlag = false
    }
}

return savePointSetTrigger