using System;
using Monocle;
using MonoMod.ModInterop;
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

        public static int GetPlayerCurrentHealth()
        {
            if (BossesHelperModule.Session.mapDamageController != null)
                return BossesHelperModule.Session.mapDamageController.health;
            return -1;
        }
    }
}
