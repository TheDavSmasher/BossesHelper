using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Components
{
	public class EntityChain(Entity chained, bool startChained, bool removeTogether = false)
		: Component(startChained, visible: false)
	{
		public Vector2 ChainOffset;

		public override void Added(Entity entity)
		{
			base.Added(entity);
			ChainOffset = chained.Position - Entity.Position;
		}

		public override void Update()
		{
			chained.Position = Entity.Position + ChainOffset;
		}

		public override void Removed(Entity entity)
		{
			base.Removed(entity);
			if (removeTogether)
				chained.RemoveSelf();
		}
	}
}
