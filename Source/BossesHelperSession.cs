using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
using YamlDotNet.Serialization;

namespace Celeste.Mod.BossesHelper;

public class BossesHelperSession : EverestModuleSession
{
    #region Health System
    public bool wasOffscreen;

    public Vector2 lastSafePosition;

    public Vector2 lastSpawnPoint;

    public Vector2 SafeSpawn
    {
        set
        {
            lastSafePosition = value;
            lastSpawnPoint = value;
        }
    }

    public bool alreadyFlying;

    public float damageCooldown;

    public struct HealthSystemData
    {
        //Overhead
        public bool isCreated;

        public bool isEnabled;

        //Damage
        public bool activateInstantly;

        public bool globalController;

        public bool globalHealth;

        public string activateFlag;

        public int playerHealthVal;

        public float damageCooldown;

        public bool playerStagger;

        public bool playerBlink;

        public string onDamageFunction;

        public HealthSystemManager.CrushEffect playerOnCrush;

        public HealthSystemManager.OffscreenEffect playerOffscreen;

        //Visual
        public string iconSprite;

        public string startAnim;

        public string endAnim;

        public string iconSeparation;

        public Vector2 healthBarPos;

        public Vector2 healthIconScale;

        public bool startVisible;
    }

    public HealthSystemData healthData;

    public int currentPlayerHealth;

    [YamlIgnore]
    public HealthSystemManager mapHealthSystemManager;

    [YamlIgnore]
    public DamageController mapDamageController;

    [YamlIgnore]
    public PlayerHealthBar mapHealthBar;

    public bool safeGroundBlockerCreated;

    public bool globalSafeGroundBlocker;

    [YamlIgnore]
    public UpdateSafeBlocker mapUpdateSafeBlocker;
    #endregion

    #region Boss Phase
    public struct BossPhase(int bossHealthAt, bool startImmediately, int startIndex)
    {
        public int bossHealthAt = bossHealthAt;

        public bool startImmediately = startImmediately;

        public int startWithPatternIndex = startIndex;
    }

    public BossPhase BossPhaseSaved { get; set; }
    #endregion

    #region Global Save Point
    public bool savePointSet;

    public bool travelingToSavePoint;

    public string savePointLevel;

    public Vector2 savePointSpawn;

    public Player.IntroTypes savePointSpawnType;
    #endregion
}
