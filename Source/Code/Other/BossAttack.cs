using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers;
using NLua;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;

namespace Celeste.Mod.BossesHelper.Code.Other
{
    public class BossAttack(string filepath, Player player, BossController controller): IBossAction
    {
        private readonly LuaFunction attackFunction = LoadLuaFile(filepath, "getAttackData", new Dictionary<object, object>
            {
                { "player", player },
                { "bossID", controller.BossID },
                { "puppet", controller.Puppet },
                { "boss", controller },
                { "modMetaData", BossesHelperModule.Instance.Metadata }
            }
        )?.FirstOrDefault();

        public IEnumerator Perform()
        {
            yield return attackFunction.ToIEnumerator();
        }
    }
}
