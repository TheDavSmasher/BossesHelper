using Celeste.Mod.BossesHelper.Code.Helpers.Lua;
using Monocle;
using NLua;
using System;

namespace Celeste.Mod.BossesHelper.Code.Components
{
	public abstract class StateChecker(Action<Entity> action, bool stateNeeded = true, bool removeOnComplete = true)
		: Component(active: true, visible: false)
	{
		protected readonly bool state = stateNeeded;

		internal StateChecker(LuaFunction action, bool stateNeeded = true, bool removeOnComplete = true)
			: this(action.ToAction<Entity>(), stateNeeded, removeOnComplete) { }

		public override void Update()
		{
			if (StateCheck() == state)
			{
				action.Invoke(Entity);
				if (removeOnComplete)
					RemoveSelf();
			}
		}

		protected abstract bool StateCheck();
	}
}
