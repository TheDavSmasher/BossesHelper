using NLua;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    internal class AttackEntity : Entity
    {
        private Sprite sprite;

        private LuaFunction onCollide;

        public AttackEntity(Vector2 position, Collider attackbox, LuaFunction onPlayer, bool startCollidable, string spriteName, float xScale = 1f, float yScale = 1f) : base(position)
        {
            base.Collider = attackbox;
            base.Collidable = startCollidable;
            sprite = GFX.SpriteBank.Create(spriteName);
            sprite.Scale = new Vector2(xScale, yScale);
            Add(sprite);
            onCollide = onPlayer;
            Add(new PlayerCollider(OnPlayer));
        }

        public void PlayAnim(string anim)
        {
            if (sprite.Has(anim))
            {
                sprite.Play(anim);
            }
            else
            {
                Logger.Log(LogLevel.Warn, "BossesHelper/AttackEntity", "Animation specified does not exist!");
            }
        }

        private void OnPlayer(Player player)
        {
            onCollide.Call(player);
        }

        public void SetCollisionActive(bool active)
        {
            base.Collidable = active;
        }
    }
}
