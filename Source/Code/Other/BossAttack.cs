using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers;
using NLua;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.BossesHelper.Code.Other
{
    public class BossAttack
    {
        private LuaFunction attackFunction;

        private readonly string filepath;

        public BossController.AttackDelegates Delegates { get; private set; }

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
            LuaFunction[] array = LuaBossHelper.LoadLuaFile(filename, "getAttackData", dict);
            if (array != null)
            {
                attackFunction = array.ElementAtOrDefault(0);
            }
        }

        public BossAttack(string filepath, string bossId, BossController.AttackDelegates allDelegates)
        {
            this.filepath = filepath;
            Delegates = allDelegates;
            LoadAttacks(filepath, bossId, allDelegates.playerRef, allDelegates.puppetRef);
        }

        public IEnumerator Coroutine()
        {
            yield return LuaBossHelper.LuaFunctionToIEnumerator(attackFunction);
        }
    }
}
