local enableHealthTrigger = {}

enableHealthTrigger.name = "BossesHelper/HealthEnableTrigger"
enableHealthTrigger.depth = 0
enableHealthTrigger.nodeLimits = {0, 0}
enableHealthTrigger.placements = {
    name = "Enable Health Trigger",
    data = {
        enableState = true
    }
}

return enableHealthTrigger