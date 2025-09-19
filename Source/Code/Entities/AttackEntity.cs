using Microsoft.Xna.Framework;
using Monocle;
using NLua;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
	public class AttackEntity : BossEntity
	{
		private readonly LuaFunction onCollide;

		public AttackEntity(Vector2 position, Collider attackbox, LuaFunction onPlayer, bool startCollidable, string spriteName, float xScale = 1f, float yScale = 1f)
			: base(position, spriteName, new Vector2(xScale, yScale), startCollidable)
		{
			Collider = attackbox;
			onCollide = onPlayer;
			Add(new PlayerCollider(OnPlayer));
		}

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
