---@meta SavePointShared

local savePointObj = {}

savePointObj.respawnOpts = {
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

function savePointObj.createData(name)
    local newObj = {}

    newObj.name = "BossesHelper/"..name
    newObj.depth = 0
    newObj.nodeLimits = {0, 1}
    newObj.nodeLineRenderType = "line"

    return newObj
end

return savePointObj