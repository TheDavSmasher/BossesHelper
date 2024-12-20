using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    public class GlobalSavePoint : Entity
    {
        public EntityID ID;

        public Vector2 spawnPoint;

        public Player.IntroTypes spawnType;

        public Sprite savePointSprite;

        public GlobalSavePoint(EntityData entityData, Vector2 offset, EntityID id)
            : base(entityData.Position + offset)
        {
            this.ID = id;
            spawnPoint = entityData.Nodes[0];
            spawnType = entityData.Enum("respawnType", Player.IntroTypes.Respawn);
            string spriteName = entityData.Attr("savePointSprite");
            if (!string.IsNullOrEmpty(spriteName))
            {
                savePointSprite = GFX.SpriteBank.Create(spriteName);
                Add(savePointSprite);
            }
        }

        public void Interact(Player player)
        {
            BossesHelperModule.Session.savePointLevel = ID.Level;
            BossesHelperModule.Session.savePointSpawn = spawnPoint;
            BossesHelperModule.Session.savePointSpawnType = spawnType;
        }
    }
}
