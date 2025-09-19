using Celeste.Mod.BossesHelper.Code.Helpers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
	public abstract class BossEntity : Actor
	{
		public readonly Sprite Sprite;

		public BossEntity(Vector2 position, string spriteName, Vector2 spriteScale, bool collidable) : base(position)
		{
			Collidable = collidable;
			if (GFX.SpriteBank.TryCreate(spriteName, out Sprite))
			{
				Sprite.Scale = spriteScale;
				Add(Sprite);
			}
		}

		public void PlayAnim(string anim)
			=> Sprite.PlayOrWarn(anim);
	}
}
