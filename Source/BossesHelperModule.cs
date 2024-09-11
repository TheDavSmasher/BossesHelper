using System;
using System.Collections.Generic;
using Celeste.Mod.BossesHelper.Code.Other;

namespace Celeste.Mod.BossesHelper;

public class BossesHelperModule : EverestModule {
    public static BossesHelperModule Instance { get; private set; }

    // Store Settings
    public override Type SettingsType => typeof(BossesHelperModuleSettings);
    public static BossesHelperModuleSettings Settings => (BossesHelperModuleSettings) Instance._Settings;

    // Store Session
    public override Type SessionType => typeof(BossesHelperModuleSession);
    public static BossesHelperModuleSession Session => (BossesHelperModuleSession) Instance._Session;

    // Store Save Data
    public override Type SaveDataType => typeof(BossesHelperModuleSaveData);
    public static BossesHelperModuleSaveData SaveData => (BossesHelperModuleSaveData) Instance._SaveData;

    public BossesHelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(BossesHelperModule), LogLevel.Info);
        Logger.Log("BossesHelper", "BossesHelper Loaded!");
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(BossesHelperModule), LogLevel.Info);
#endif
    }

    public override void LoadContent(bool firstLoad)
    {
        base.LoadContent(firstLoad);
        BossEvent.WarmUp();
    }

    public override void Load() {
        // TODO: apply any hooks that should always be active
    }

    public override void Unload() {
        // TODO: unapply any hooks applied in Load()
    }

    public static EntityData MakeEntityData()
    {
        EntityData entityData = new EntityData();
        entityData.Values = new Dictionary<string, object>();
        return entityData;
    }
}