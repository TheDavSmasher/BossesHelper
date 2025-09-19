using Microsoft.Xna.Framework;
using Monocle;
using NLua;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
	public class AttackActor : BossActor
	{
		private readonly LuaFunction onCollide;

		public AttackActor(Vector2 position, Collider attackbox, LuaFunction onPlayer, bool startCollidable,
			bool solidCollidable, string spriteName, float gravMult, float maxFall, float xScale = 1f, float yScale = 1f)
			: base(position, spriteName, new Vector2(xScale, yScale), maxFall, startCollidable, solidCollidable, gravMult)
		{
			Collider = attackbox;
			onCollide = onPlayer;
			Add(new PlayerCollider(OnPlayer));
		}

		private void OnPlayer(Player player)
		{
			onCollide.Call(this, player);
		}

		public void SetSolidCollisionActive(bool active)
		{
			SolidCollidable = active;
		}

		public void SetCollisionActive(bool active)
		{
			Collidable = active;
		}

		public void SetEffectiveGravityMult(float mult)
		{
			GravityMult = mult;
		}
	}
}
