namespace Celeste.Mod.BossesHelper;

using Microsoft.Xna.Framework.Input;

public class BossesHelperModuleSettings : EverestModuleSettings {

    [DefaultButtonBinding(Buttons.LeftShoulder, Keys.Tab)]
    [SettingName("Sidekick Laser")]
    public ButtonBinding SidekickLaserBind { get; set; }
}