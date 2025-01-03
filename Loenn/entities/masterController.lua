local bossMasterController = {}

local hurtModeOpts = {
    {"Player Contact", "playerContact"},
    {"Player Dash", "playerDash"},
    {"Head Bonk", "headBonk"},
    {"Sidekick Attack", "sidekickAttack"},
    {"Custom", "custom"}
}

bossMasterController.name = "BossesHelper/BossController"
bossMasterController.depth = 0
bossMasterController.texture = "loenn/BossesHelper/BossController"
--bossMasterController.justification = {0.5, 1.0}
--bossMasterController.nodeLineRenderType = "line"
bossMasterController.nodeLimits = {0, 0}
bossMasterController.fieldInformation = {
    bossHealthMax = {
        fieldType = "integer"
    },
    __Boss_pad = {
        fieldType = "spacer"
    },
    hurtMode = {
        options = hurtModeOpts,
        editable = false
    },
    bossHitCooldown = {
        fieldType = "number",
        minimumValue = 0
    },
    maxFall = {
        fieldType = "number"
    },
    baseGravityMultiplier = {
        fieldType = "number"
    },
    sidekickCooldown = {
        fieldType = "number"
    },
    hitboxMetadataPath = {
        fieldType = "path",
        allowedExtensions = {"xml"},
        allowMissingPath = false
    },
    patternsPath = {
        fieldType = "path",
        allowedExtensions = {"xml"},
        allowMissingPath = false
    },
    functionsPath = {
        fieldType = "path",
        allowedExtensions = {"lua"},
        allowMissingPath = false
    },
    attacksPath = {
        fieldType = "path",
        allowFolders = true,
        allowFiles = false
    },
    eventsPath = {
        fieldType = "path",
        allowFolders = true,
        allowFiles = false
    }
}

bossMasterController.fieldOrder = {
    "x", "y", "bossID", "bossSprite", "bossHealthMax", "hurtMode", "maxFall", "bossHitCooldown", "baseGravityMultiplier", "sidekickCooldown",
    "hitboxMetadataPath", "patternsPath", "attacksPath", "eventsPath", "functionsPath", "__Boss_pad",
    "dynamicFacing", "mirrorSprite", "killOnContact", "startAttackingImmediately", "sidekickFreeze"
}

bossMasterController.placements = {
    name = "Boss Controller",
    data = {
        hitboxMetadataPath = "",
        attacksPath = "",
        eventsPath = "",
        functionsPath = "",
        patternsPath = "",
        bossID = "",
        bossSprite = "",
        bossHealthMax = 3,
        hurtMode = "playerContact",
        bossHitCooldown = "0.5",
        maxFall = "90",
        baseGravityMultiplier = "1.0",
        sidekickCooldown = "5.0",
        dynamicFacing = true,
        mirrorSprite = false,
        killOnContact = false,
        startAttackingImmediately = false,
        sidekickFreeze = false
    }
}

--[[function bossMasterController.texture(room, entity)
    local texture = string.format("characters/%s00", entity.bossSprite or "")

    return (entity.bossSprite == "") and texture or "loenn/BossesHelper/BossController"
end]]--

return bossMasterController