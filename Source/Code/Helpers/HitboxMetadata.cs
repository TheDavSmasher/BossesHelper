using Monocle;
using System.Collections.Generic;
using static Celeste.Mod.BossesHelper.Code.Entities.BossPuppet;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
	public class HitboxMetadata
	{
		public readonly EnumDict<ColliderOption, Dictionary<string, Collider>> ColliderOptions = new(_ => []);

		private readonly Dictionary<string, EnumDict<ColliderOption, Collider>> TagGroups = [];

		private readonly HashSet<string> TagGroupKeepOld = [];

		public Dictionary<string, Collider> this[ColliderOption opt]
			=> ColliderOptions[opt];

		public EnumDict<ColliderOption, Collider> this[string tag]
			=> TagGroups[tag];

		public void Add(ColliderOption option, string tag, Collider collider)
		{
			if (collider == null || this[option].TryAdd(tag, collider))
				return;
			if (this[option][tag] is ColliderList list)
				list.Add(collider);
			else
				this[option][tag] = new ColliderList(this[option][tag], collider);
		}

		public void Add(string tag, ColliderOption option, Collider collider)
		{
			if (collider == null ||
				TagGroups.TryAdd(tag, new(opt => opt == option ? collider : null)) ||
				this[tag].TryAdd(option, collider))
				return;
			if (this[tag][option] is ColliderList list)
				list.Add(collider);
			else
				this[tag][option] = new ColliderList(this[tag][option], collider);
		}
	}
}
