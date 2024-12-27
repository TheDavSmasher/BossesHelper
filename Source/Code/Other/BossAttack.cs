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
        private LuaFunction attackFunction;

        public readonly BossController.AttackDelegates Delegates;

        private void LoadAttacks(string filename, string bossId, Player player, BossPuppet puppet)
        {
            Dictionary<object, object> dict = new Dictionary<object, object>
            {
                { "player", player },
                { "bossID", bossId },
                { "puppet", puppet },
                { "bossAttack", Delegates },
                { "modMetaData", BossesHelperModule.Instance.Metadata }
            };
            LuaFunction[] array = LoadLuaFile(filename, "getAttackData", dict);
            if (array != null)
            {
                attackFunction = array.ElementAtOrDefault(0);
            }
        }

        public BossAttack(string filepath, string bossId, BossController.AttackDelegates allDelegates)
        {
            Delegates = allDelegates;
            LoadAttacks(filepath, bossId, allDelegates.playerRef, allDelegates.puppetRef);
        }

        public IEnumerator Coroutine()
        {
            yield return attackFunction.LuaFunctionToIEnumerator();
        }
    }
}
