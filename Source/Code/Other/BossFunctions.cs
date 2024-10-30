using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers;
using NLua;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Celeste.Mod.BossesHelper.Code.Other
{
    internal class BossFunctions
    {
        private LuaFunction OnHitLua;

        private LuaFunction OnContactLua;

        private LuaFunction OnDashLua;

        private LuaFunction OnBounceLua;

        private LuaFunction OnLaserLua;

        private readonly string filepath;

        public BossController.OnHitDelegates Delegates { get; private set; }

        private void LoadMethods(string filename, string bossId, Player player, BossPuppet puppet)
        {
            Dictionary<object, object> dict = new Dictionary<object, object>
            {
                { "player", player },
                { "bossID", bossId },
                { "puppet", puppet },
                { "boss", Delegates },
                { "modMetaData", BossesHelperModule.Instance.Metadata }
            };
            LuaFunction[] array = LuaBossHelper.LoadLuaFile(filename, "getInterruptData", dict);
            if (array != null)
            {
                OnHitLua = array.ElementAtOrDefault(0);
                OnContactLua = array.ElementAtOrDefault(1) ?? OnHitLua;
                OnDashLua = array.ElementAtOrDefault(2) ?? OnHitLua;
                OnBounceLua = array.ElementAtOrDefault(3) ?? OnHitLua;
                OnLaserLua = array.ElementAtOrDefault(4) ?? OnHitLua;
                array.ElementAtOrDefault(5)?.Call();
            }
        }

        public BossFunctions(string filepath, string bossId, BossController.OnHitDelegates delegates)
        {
            this.filepath = filepath;
            Delegates = delegates;
            LoadMethods(filepath, bossId, delegates.playerRef, delegates.puppetRef);
        }

        public IEnumerator OnContactCoroutine()
        {
            if (OnContactLua != null)
                yield return LuaBossHelper.LuaFunctionToIEnumerator(OnContactLua);
        }

        public IEnumerator OnDashCoroutine()
        {
            if (OnDashLua != null)
                yield return LuaBossHelper.LuaFunctionToIEnumerator(OnDashLua);
        }

        public IEnumerator OnBounceCoroutine()
        {
            if (OnBounceLua != null)
                yield return LuaBossHelper.LuaFunctionToIEnumerator(OnBounceLua);
        }

        public IEnumerator OnLaserCoroutine()
        {
            if (OnLaserLua != null)
                yield return LuaBossHelper.LuaFunctionToIEnumerator(OnLaserLua);
        }
    }
}
