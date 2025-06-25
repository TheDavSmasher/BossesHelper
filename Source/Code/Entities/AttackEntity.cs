using NLua;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.BossesHelper.Code.Helpers;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    internal class AttackEntity : Entity
    {
        public readonly Sprite Sprite;

        private readonly LuaFunction onCollide;

        public AttackEntity(Vector2 position, Collider attackbox, LuaFunction onPlayer, bool startCollidable, string spriteName, float xScale = 1f, float yScale = 1f)
            : base(position)
        {
            base.Collider = attackbox;
            base.Collidable = startCollidable;
            if (GFX.SpriteBank.TryCreate(spriteName, out Sprite))
            {
                Sprite.Scale = new Vector2(xScale, yScale);
                Add(Sprite);
            }
            onCollide = onPlayer;
            Add(new PlayerCollider(OnPlayer));
        }

        public void PlayAnim(string anim)
        {
            if (!Sprite.TryPlay(anim))
            {
                Logger.Log(LogLevel.Warn, "BossesHelper/AttackEntity", "Animation specified does not exist!");
            }
        }

        private void OnPlayer(Player player)
        {
            onCollide.Call(this, player);
        }

        public void SetCollisionActive(bool active)
        {
            base.Collidable = active;
        }
    }
}
