using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers;
using NLua;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;

namespace Celeste.Mod.BossesHelper.Code.Other
{
    public class BossAttack
    {
        private readonly LuaFunction attackFunction;

        public BossAttack(string filepath, string bossId, Player player,
            BossPuppet puppet, BossController.AttackDelegates delegates)
        {
            Dictionary<object, object> dict = new Dictionary<object, object>
            {
                { "player", player },
                { "bossID", bossId },
                { "puppet", puppet },
                { "bossAttack", delegates },
                { "modMetaData", BossesHelperModule.Instance.Metadata }
            };
            LuaFunction[] array = LoadLuaFile(filepath, "getAttackData", dict);
            attackFunction = array?.ElementAtOrDefault(0);
        }

        public IEnumerator Coroutine()
        {
            yield return attackFunction.ToIEnumerator();
        }
    }
}
