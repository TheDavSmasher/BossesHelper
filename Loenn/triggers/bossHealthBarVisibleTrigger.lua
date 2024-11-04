local bossHealthVisible = {}

bossHealthVisible.name = "BossesHelper/BossHealthBarVisibleTrigger"
bossHealthVisible.depth = 0
bossHealthVisible.nodeLimits = {1, 1}
bossHealthVisible.nodeLineRenderType = "line"
bossHealthVisible.placements = {
    name = "Boss Health Bar Visible Trigger",
    data = {
        state = true,
        onlyOnce = true
    }
}

return bossHealthVisible