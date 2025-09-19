using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.BossesHelper
{
	public partial class BossesHelperModule
	{
		public static int PlayerHealth => HealthData.isEnabled ? Session.currentPlayerHealth : -1;
	}

	namespace Code.Entities
	{
		public partial class BossController
		{
			public string CurrentPatternName => CurrentPattern.Name;

			public bool IsActing => CurrentPattern.IsActing;

			public void AddEntity(Entity entity)
			{
				if (!activeEntities.Contains(entity))
				{
					Scene.Add(entity);
					activeEntities.Add(entity);
					entity.Scene = Scene;
				}
			}

			public void DestroyEntity(Entity entity)
			{
				if (activeEntities.Remove(entity))
				{
					entity.RemoveSelf();
				}
			}

			public int GetPatternIndex(string goTo)
			{
				return NamedPatterns.GetValueOrDefault(goTo, -1);
			}

			public void ForceNextAttack(int index)
			{
				if (CurrentPattern is RandomPattern Random)
					Random.ForceNextAttack(index);
			}

			public void SavePhaseChangeInSession(int health, int patternIndex, bool startImmediately)
			{
				BossesHelperModule.Session.BossPhasesSaved[BossID] =
					new(health, startImmediately, patternIndex);
			}

			public void RemoveBoss(bool permanent)
			{
				RemoveSelf();
				if (permanent)
				{
					Scene.DoNotLoad(SourceId);
				}
			}

			private readonly Dictionary<string, object> storedObjects = [];

			public void StoreObject(string key, object toStore)
			{
				storedObjects.TryAdd(key, toStore);
			}

			public object GetStoredObject(string key)
			{
				return storedObjects.TryGetValue(key, out object storedObject) ? storedObject : null;
			}

			public void DeleteStoredObject(string key)
			{
				storedObjects.Remove(key);
			}

			public int GetHealth() => Health;

			public void DecreaseHealth(int val = 1)
			{
				Health -= val;
			}
		}

		public partial class BossPuppet
		{
			public float BossHitCooldown => BossDamageCooldown.TimeLeft;

			public void Set1DSpeedDuring(float speed, bool isX, float time)
			{
				Keep1DSpeed(speed, isX, time).Coroutine(this);
			}

			public IEnumerator Keep1DSpeed(float speed, bool isX, float time)
			{
				while (time > 0)
				{
					(isX ? ref Speed.X : ref Speed.Y) = speed;
					time -= Engine.DeltaTime;
					yield return null;
				}
			}

			public void Speed1DTween(float start, float target, float time, bool isX, Ease.Easer easer = null)
			{
				Tween tween = Tween.Create(Tween.TweenMode.Oneshot, easer, time, true);
				tween.OnUpdate = t => (isX ? ref Speed.X : ref Speed.Y) = start + (target - start) * t.Eased;
				Add(tween);
			}

			public void ChangeHitboxOption(string tag)
			{
				Collider = GetTagOrDefault(ColliderOption.Hitboxes, tag, Sprite.Height);
			}

			public void ChangeHurtboxOption(string tag)
			{
				Hurtbox = GetTagOrDefault(ColliderOption.Hurtboxes, tag, Sprite.Height);
				if (BossCollision is PlayerCollider collider)
				{
					collider.Collider = Hurtbox;
				}
			}
		}

		public partial class BounceBossPuppet
		{
			public void ChangeBounceboxOption(string tag)
			{
				Bouncebox = GetTagOrDefault(ColliderOption.Bouncebox, tag, 6f);
				if (BossCollision is PlayerCollider collider)
				{
					collider.Collider = Bouncebox;
				}
			}
		}

		public partial class SidekickBossPuppet
		{
			public void ChangeTargetOption(string tag)
			{
				Target = GetTagOrDefault(ColliderOption.Target, tag, null);
				if (BossCollision is SidekickTarget target)
				{
					target.Collider = Target;
				}
			}
		}
	}
}
