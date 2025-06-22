﻿using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Monocle;
using NLua;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper;
using static Celeste.Mod.BossesHelper.Code.Other.BossActions;


namespace Celeste.Mod.BossesHelper.Code.Other
{
    internal class BossFunctions : ILuaLoader
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

        public (string Name, int Count) Command => ("getInterruptData", 6);

        public BossFunctions(string filepath, Player player, BossController controller)
        {
            LuaFunction[] array = this.LoadFile(filepath, player, controller);
            LuaFunction OnHitLua = array[0];
            OnContactLua = array[1] ?? OnHitLua;
            OnDashLua = array[2] ?? OnHitLua;
            OnBounceLua = array[3] ?? OnHitLua;
            OnLaserLua = array[4] ?? OnHitLua;
            array[5]?.Call();
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
