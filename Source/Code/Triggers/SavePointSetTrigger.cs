using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Triggers
{
	[CustomEntity("BossesHelper/SavePointSetTrigger")]
	public class SavePointSetTrigger(EntityData data, Vector2 offset, EntityID id)
				: SingleUseTrigger(data, offset, id, data.Bool("onlyOnce"), true)
	{
		private readonly Player.IntroTypes spawnType = data.Enum<Player.IntroTypes>("respawnType");

		private readonly Vector2? spawnPosition = data.FirstNodeNullable() + offset;

		private readonly string flagTrigger = data.String("flagTrigger");

		private readonly bool invertFlag = data.Bool("invertFlag");

		private GlobalSavePointChanger Changer;

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Add(Changer = new(ID,
				spawnPosition ?? SceneAs<Level>().Session.GetSpawnPoint(Center),
				spawnType));
		}

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			if (flagTrigger == null ||
				(player.SceneAs<Level>().Session.GetFlag(flagTrigger) ^ invertFlag))
			{
				ConsumeAfter(() => Changer?.Update());
			}
		}
	}
}
