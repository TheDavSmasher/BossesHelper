using System;
using Monocle;
using MonoMod.ModInterop;
using Microsoft.Xna.Framework;
using Celeste.Mod.BossesHelper.Code.Components;

namespace Celeste.Mod.BossesHelper
{
    [ModExportName("BossesHelper")]
    public static class BossesHelperExports
    {
        public static Component GetEntityColliderComponent<T>(Action<T> onCollide) where T : Entity
        {
            return new EntityCollider<T>(onCollide);
        }

        public static Component GetEntityChainComponent(Entity entity, bool chain)
        {
            return new EntityChain(entity, chain);
        }

        public static Component GetBossHealthComponent(Func<int> action)
        {
            return new BossHealthTracker(action);
        }

        public static int GetCurrentPlayerHealth()
        {
            if (BossesHelperModule.Session.mapDamageController != null)
                return BossesHelperModule.Session.mapDamageController.health;
            return -1;
        }

        public static void RecoverPlayerHealth(int amount)
        {
            BossesHelperModule.Session.mapDamageController?.RecoverHealth(amount);
        }

        public static void MakePlayerTakeDamage(Vector2 from, int amount = 1, bool silent = false, bool stagger = true, bool ignoreCooldown = false)
        {
            BossesHelperModule.PlayerTakesDamage(from, amount, silent, stagger, ignoreCooldown);
        }
    }
}
