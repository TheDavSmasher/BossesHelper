using Celeste.Mod.BossesHelper.Code.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper;

public class BossesHelperSession : EverestModuleSession
{
	#region Health System
	public bool wasOffscreen;

	public bool useFakeDeath;

	public bool fakeDeathRespawn;

	public Vector2 lastSafePosition;

	public Vector2 lastSpawnPoint;

	public Vector2 SafeSpawn
	{
		set => lastSafePosition = lastSpawnPoint = value.NearestWhole();
	}

	public bool alreadyFlying;

	public float damageCooldown;

	public struct HealthSystemData
	{
		//Overhead
		public bool isCreated;

		public bool isEnabled;

		//Damage
		public bool activateInstantly;

		public bool globalController;

		public bool globalHealth;

		public string activateFlag;

		public int playerHealthVal;

		public float damageCooldown;

		public bool playerStagger;

		public bool playerBlink;

		public string onDamageFunction;

		public HealthSystemManager.CrushEffect playerOnCrush;

		public HealthSystemManager.OffscreenEffect playerOffscreen;

		public string fakeDeathMethods;

		public readonly List<string> FakeDeathMethods => SeparateList(fakeDeathMethods);

		//Visual
		public string iconSprite;

		public string startAnim;

		public string endAnim;

		public string iconSeparation;

		public string frameSprite;

		public Vector2 healthBarPos;

		public Vector2 healthIconScale;

		public bool startVisible;

		public bool removeOnDamage;
	}

	public HealthSystemData healthData;

	public int currentPlayerHealth;

	public bool safeGroundBlockerCreated;

	public bool globalSafeGroundBlocker;
	#endregion

	#region Boss Phase
	public readonly record struct BossPhase(int BossHealthAt, bool StartImmediately, int StartWithPatternIndex);

	public Dictionary<string, BossPhase> BossPhasesSaved = [];
	#endregion

	#region Global Save Point
	public bool savePointSet;

	public bool travelingToSavePoint;

	public string savePointLevel;

	public Vector2 savePointSpawn;

	public Player.IntroTypes savePointSpawnType;
	#endregion
}
