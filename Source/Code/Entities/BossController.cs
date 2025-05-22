using Celeste.Mod.BossesHelper.Code.Helpers;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;
using static Celeste.Mod.BossesHelper.Code.Helpers.UserFileReader;
using static Celeste.Mod.BossesHelper.Code.Other.BossPatterns;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [CustomEntity("BossesHelper/BossController")]
    public class BossController : Entity
    {
        public Random Random { get; private set; }

        public string BossID { get; private set; }

        public BossPuppet Puppet { get; private set; }

        private EntityID id;

        private int Health;

        private bool playerHasMoved;

        private bool isActing;

        private int currentPatternIndex;

        private int? forcedAttackIndex;

        private readonly bool startAttackingImmediately;

        private readonly Coroutine ActivePattern;

        private List<BossPattern> AllPatterns;

        private BossPattern CurrentPattern => AllPatterns[currentPatternIndex];

        private readonly List<Entity> activeEntities;

        private readonly string attacksPath;

        private readonly string eventsPath;

        private readonly string functionsPath;

        private readonly string patternsPath;

        public BossController(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset)
        {
            this.id = id;
            BossID = data.Attr("bossID");
            Health = data.Int("bossHealthMax", -1);
            startAttackingImmediately = data.Bool("startAttackingImmediately");
            attacksPath = data.Attr("attacksPath");
            eventsPath = data.Attr("eventsPath");
            functionsPath = data.Attr("functionsPath");
            patternsPath = data.Attr("patternsPath");
            isActing = false;
            currentPatternIndex = 0;
            ActivePattern = new Coroutine();
            Add(ActivePattern);
            Puppet = new BossPuppet(data, offset, () => Health);
            activeEntities = new List<Entity>();
            if (BossesHelperModule.Session.BossPhasesSaved.TryGetValue(BossID, out BossesHelperSession.BossPhase phase))
            {
                Health = phase.bossHealthAt;
                currentPatternIndex = phase.startWithPatternIndex;
                startAttackingImmediately = phase.startImmediately;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Scene.Add(Puppet);
            int tasSeed = BossesHelperModule.Instance.TASSeed;
            int generalSeed = tasSeed > 0 ? tasSeed : (int)Math.Floor(Scene.TimeActive);
            Random = new Random(generalSeed * 37 + new Crc32().Get(id.Key));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            AllPatterns = ReadPatternFile(patternsPath, SceneAs<Level>().LevelOffset,
                ReadLuaFiles(this, attacksPath, eventsPath, functionsPath),
                new(ChangeToPattern, Random.Next, val => isActing = val, AttackIndexForced));
            Add(new Coroutine(CheckForPlayer()));
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
                    ChangeToPattern(CurrentPattern.GoToPattern);
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

        private void ChangeToPattern(int? goTo)
        {
            StartAttackPattern(goTo ?? currentPatternIndex + 1);
        }

        private IEnumerator CheckForPlayer()
        {
            while (Scene.GetPlayer() is not null)
            {
                yield return null;
            }
            CurrentPattern.EndAction(MethodEndReason.PlayerDied);
        }

        #region Lua Helper methods
        public IEnumerator WaitForAttackToEnd()
        {
            while (isActing)
            {
                yield return null;
            }
        }

        public void InterruptPattern()
        {
            ActivePattern.Active = false;
            isActing = false;
            CurrentPattern.EndAction(MethodEndReason.Interrupted);
        }

        public void SavePhaseChangeInSession(int health, int patternIndex, bool startImmediately)
        {
            BossesHelperModule.Session.BossPhasesSaved.Add(BossID,
                new BossesHelperSession.BossPhase(health, startImmediately, patternIndex));
        }

        public void RemoveBoss(bool permanent)
        {
            RemoveSelf();
            if (permanent)
            {
                SceneAs<Level>().Session.DoNotLoad.Add(id);
            }
        }

        public int GetCurrentPatternIndex()
        {
            return currentPatternIndex;
        }

        public int GetHealth()
        {
            return Health;
        }

        public void SetHealth(int val)
        {
            Health = val;
        }

        public void DecreaseHealth(int val = 1)
        {
            Health -= val;
        }

        public void ForceNextAttackIndex(int index)
        {
            forcedAttackIndex = index;
        }

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

        public void DestroyAll()
        {
            activeEntities.ForEach(entity => entity.RemoveSelf());
            activeEntities.Clear();
        }
        #endregion
    }
}
