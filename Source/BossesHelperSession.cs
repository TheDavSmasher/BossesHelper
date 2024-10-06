using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Entities;

namespace Celeste.Mod.BossesHelper;

public class BossesHelperSession : EverestModuleSession
{
    public struct BossPhase(int bossHealthAt, bool startImmediately, int startIndex)
    {
        public int bossHealthAt = bossHealthAt;

        public bool startImmediately = startImmediately;

        public int startWithPatternIndex = startIndex;
    }

    public BossPhase BossPhaseSaved { get; set; }

    public struct HealthSystemData
    {
        public int playerHealthVal;

        public string iconSprite;

        public string startAnim;

        public string endAnim;

        public float iconSeparation;

        public Vector2 healthBarPos;

        public Vector2 healthIconScale;

        public float damageCooldown;

        public bool globalController;

        public bool globalHealth;

        public HealthSystemManager.CrushEffect playerOnCrush;

        public bool isCreated;

        public bool isEnabled;

        public bool activateInstantly;

        public string activateFlag;
    }

    public HealthSystemData healthData;
}
