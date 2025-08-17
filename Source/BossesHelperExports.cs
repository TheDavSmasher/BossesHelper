using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.BossesHelper.Code.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;
using System;
using System.Reflection;

namespace Celeste.Mod.BossesHelper
{
	[ModExportName("BossesHelper")]
	public static class BossesHelperExports
	{
		public static Component GetGlobalSavePointChangerComponent(object levelNameSource, Vector2 spawnPoint, Player.IntroTypes introType = Player.IntroTypes.Respawn)
		{
			return new GlobalSavePointChanger(levelNameSource, spawnPoint, introType);
		}

		public static void CreateGlobalSavePointOnEntityOnMethod<T>(T entity, Vector2 spawnPoint, string method,
			Player.IntroTypes spawnType = Player.IntroTypes.Respawn, BindingFlags flags = BindingFlags.Default,
			bool stateMethod = false) where T : Entity
		{
			new GlobalSavePointChanger(entity, spawnPoint, spawnType)
				.AddToEntityOnMethod(entity, method, flags, stateMethod);
		}

		public static Component GetEntityChainComponent(Entity entity, bool chain, bool remove = false)
		{
			return new EntityChain(entity, chain, remove);
		}

		public static Component GetEntityTimerComponent(float timer, Action<Entity> action)
		{
			return new EntityTimer(timer, action);
		}

		public static Component GetEntityFlaggerComponent(string flag, Action<Entity> action, bool stateNeeded = true, bool resetFlag = true)
		{
			return new EntityFlagger(flag, action, stateNeeded, resetFlag);
		}

		public static Component GetBossHealthTrackerComponent(Func<int> action)
		{
			return new BossHealthTracker(action);
		}

		public static int GetCurrentPlayerHealth()
		{
			if (Engine.Scene.Tracker.GetEntity<HealthSystemManager>() != null)
				return BossesHelperModule.Session.currentPlayerHealth;
			return -1;
		}

		public static void RecoverPlayerHealth(int amount)
		{
			Engine.Scene.Tracker.GetEntity<HealthSystemManager>()?.RecoverHealth(amount);
		}

		public static void MakePlayerTakeDamage(Vector2? from = null, int amount = 1, bool silent = false, bool stagger = true, bool ignoreCooldown = false)
		{
			BossesHelperModule.PlayerTakesDamage(from, amount, silent, stagger, ignoreCooldown);
		}

		public static void UseFakeDeath()
		{
			BossesHelperModule.Session.useFakeDeath = true;
		}

		public static void ClearFakeDeath()
		{
			BossesHelperModule.Session.useFakeDeath = false;
		}
	}
}
