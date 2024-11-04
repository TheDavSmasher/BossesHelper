local healthBarVisibleTrigger = {}

healthBarVisibleTrigger.name = "BossesHelper/PlayerHealthBarVisibleTrigger"
healthBarVisibleTrigger.depth = 0
healthBarVisibleTrigger.nodeLimits = {0, 0}
healthBarVisibleTrigger.placements = {
    name = "Player Health Bar Visible Trigger",
    data = {
        state = true,
        onlyOnce = true
    }
}

return healthBarVisibleTrigger