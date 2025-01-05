﻿using Monocle;
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
        private Random random;

        public struct AttackDelegates(Player playerRef, BossPuppet puppetRef,
            Action<Entity> addEntity, Action<Entity> destroyEntity, Action destroyAll)
        {
            public Player playerRef = playerRef;

            public BossPuppet puppetRef = puppetRef;

            public Action<Entity> addEntity = addEntity;

            public Action<Entity> destroyEntity = destroyEntity;

            public Action destroyAll = destroyAll;
        }

        public struct OnHitDelegates(Player playerRef, BossPuppet puppetRef, Func<int> getHealth, Action<int> setHealth,
            Action<int> decreaseHealth, Func<IEnumerator> waitForAttack, Action interruptPattern, Func<int> currentPattern,
            Action<int> startAttackPattern, Action<int, int, bool> savePhaseChangeToSession, Action<bool> removeBoss)
        {
            public Player playerRef = playerRef;

            public BossPuppet puppetRef = puppetRef;

            public Func<int> getHealth = getHealth;

            public Action<int> setHealth = setHealth;

            public Action<int> decreaseHealth = decreaseHealth;

            public Func<IEnumerator> waitForAttack = waitForAttack;

            public Action interruptPattern = interruptPattern;

            public Func<int> currentPattern = currentPattern;

            public Action<int> startAttackPattern = startAttackPattern;

            public Action<int, int, bool> savePhaseChangeToSession = savePhaseChangeToSession;

            public Action<bool> removeBoss = removeBoss;
        }

        public struct CustceneDelegates(Action<bool> removeBoss)
        {
            public Action<bool> removeBoss = removeBoss;
        }

        private EntityID id;

        private readonly string BossID;

        public Level Level;

        public readonly BossPuppet Puppet;

        public Dictionary<string, BossAttack> AllAttacks;

        public Dictionary<string, BossEvent> AllEvents;

        private int Health;

        public List<BossPattern> Patterns;

        private int currentPatternIndex;

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
            AllAttacks = new Dictionary<string, BossAttack>();
            AllEvents = new Dictionary<string, BossEvent>();
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
            PopulatePatterns((scene as Level).LevelOffset);
            int individualSeed = (new Crc32()).Get(id.Key);
            if (BossesHelperModule.Instance.TASSeed > 0)
            {
                random = new Random(BossesHelperModule.Instance.TASSeed * 37 + individualSeed);
            }
            else
            {
                random = new Random((int)Math.Floor(Level.TimeActive) * 37 + individualSeed);
            }
        }

        private void PopulatePatterns(Vector2 levelOffset)
        {
            Patterns = new();
            UserFileReader.ReadPatternFileInto(patternsPath, ref Patterns, levelOffset);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            //Logger.Log(LogLevel.Info, "Bosses Helper", "Controller Awake");
            //Logger.Log(LogLevel.Info, "Stack Trace", new System.Diagnostics.StackTrace().ToString());
            Player player = scene.Tracker.GetEntity<Player>();
            PopulateAttacksEventsAndFunctions(player);
        }

        private void PopulateAttacksEventsAndFunctions(Player player)
        {
            UserFileReader.ReadAttackFilesInto(attacksPath, ref AllAttacks, BossID,
                new(player, Puppet, AddEntity, DestroyEntity, DestroyAll));
            UserFileReader.ReadEventFilesInto(eventsPath, ref AllEvents, BossID, player, Puppet,
                new(RemoveBoss));
            UserFileReader.ReadCustomCodeFileInto(functionsPath, out BossFunctions bossReactions, BossID,
                new(player, Puppet, () => Health, (val) => Health = val, DecreaseHealth, WaitForAttackToEnd,
                InterruptPattern, () => currentPatternIndex, StartAttackPattern, SavePhaseChangeInSession, RemoveBoss));
            Puppet.SetPuppetFunctions(bossReactions);
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
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null)
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
            Hitbox positionTrigger = Patterns[currentPatternIndex].PlayerPositionTrigger;
            return positionTrigger != null && positionTrigger.Collide(entityPos);
        }

        private void StartAttackPattern(int goTo = -1)
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
                    int nextAttack = random.Next() % pattern.StatePatternOrder.Length;
                    yield return PerformMethod(pattern.StatePatternOrder[nextAttack]);
                    currentAction++;

                    if (pattern.IterationCount == null) continue;
                    if (currentAction > pattern.MinRandomIter && (currentAction > pattern.IterationCount || random.Next() % 2 == 1))
                    {
                        if (pattern.GoToPattern is int next)
                            StartAttackPattern(next);
                        else
                            StartNextAttackPattern();
                        yield return null;
                    }
                }
            }
            //Deterministic Pattern
            if (pattern.PrePatternMethods != null)
            {
                foreach (BossPattern.Method method in pattern.PrePatternMethods)
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
                if (loop >= pattern.MinRandomIter && (loop >= pattern.IterationCount || random.Next() % 2 == 1))
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

        private IEnumerator PerformMethod(BossPattern.Method method)
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
        private IEnumerator WaitForAttackToEnd()
        {
            while (isAttacking)
            {
                yield return null;
            }
        }

        private void InterruptPattern()
        {
            currentPattern.Active = false;
            DestroyAll();
        }

        private void SavePhaseChangeInSession(int health, int patternIndex, bool startImmediately)
        {
            BossesHelperModule.Session.BossPhaseSaved = new(health, startImmediately, patternIndex);
        }

        private void RemoveBoss(bool permanent)
        {
            RemoveSelf();
            if (permanent)
            {
                Level.Session.DoNotLoad.Add(id);
            }
        }

        private void DecreaseHealth(int val = 1)
        {
            Health -= val;
        }
        #endregion

        #region Attack Delegates
        private void AddEntity(Entity entity)
        {
            if (!activeEntities.Contains(entity))
            {
                Level.Add(entity);
                activeEntities.Add(entity);
                entity.Scene = Level;
            }
        }

        private void DestroyEntity(Entity entity)
        {
            if (activeEntities.Remove(entity))
            {
                entity.RemoveSelf();
            }
        }

        private void DestroyAll()
        {
            activeEntities.ForEach(entity => entity.RemoveSelf());
            activeEntities.Clear();
        }
        #endregion
        #endregion
    }
}
