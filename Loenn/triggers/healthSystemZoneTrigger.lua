local healthSystemZoneTrigger = {}

healthSystemZoneTrigger.name = "BossesHelper/HealthSystemZoneTrigger"
healthSystemZoneTrigger.depth = 0
healthSystemZoneTrigger.nodeLimits = {0, 0}
healthSystemZoneTrigger.placements = {
    name = "Health System Enabled-Zone Trigger",
    data = {
        enableState = true
    }
}

return healthSystemZoneTrigger