using NLua;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.BossesHelper.Code.Helpers;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    internal class AttackEntity : Entity
    {
        private readonly Sprite sprite;

        private readonly LuaFunction onCollide;

        public AttackEntity(Vector2 position, Collider attackbox, LuaFunction onPlayer, bool startCollidable, string spriteName, float xScale = 1f, float yScale = 1f)
            : base(position)
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
            if (!sprite.TryPlay(anim))
            {
                Logger.Log(LogLevel.Warn, "BossesHelper/AttackEntity", "Animation specified does not exist!");
            }
        }

        private void OnPlayer(Player player)
        {
            onCollide.Call(player, this);
        }

        public void SetCollisionActive(bool active)
        {
            base.Collidable = active;
        }
    }
}
