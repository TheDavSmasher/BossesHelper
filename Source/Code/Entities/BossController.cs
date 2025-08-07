using Celeste.Mod.BossesHelper.Code.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
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

        private int Health;

        private bool playerHasMoved;

        private bool isActing;

        private int currentPatternIndex;

        private int? forcedAttackIndex;

        private readonly bool startAttackingImmediately;

        private readonly Coroutine ActivePattern;

        private readonly List<Entity> activeEntities = [];

        private List<BossPattern> AllPatterns;

        private BossPattern CurrentPattern => AllPatterns[currentPatternIndex];

        public BossController(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset)
        {
            BossID = data.Attr("bossID");
            Health = data.Int("bossHealthMax", -1);
            startAttackingImmediately = data.Bool("startAttackingImmediately");
            Add(ActivePattern = new Coroutine());
            Puppet = new BossPuppet(data, offset, () => Health);
            if (BossesHelperModule.Session.BossPhasesSaved.TryGetValue(BossID, out BossesHelperSession.BossPhase phase))
            {
                Health = phase.BossHealthAt;
                currentPatternIndex = phase.StartWithPatternIndex;
                startAttackingImmediately = phase.StartImmediately;
            }
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
            AllPatterns = ReadPatternFile(SourceData.Attr("patternsPath"), SceneAs<Level>().LevelOffset,
                this.ReadLuaFiles(SourceData.Attr("attacksPath"), SourceData.Attr("eventsPath")),
                new(ChangeToPattern, Random.Next, val => isActing = val, AttackIndexForced));
            this.ReadBossFunctions(SourceData.Attr("functionsPath"));
            CheckForPlayer().Coroutine(this);
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
                if (!playerHasMoved && (entity.Speed != Vector2.Zero || startAttackingImmediately || isActing))
                {
                    playerHasMoved = true;
                    if (!isActing)
                        StartAttackPattern(currentPatternIndex);
                }
                if (!isActing && IsPlayerWithinSpecifiedRegion(entity.Position))
                {
                    InterruptPattern();
                    ChangeToPattern();
                }
            }
        }

        private bool IsPlayerWithinSpecifiedRegion(Vector2 entityPos)
        {
            return CurrentPattern is AttackPattern attack
                && attack.PlayerPositionTrigger is Hitbox positionTrigger 
                && positionTrigger.Collide(entityPos);
        }

        public void StartAttackPattern(int goTo = -1)
        {
            if (goTo >= AllPatterns.Count)
            {
                currentPatternIndex = -1;
                ActivePattern.Active = false;
                return;
            }
            if (goTo > 0)
            {
                currentPatternIndex = goTo;
            }
            ActivePattern.Replace(CurrentPattern.Perform());
        }

        private int? AttackIndexForced()
        {
            if (forcedAttackIndex is not int index)
                return null;
            forcedAttackIndex = null;
            return index;
        }

        private void ChangeToPattern()
        {
            StartAttackPattern(CurrentPattern.GoToPattern ?? currentPatternIndex + 1);
        }

        private IEnumerator CheckForPlayer()
        {
            while (Scene.GetPlayer() is not null)
            {
                yield return null;
            }
            CurrentPattern.EndAction(MethodEndReason.PlayerDied);
        }

        public void InterruptPattern()
        {
            ActivePattern.Active = false;
            isActing = false;
            CurrentPattern.EndAction(MethodEndReason.Interrupted);
        }

        public void DestroyAll()
        {
            activeEntities.ForEach(entity => entity.RemoveSelf());
            activeEntities.Clear();
        }
    }
}
