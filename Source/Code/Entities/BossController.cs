using Monocle;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.BossesHelper.Code.Other;
using Microsoft.Xna.Framework;
using System.Collections;
using System;
using Celeste.Mod.BossesHelper.Code.Helpers;
using static Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [CustomEntity("BossesHelper/BossController")]
    public class BossController : Entity
    {
        public Random Random { get; private set; }

        private EntityID id;

        public string BossID { get; private set; }

        private Level Level;

        public BossPuppet Puppet { get; private set; }

        private Dictionary<string, BossAttack> AllAttacks;

        private Dictionary<string, BossEvent> AllEvents;

        private int Health;

        private List<BossPattern> Patterns;

        private int currentPatternIndex;

        private int? forcedAttackIndex;

        private readonly Coroutine currentPattern;

        private bool playerHasMoved;

        private bool isAttacking;

        private bool startAttackingImmediately;

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
            isAttacking = false;
            currentPatternIndex = 0;
            currentPattern = new Coroutine();
            Add(currentPattern);
            Puppet = new BossPuppet(data, offset, () => Health);
            activeEntities = new List<Entity>();
            FetchSavedPhase();
        }

        private void FetchSavedPhase()
        {
            BossesHelperSession.BossPhase phase = BossesHelperModule.Session.BossPhaseSaved;
            if (phase.bossHealthAt != 0)
            {
                Health = phase.bossHealthAt;
                currentPatternIndex = phase.startWithPatternIndex;
                startAttackingImmediately = phase.startImmediately;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level = SceneAs<Level>();
            Level.Add(Puppet);
            UserFileReader.ReadPatternFileInto(patternsPath, out Patterns, Level.LevelOffset);
            int tasSeed = BossesHelperModule.Instance.TASSeed;
            int generalSeed = tasSeed > 0 ? tasSeed : (int)Math.Floor(Level.TimeActive);
            Random = new Random(generalSeed * 37 + new Crc32().Get(id.Key));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player player = scene.GetPlayer();
            UserFileReader.ReadAttackFilesInto(attacksPath, out AllAttacks, player, this);
            UserFileReader.ReadEventFilesInto(eventsPath, out AllEvents, player, this);
            UserFileReader.ReadCustomCodeFileInto(functionsPath, player, this);
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
                if (!playerHasMoved && (entity.Speed != Vector2.Zero || startAttackingImmediately))
                {
                    playerHasMoved = true;
                    StartAttackPattern(currentPatternIndex);
                }
                if (!playerHasMoved && isAttacking)
                {
                    playerHasMoved = true;
                }
                if (!isAttacking && IsPlayerWithinSpecifiedRegion(entity.Position))
                {
                    InterruptPattern();
                    if (Patterns[currentPatternIndex].GoToPattern is int next)
                        StartAttackPattern(next);
                    else
                        StartNextAttackPattern();
                }
            }
        }

        private bool IsPlayerWithinSpecifiedRegion(Vector2 entityPos)
        {
            return Patterns[currentPatternIndex].PlayerPositionTrigger is Hitbox positionTrigger 
                && positionTrigger.Collide(entityPos);
        }

        public void StartAttackPattern(int goTo = -1)
        {
            if (goTo >= Patterns.Count)
            {
                currentPatternIndex = -1;
                currentPattern.Active = false;
                return;
            }
            if (goTo > 0)
            {
                currentPatternIndex = goTo;
            }
            currentPattern.Replace(PerformPattern(Patterns[currentPatternIndex]));
        }

        private void StartNextAttackPattern()
        {
            StartAttackPattern(currentPatternIndex + 1);
        }

        private IEnumerator PerformPattern(BossPattern pattern)
        {
            //Boss Event
            if (pattern.IsEvent)
            {
                if (AllEvents.TryGetValue(pattern.FirstAction, out BossEvent cutscene))
                {
                    Level.Add(cutscene);
                    do
                    {
                        yield return null;
                    } 
                    while (cutscene.Running) ;
                }
                else
                {
                    Logger.Log(LogLevel.Error, "Bosses Helper", "Could not find specified event file.");
                }
                if (pattern.GoToPattern is int next)
                    StartAttackPattern(next);
                else
                    StartNextAttackPattern();
                yield return null;
            }
            int currentAction = 0;
            //Random Pattern
            if (pattern.RandomPattern)
            {
                while (true)
                {
                    int nextAttack = (forcedAttackIndex ?? Random.Next()) % pattern.StatePatternOrder.Length;
                    forcedAttackIndex = null;
                    yield return PerformMethod(pattern.StatePatternOrder[nextAttack]);
                    currentAction++;

                    if (pattern.IterationCount == null) continue;
                    if (currentAction > pattern.MinRandomIter && (currentAction > pattern.IterationCount || Random.Next() % 2 == 1))
                    {
                        if (pattern.GoToPattern is int next)
                            StartAttackPattern(next);
                        else
                            StartNextAttackPattern();
                        yield return null;
                    }
                }
            }
            //Ordered Pattern
            if (pattern.PrePatternMethods != null)
            {
                foreach (Method method in pattern.PrePatternMethods)
                {
                    yield return PerformMethod(method);
                }
            }
            int loop = 0;
            while (true)
            {
                if (currentAction >= pattern.StatePatternOrder.Length)
                {
                    loop++;
                    currentAction = 0;
                }
                if (loop > pattern.MinRandomIter && (loop > pattern.IterationCount || Random.Next() % 2 == 1))
                {
                    if (pattern.GoToPattern is int next)
                        StartAttackPattern(next);
                    else
                        StartNextAttackPattern();
                    yield return null;
                }

                yield return PerformMethod(pattern.StatePatternOrder[currentAction]);

                currentAction++;
            }
        }

        private IEnumerator PerformMethod(Method method)
        {
            if (!method.ActionName.ToLower().Equals("wait"))
            {
                if (AllAttacks.TryGetValue(method.ActionName, out BossAttack attack))
                {
                    isAttacking = true;
                    yield return attack.Coroutine();
                    isAttacking = false;
                }
                else
                {
                    Logger.Log(LogLevel.Error, "Bosses Helper", "Could not find specified attack file.");
                }
            }
            yield return method.Duration;
        }

        #region Delegate methods
        #region Interruption Delegates
        public IEnumerator WaitForAttackToEnd()
        {
            while (isAttacking)
            {
                yield return null;
            }
        }

        public void InterruptPattern()
        {
            currentPattern.Active = false;
            DestroyAll();
        }

        public void SavePhaseChangeInSession(int health, int patternIndex, bool startImmediately)
        {
            BossesHelperModule.Session.BossPhaseSaved = new(health, startImmediately, patternIndex);
        }

        public void RemoveBoss(bool permanent)
        {
            RemoveSelf();
            if (permanent)
            {
                Level.Session.DoNotLoad.Add(id);
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
        #endregion

        #region Attack Delegates
        public void ForceNextAttackIndex(int index)
        {
            forcedAttackIndex = index;
        }

        public void AddEntity(Entity entity)
        {
            if (!activeEntities.Contains(entity))
            {
                Level.Add(entity);
                activeEntities.Add(entity);
                entity.Scene = Level;
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
        #endregion
    }
}
