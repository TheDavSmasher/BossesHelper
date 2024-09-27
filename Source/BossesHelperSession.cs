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
}
