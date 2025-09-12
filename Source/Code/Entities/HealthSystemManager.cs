﻿using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
	[Tracked(false)]
	[CustomEntity("BossesHelper/HealthSystemManager")]
	public partial class HealthSystemManager : GlobalEntity
	{
		private static BossesHelperSession ModSession => BossesHelperModule.Session;

		private static ref BossesHelperSession.HealthSystemData HealthData => ref ModSession.healthData;

		public new bool Visible
		{
			get => HealthBar.Visible;
			set => HealthBar.Visible = value;
		}

		public enum CrushEffect
		{
			PushOut,
			InvincibleSolid,
			FakeDeath,
			InstantDeath
		}

		public enum OffscreenEffect
		{
			BounceUp,
			BubbleBack,
			FakeDeath,
			InstantDeath
		}

		private PlayerHealthBar HealthBar;

		private DamageController Controller;

		public static ref bool IsEnabled => ref HealthData.isEnabled;

		private HealthSystemManager(bool resetHealth, bool isGlobal, int setHealthTo = 0, string activateFlag = null)
			: base(isGlobal)
		{
			if (activateFlag != null)
			{
				HealthData.activateFlag = activateFlag;
			}
			if (setHealthTo > 0)
			{
				HealthData.playerHealthVal = setHealthTo;
			}
			if (resetHealth)
			{
				ModSession.currentPlayerHealth = HealthData.playerHealthVal;
			}
			Add(new EntityFlagger(HealthData.activateFlag, _ => EnableHealthSystem()));
		}

		public HealthSystemManager(EntityData data, Vector2 _)
			: this(!HealthData.isCreated, data.Bool("isGlobal"), data.Int("playerHealth"), data.String("activationFlag"))
		{
			UpdateSessionData(data);
		}

		public HealthSystemManager() : this(!HealthData.globalHealth, HealthData.globalController) { }

		public void UpdateSessionData(EntityData data)
		{
			bool wasEnabled = IsEnabled;
			ChangeGlobalState(data.Bool("isGlobal"));
			HealthData = new()
			{
				iconSprite = data.String("healthIcons", HealthData.iconSprite),
				startAnim = data.String("healthIconsCreateAnim", HealthData.startAnim),
				endAnim = data.String("healthIconsRemoveAnim", HealthData.endAnim),
				healthBarPos = new(
					data.Float("healthIconsScreenX", HealthData.healthBarPos.X),
					data.Float("healthIconsScreenY", HealthData.healthBarPos.Y)
				),
				healthIconScale = new(
					data.Float("healthIconsScaleX", HealthData.healthIconScale.X),
					data.Float("healthIconsScaleY", HealthData.healthIconScale.Y)
				),
				iconSeparation = data.String("healthIconsSeparation", HealthData.iconSeparation),
				frameSprite = data.String("frameSprite", HealthData.frameSprite),
				globalController = IsGlobal,
				globalHealth = IsGlobal && data.Bool("globalHealth"),
				playerHealthVal = data.Int("playerHealth", HealthData.playerHealthVal),
				damageCooldown = data.Float("damageCooldown", HealthData.damageCooldown),
				playerOnCrush = data.Enum("crushEffect", HealthData.playerOnCrush),
				playerOffscreen = data.Enum("offscreenEffect", HealthData.playerOffscreen),
				fakeDeathMethods = data.String("fakeDeathMethods", HealthData.fakeDeathMethods),
				onDamageFunction = data.String("onDamageFunction", HealthData.onDamageFunction),
				activateInstantly = data.Bool("applySystemInstantly"),
				startVisible = data.Bool("startVisible"),
				removeOnDamage = data.Bool("removeOnDamage", true),
				playerBlink = data.Bool("playerBlink", true),
				playerStagger = data.Bool("playerStagger", true),
				activateFlag = data.String("activationFlag", HealthData.activateFlag),
				isCreated = true
			};
			if (wasEnabled)
			{
				if (HealthBar != null)
				{
					HealthBar.RemoveSelf();
					Scene.Add(HealthBar = new PlayerHealthBar());
				}
				Controller?.UpdateState(HealthBar);
			}
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (scene.GetEntity<HealthSystemManager>() != this)
			{
				RemoveSelf();
			}
			else if (IsEnabled || HealthData.activateInstantly)
			{
				EnableHealthSystem();
			}
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			if (scene.GetEntity<HealthSystemManager>() == this)
			{
				DisableHealthSystem();
				HealthData.isCreated = false;
			}
		}

		public void TakeDamage(Vector2 direction, int amount = 1, bool silent = false, bool stagger = true, bool evenIfInvincible = false)
		{
			if (!IsEnabled) return;
			int damageDealt = Controller.TakeDamage(direction, amount, silent, stagger, evenIfInvincible);
			HealthBar.DecreaseHealth(damageDealt);
		}

		public void RecoverHealth(int amount = 1)
		{
			if (!IsEnabled) return;
			Controller.RecoverHealth(amount);
			HealthBar.RefillHealth(amount);
		}

		public void RefillHealth()
		{
			if (!IsEnabled) return;
			Controller.RefillHealth();
			HealthBar.RefillHealth();
		}

		public void EnableHealthSystem()
		{
			IsEnabled = true;
			if (Scene.GetEntity<PlayerHealthBar>() == null)
				Scene.Add(HealthBar ??= new PlayerHealthBar());

			if (Scene.GetEntity<DamageController>() == null)
			{
				Scene.Add(Controller ??= new DamageController());
				Controller.Scene = Scene;
				Controller.UpdateState(HealthBar);
			}

			LoadFakeDeathHooks();
			Get<EntityFlagger>()?.RemoveSelf();
		}

		public void DisableHealthSystem()
		{
			IsEnabled = false;
			UnloadFakeDeathHooks();
			Controller?.RemoveSelf();
			HealthBar?.RemoveSelf();
		}

		public void SetEnableState(bool value)
		{
			if (value)
				EnableHealthSystem();
			else
				DisableHealthSystem();
		}

		public void PauseHealthSystem()
		{
			SetActiveState(false);
		}

		public void UnpauseHealthSystem()
		{
			SetActiveState(true);
		}

		public void SetActiveState(bool active)
		{
			IsEnabled = active;
			Active = active;
			if (Controller != null)
				Controller.Active = active;
			if (HealthBar != null)
				HealthBar.Active = active;
		}

		public static partial void LoadFakeDeathHooks();
		public static partial void UnloadFakeDeathHooks();
	}
}
