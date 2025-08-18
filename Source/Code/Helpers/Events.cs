using Celeste.Mod.BossesHelper.Code.Entities;
using System;

namespace Celeste.Mod.BossesHelper.Code.Helpers
{
	public static class Events
	{
		public static class BossActions
		{
			public static event Action<BossController, bool> OnActionChange;

			internal static void ActionStart(BossController controller)
			{
				BossActions.OnActionChange?.Invoke(controller, true);
			}

			internal static void ActionEnd(BossController controller)
			{
				BossActions.OnActionChange?.Invoke(controller, false);
			}
		}
	}
}
