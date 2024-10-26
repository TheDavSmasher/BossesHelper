local recoverHealthTrigger = {}

recoverHealthTrigger.name = "BossesHelper/RecoverHealthTrigger"
recoverHealthTrigger.depth = 0
recoverHealthTrigger.nodeLimits = {0, 0}
recoverHealthTrigger.fieldInformation = {
    healAmount = {
        fieldType = number,
        minimumValue = 1
    }
}
recoverHealthTrigger.placements = {
    name = "Recover Health Trigger",
    data = {
        healAmount = 1,
        onlyOnce = true,
        permanent = false
    }
}

return recoverHealthTrigger