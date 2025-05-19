using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;
using System.Collections.Generic;

namespace Celeste.Mod.BossesHelper;

public class BossesHelperSession : EverestModuleSession
{
    #region Health System
    public bool wasOffscreen;

    public bool useFakeDeath;

    public bool fakeDeathRespawn;

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

        public string[] fakeDeathMethods;

        //Visual
        public string iconSprite;

        public string startAnim;

        public string endAnim;

        public string iconSeparation;

        public string frameSprite;

        public Vector2 healthBarPos;

        public Vector2 healthIconScale;

        public bool startVisible;

        public bool removeOnDamage;
    }

    public HealthSystemData healthData;

    public int currentPlayerHealth;

    public bool safeGroundBlockerCreated;

    public bool globalSafeGroundBlocker;
    #endregion

    #region Boss Phase
    public struct BossPhase(int bossHealthAt, bool startImmediately, int startIndex)
    {
        public int bossHealthAt = bossHealthAt;

        public bool startImmediately = startImmediately;

        public int startWithPatternIndex = startIndex;
    }

    public Dictionary<string, BossPhase> BossPhasesSaved = [];
    #endregion

    #region Global Save Point
    public bool savePointSet;

    public bool travelingToSavePoint;

    public string savePointLevel;

    public Vector2 savePointSpawn;

    public Player.IntroTypes savePointSpawnType;
    #endregion
}
