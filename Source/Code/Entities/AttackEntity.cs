using Celeste.Mod.BossesHelper.Code.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using NLua;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
	internal class AttackEntity : Entity
	{
		public readonly Sprite Sprite;

		private readonly LuaFunction onCollide;

		public AttackEntity(Vector2 position, Collider attackbox, LuaFunction onPlayer, bool startCollidable, string spriteName, float xScale = 1f, float yScale = 1f)
			: base(position)
		{
			Collider = attackbox;
			Collidable = startCollidable;
			if (GFX.SpriteBank.TryCreate(spriteName, out Sprite))
			{
				Sprite.Scale = new Vector2(xScale, yScale);
				Add(Sprite);
			}
			onCollide = onPlayer;
			Add(new PlayerCollider(OnPlayer));
		}

		public void PlayAnim(string anim)
			=> Sprite.PlayOrWarn(anim);

		private void OnPlayer(Player player)
		{
			onCollide.Call(this, player);
		}

		public void SetCollisionActive(bool active)
		{
			Collidable = active;
		}
	}
}
