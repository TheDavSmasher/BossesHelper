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

        private readonly LuaFunction OnContactLua;

        private readonly LuaFunction OnDashLua;

        private readonly LuaFunction OnBounceLua;

        private readonly LuaFunction OnLaserLua;

        public BossFunctions(string filepath, string bossId, Player player,
            BossPuppet puppet, BossController.OnHitDelegates delegates)
        {
            Dictionary<object, object> dict = new Dictionary<object, object>
            {
                { "player", player },
                { "bossID", bossId },
                { "puppet", puppet },
                { "boss", delegates },
                { "modMetaData", BossesHelperModule.Instance.Metadata }
            };
            if (LoadLuaFile(filepath, "getInterruptData", dict) is LuaFunction[] array)
            {
                LuaFunction OnHitLua = array.FirstOrDefault();
                OnContactLua = array.ElementAtOrDefault(1, OnHitLua);
                OnDashLua = array.ElementAtOrDefault(2, OnHitLua);
                OnBounceLua = array.ElementAtOrDefault(3, OnHitLua);
                OnLaserLua = array.ElementAtOrDefault(4, OnHitLua);
                array.ElementAtOrDefault(5)?.Call();
            }
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
