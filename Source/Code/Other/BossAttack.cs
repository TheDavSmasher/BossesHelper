using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers;
using NLua;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;

namespace Celeste.Mod.BossesHelper.Code.Other
{
    public class BossAttack(string filepath, string bossId, Player player,
        BossPuppet puppet, BossController.AttackDelegates delegates)
    {
        private readonly LuaFunction attackFunction = LoadLuaFile(filepath, "getAttackData", new Dictionary<object, object>
            {
                { "player", player },
                { "bossID", bossId },
                { "puppet", puppet },
                { "boss", delegates },
                { "modMetaData", BossesHelperModule.Instance.Metadata }
            }
        )?.FirstOrDefault();

        public IEnumerator Coroutine()
        {
            yield return attackFunction.ToIEnumerator();
        }
    }
}
