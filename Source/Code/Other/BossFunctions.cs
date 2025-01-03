using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Monocle;
using NLua;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;


namespace Celeste.Mod.BossesHelper.Code.Other
{
    internal class BossFunctions
    {
        public enum DamageSource
        {
            Contact,
            Dash,
            Bounce,
            Laser
        }

        private LuaFunction OnContactLua;

        private LuaFunction OnDashLua;

        private LuaFunction OnBounceLua;

        private LuaFunction OnLaserLua;

        public readonly BossController.OnHitDelegates Delegates;

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
            LuaFunction[] array = LoadLuaFile(filename, "getInterruptData", dict);
            if (array != null)
            {
                LuaFunction OnHitLua = array.ElementAtOrDefault(0);
                OnContactLua = array.ElementAtOrDefault(1) ?? OnHitLua;
                OnDashLua = array.ElementAtOrDefault(2) ?? OnHitLua;
                OnBounceLua = array.ElementAtOrDefault(3) ?? OnHitLua;
                OnLaserLua = array.ElementAtOrDefault(4) ?? OnHitLua;
                array.ElementAtOrDefault(5)?.Call();
            }
        }

        public BossFunctions(string filepath, string bossId, BossController.OnHitDelegates delegates)
        {
            Delegates = delegates;
            LoadMethods(filepath, bossId, delegates.playerRef, delegates.puppetRef);
        }

        public Coroutine OnDamageCoroutine(DamageSource source)
        {
            return new Coroutine(OnDamage(source));
        }

        private IEnumerator OnDamage(DamageSource source)
        {
            yield return (source switch
            {
                DamageSource.Contact => OnContactLua,
                DamageSource.Dash => OnDashLua,
                DamageSource.Bounce => OnBounceLua,
                DamageSource.Laser => OnLaserLua,
                _ => null
            })?.ToIEnumerator();
        }
    }
}
