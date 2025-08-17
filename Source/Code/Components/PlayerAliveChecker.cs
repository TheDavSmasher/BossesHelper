using Monocle;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
	[Tracked(false)]
	public class PlayerAliveChecker(Action onDeath) : Component(true, false)
	{
		public readonly Action OnPlayerDeath = onDeath;
	}
}
