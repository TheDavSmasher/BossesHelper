using Monocle;
using System.Collections.Generic;
using System.Xml.Linq;
using static Celeste.Mod.BossesHelper.Code.Entities.BossPuppet;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
	public class HitboxMetadata
	{
		public readonly EnumDict<ColliderOption, Dictionary<string, Collider>> ColliderOptions = new(_ => []);

		public Dictionary<string, Collider> this[ColliderOption opt]
			=> ColliderOptions[opt];

		public void Add(ColliderOption option, string tag, Collider collider)
		{
			if (collider == null || this[option].TryAdd(tag, collider))
				return;
			if (this[option][tag] is ColliderList list)
				list.Add(collider);
			else
				this[option][tag] = new ColliderList(this[option][tag], collider);
		}
	}
}
