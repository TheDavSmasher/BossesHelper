using Celeste.Mod.BossesHelper.Code.Entities;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
	public abstract class HealthBarVisibleTrigger(EntityData data, Vector2 offset, EntityID id)
		: SingleUseTrigger(data, offset, id, false, data.Bool("onlyOnce"))
	{
		private readonly bool state = data.Bool("visible");

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			if (GetHealthBar(out var healthBar))
			{
				ConsumeAfter(() => healthBar.Visible = state);
			}
		}

		protected abstract bool GetHealthBar(out Entity entity);
	}

	[CustomEntity("BossesHelper/PlayerHealthBarVisibleTrigger")]
	public class PlayerHealthBarVisibleTrigger(EntityData data, Vector2 offset, EntityID id)
		: HealthBarVisibleTrigger(data, offset, id)
	{
		protected override bool GetHealthBar(out Entity entity)
		{
			return (entity = Scene.GetEntity<HealthSystemManager>()) != null;
		}
	}

	[CustomEntity("BossesHelper/BossHealthBarVisibleTrigger")]
	public class BossHealthBarVisibleTrigger(EntityData data, Vector2 offset, EntityID id)
		: HealthBarVisibleTrigger(data, offset, id)
	{
		private readonly Vector2 node = data.Nodes[0];

		protected override bool GetHealthBar(out Entity entity)
		{
			return (entity = SceneAs<Level>().Tracker.GetNearestEntity<BossHealthBar>(node)) is BossHealthBar;
		}
	}
}
