using Celeste.Mod.BossesHelper.Code.Components;
using Celeste.Mod.BossesHelper.Code.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using static Celeste.Mod.BossesHelper.Code.Entities.BossPuppet;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;
using static Celeste.Mod.BossesHelper.Code.Helpers.UserFileReader;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
	[CustomEntity("BossesHelper/BossController")]
	public partial class BossController : Entity
	{
		public Random Random { get; private set; }

		public readonly string BossID;

		public readonly BossPuppet Puppet;

		public int Health;

		private bool playerHasMoved;

		public readonly SingleUse<int> ForcedAttackIndex = new();

		private readonly bool startAttackingImmediately;

		private readonly Coroutine PatternCoroutine;

		private readonly List<Entity> activeEntities = [];

		private readonly List<BossPattern> AllPatterns = [];

		private readonly Dictionary<string, int> NamedPatterns = [];

		private Dictionary<string, IBossAction> BossActions;

		public int CurrentPatternIndex { get; private set; }

		private BossPattern CurrentPattern => AllPatterns[CurrentPatternIndex];

		public BossController(EntityData data, Vector2 offset, EntityID id)
			: base(data.Position + offset)
		{
			SourceData = data;
			SourceId = id;
			BossID = data.Attr("bossID");
			Add(PatternCoroutine = new Coroutine(false));
			Puppet = Create(data.Enum<HurtModes>("hurtMode"), data, offset);
			Puppet.Add(new BossHealthTracker(() => Health));
			if (BossesHelperModule.Session.BossPhasesSaved.TryGetValue(BossID, out BossesHelperSession.BossPhase phase))
			{
				Health = phase.BossHealthAt;
				CurrentPatternIndex = phase.StartWithPatternIndex;
				startAttackingImmediately = phase.StartImmediately;
			}
			else
			{
				Health = data.Int("bossHealthMax", -1);
				startAttackingImmediately = data.Bool("startAttackingImmediately");
			}
			Everest.Events.Player.OnDie += _ => CurrentPattern.EndAction(MethodEndReason.PlayerDied);
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Scene.Add(Puppet);
			int tasSeed = BossesHelperModule.Instance.TASSeed;
			int generalSeed = tasSeed > 0 ? tasSeed : (int)Math.Floor(Scene.TimeActive);
			Random = new Random(generalSeed * 37 + new Crc32().Get(SourceId.Key));
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			BossActions = this.ReadLuaFiles(
				(SourceData.Attr("attacksPath"), BossAttack.Create),
				(SourceData.Attr("eventsPath"), BossEvent.Create)
			);
			Puppet.BossFunctions = ReadLuaFilePath(SourceData.Attr("functionsPath"), path => new BossFunctions(path, this));
			AllPatterns.AddRange(this.ReadPatternFile(SourceData.Attr("patternsPath")));
			for (int i = 0; i < AllPatterns.Count; i++)
			{
				if (AllPatterns[i].Name is string name)
					NamedPatterns.Add(name, i);
			}
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			DestroyAll();
			Puppet.RemoveSelf();
		}

		public override void Update()
		{
			base.Update();
			if (Scene.GetPlayer() is Player entity)
			{
				bool IsActing = CurrentPattern.IsActing;
				if (!playerHasMoved && (entity.Speed != Vector2.Zero || startAttackingImmediately || IsActing))
				{
					playerHasMoved = true;
					if (!IsActing)
						StartAttackPattern(CurrentPatternIndex);
				}
				if (!IsActing && IsPlayerWithinSpecifiedRegion(entity.Position))
				{
					InterruptPattern();
					ChangeToPattern();
				}
			}
		}

		private bool IsPlayerWithinSpecifiedRegion(Vector2 entityPos)
		{
			return CurrentPattern is AttackPattern attack
				&& attack.PlayerPositionTrigger is Collider positionTrigger
				&& positionTrigger.Collide(entityPos);
		}

		public void DestroyAll()
		{
			activeEntities.ForEach(entity => entity.RemoveSelf());
			activeEntities.Clear();
		}

		public void InterruptPattern()
		{
			PatternCoroutine.Active = false;
			CurrentPattern.EndAction(MethodEndReason.Interrupted);
		}

		public void StartAttackPattern(int goTo = -1)
		{
			if (goTo >= AllPatterns.Count)
			{
				CurrentPatternIndex = -1;
				PatternCoroutine.Active = false;
				return;
			}
			if (goTo > 0)
			{
				CurrentPatternIndex = goTo;
			}
			PatternCoroutine.Replace(CurrentPattern.Perform());
		}

		internal bool TryGet(string key, out IBossAction action)
		{
			return BossActions.TryGetValue(key, out action);
		}

		internal void ChangeToPattern()
		{
			StartAttackPattern(CurrentPattern.GoToPattern.TryParse(out int index) ? index :
				NamedPatterns.GetValueOrDefault(CurrentPattern.GoToPattern ?? "", CurrentPatternIndex + 1));
		}
	}
}
