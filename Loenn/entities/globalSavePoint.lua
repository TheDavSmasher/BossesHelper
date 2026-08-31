local mods = require("mods")
---@module "SavePointShared"
local savePointObj = mods.requireFromPlugin("libraries.savePointShared")

local globalSavePoint = savePointObj.createData("PlayerSavePoint")

globalSavePoint.fieldInformation = {
    respawnType = {
        options = savePointObj.respawnOpts,
        editable = false
    },
    luaFile = {
        fieldType = "path",
        allowedExtensions = {"lua"},
        allowMissingPath = false
    },
    rectWidth = {
        validator = function (string)
            res = tonumber(string)
            return res == nil or res >= 0 and string:find("%.") == nil
        end
    },
    rectXOffset = {
        validator = function (string)
            res = tonumber(string)
            return res == nil or res >= 0 and string:find("%.") == nil
        end
    },
    rectYOffset = {
        validator = function (string)
            res = tonumber(string)
            return res == nil or res >= 0 and string:find("%.") == nil
        end
    },
    talkerXOffset = {
        fieldType = "number",
        minimumValue = 0
    },
    talkerYOffset = {
        fieldType = "number",
        minimumValue = 0
    }
}

globalSavePoint.ignoredFields = {
    "_name", "_id", "originX", "originY", "height"
}

globalSavePoint.fieldOrder = {
    "x", "y", "luaFile", "savePointSprite", "respawnType", "rectWidth", "rectXOffset", "rectYOffset", "talkerXOffset", "talkerYOffset"
}

globalSavePoint.placements = {
    name = "Player Save Point",
    data = {
        rectWidth = "0",
        rectXOffset = "0",
        rectYOffset = "0",
        talkerXOffset = "0.0",
        talkerYOffset = "0.0",
        luaFile = "",
        savePointSprite = "",
        respawnType = "Respawn"
    }
}

return globalSavePoint