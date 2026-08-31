---@meta HealthSystemShared

local healthSystemObj = {}

local crushModeOpts = {
    {"Push Out", "pushOut"},
    {"Solid On Invincible Player", "invincibleSolid"},
    {"Fake Death", "fakeDeath"},
    {"Instant Death", "instantDeath"},
    {"Use Old Value", ""}
}

local offscreenModeOpts = {
    {"Bounce Up", "bounceUp"},
    {"Bubble Back", "bubbleBack"},
    {"Fake Death", "fakeDeath"},
    {"Instant Death", "instantDeath"},
    {"Use Old Value", ""}
}

function healthSystemObj.createData(name)
  local newObj = {}

  newObj.name = "BossesHelper/"..name
  newObj.depth = 0
  newObj.nodeLimits = {0, 0}
  newObj.fieldInformation = {
      healthIcons = {
          fieldType = "list",
          elementDefault = "",
          minimumElements = 1,
          elementOptions = {
              fieldType = "string"
          }
      },
      healthIconsCreateAnim = {
          fieldType = "list",
          elementDefault = "",
          elementOptions = {
              fieldType = "string"
          }
      },
      healthIconsRemoveAnim = {
          fieldType = "list",
          elementDefault = "",
          elementOptions = {
              fieldType = "string"
          }
      },
      healthIconsSeparation = {
          fieldType = "list",
          elementDefault = "0.0",
          elementOptions = {
              validator = function (string)
                  res = tonumber(string)
                  return res ~= nil and res >= 0
              end
          }
      },
      playerHealth = {
          validator = function (string)
              res = tonumber(string)
              return res == nil or res > 1 and string:find("%.") == nil
          end
      },
      damageCooldown = {
          validator = function (string)
              res = tonumber(string)
              return res == nil or res >= 0
          end
      },
      healthIconsScreenX = {
          validator = function (string)
              res = tonumber(string)
              return res == nil or (res >= 0 and res <= 1920)
          end
      },
      healthIconsScreenY = {
          validator = function (string)
              res = tonumber(string)
              return res == nil or (res >= 0 and res <= 1080)
          end
      },
      crushEffect = {
          options = crushModeOpts,
          editable = false
      },
      offscreenEffect = {
          options = offscreenModeOpts,
          editable = false
      },
      onDamageFunction = {
          fieldType = "path",
          allowedExtensions = {"lua"},
          allowMissingPath = false
      },
      __Boss_pad = {
          fieldType = "spacer"
      }
  }
  return newObj
end

return healthSystemObj