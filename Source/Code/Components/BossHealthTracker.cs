using Monocle;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
	[Tracked(false)]
	public class BossHealthTracker(Func<int> health) : Component(active: true, visible: false)
	{
		public readonly Func<int> Health = health;
	}
}
