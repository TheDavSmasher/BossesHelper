using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [Tracked(false)]
    [CustomEntity("BossesHelper/PlayerSavePoint")]
    public class GlobalSavePoint : Actor
    {
        private readonly GlobalSavePointChanger Changer;

        private readonly Sprite savePointSprite;

        public GlobalSavePoint(EntityData entityData, Vector2 offset)
            : base(entityData.Position + offset)
        {
            Add(Changer = new(entityData.Level, entityData.Nodes[0], entityData.Enum("respawnType", Player.IntroTypes.Respawn)));
            string spriteName = entityData.String("savePointSprite");
            if (!string.IsNullOrEmpty(spriteName))
            {
                savePointSprite = GFX.SpriteBank.Create(spriteName);
                Add(savePointSprite);
            }
        }

        public void Interact(Player player)
        {
            Changer.Update();
        }
    }
}
