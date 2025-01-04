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
--bossMasterController.nodeLineRenderType = "line"
bossMasterController.nodeLimits = {0, 0}
bossMasterController.fieldInformation = {
    bossHealthMax = {
        fieldType = "integer"
    },
    __Boss_pad = {
        fieldType = "spacer"
    },
    __Boss_pad_ = {
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

function bossMasterController.fieldOrder(entity)
    if entity.hurtMode == "sidekickAttack" then
        return {
            "x", "y", "bossID", "bossSprite", "bossHealthMax", "hurtMode", "maxFall", "bossHitCooldown", "baseGravityMultiplier", "sidekickCooldown",
            "hitboxMetadataPath", "patternsPath", "attacksPath", "eventsPath", "functionsPath", "__Boss_pad",
            "dynamicFacing", "mirrorSprite", "killOnContact", "startAttackingImmediately", "sidekickFreeze"
        }
    else
        return {
            "x", "y", "bossID", "bossSprite", "bossHealthMax", "hurtMode", "maxFall", "bossHitCooldown", "baseGravityMultiplier", "__Boss_pad",
            "hitboxMetadataPath", "patternsPath", "attacksPath", "eventsPath", "functionsPath", "__Boss_pad_",
            "dynamicFacing", "mirrorSprite", "killOnContact", "startAttackingImmediately"
        }
    end
end

bossMasterController.placements = {
    {
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
            dynamicFacing = true,
            mirrorSprite = false,
            killOnContact = false,
            startAttackingImmediately = false
        }
    },
    {
        name = "Boss Controller (Sidekick Mode)",
        data = {
            hitboxMetadataPath = "",
            attacksPath = "",
            eventsPath = "",
            functionsPath = "",
            patternsPath = "",
            bossID = "",
            bossSprite = "",
            bossHealthMax = 3,
            hurtMode = "sidekickAttack",
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
}

return bossMasterController