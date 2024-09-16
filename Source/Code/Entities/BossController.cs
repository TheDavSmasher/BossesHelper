﻿using Monocle;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.BossesHelper.Code.Other;
using Microsoft.Xna.Framework;
using System.Collections;
using System;
using Celeste.Mod.BossesHelper.Code.Helpers;
using NLua;

namespace Celeste.Mod.BossesHelper.Code.Entities
{
    [CustomEntity("BossesHelper/BossController")]
    public class BossController : Entity
    {
        public struct EntityTimer
        {
            public Entity target;

            public float Timer { get; set; }

            public string id;

            public LuaFunction action;

            public readonly bool Done
            {
                get
                {
                    return Timer <= 0;
                }
            }

            private EntityTimer(Entity target, float timer, LuaFunction action, string id)
            {
                this.target = target;
                this.Timer = timer;
                this.action = action;
                this.id = id;
            }

            public EntityTimer UpdateTimer()
            {
                Timer -= Engine.DeltaTime;
                return this;
            }

            public void ExecuteEarly()
            {
                Timer = 0;
            }

            public static EntityTimer DoActionOnEntityDelay(LuaFunction action, Entity entity, string id, float timer)
            {
                return new EntityTimer(entity, timer, action, id);
            }
        }

        public struct EntityFlagger
        {
            public Entity target;

            public string flag;

            public bool stateNeeded;

            public bool resetFlag;

            public LuaFunction action;
            
            public readonly bool Ready
            {
                get
                {
                    return target.SceneAs<Level>().Session.GetFlag(flag) == stateNeeded;
                }
            }

            private EntityFlagger(Entity target, string flag, bool stateNeeded, LuaFunction action, bool resetFlag)
            {
                this.target = target;
                this.flag = flag;
                this.stateNeeded = stateNeeded;
                this.resetFlag = resetFlag;
                this.action = action;
            }

            public static EntityFlagger DoActionOnEntityOnFlagState(LuaFunction action, Entity entity, string flag,
                bool state = true, bool resetFlag = true)
            {
                return new EntityFlagger(entity, flag, state, action, resetFlag);
            }
        }

        public struct AttackDelegates(Action<Entity> addEntity, Action<Entity, string, LuaFunction, float> addEntityWithTimer, Action<Entity, string, LuaFunction, bool, bool> addEntityWithFlagger, Action<Entity> destroyEntity, Action destroyAll)
        {
            public Action<Entity> addEntity = addEntity;

            public Action<Entity, string, LuaFunction, float> addEntityWithTimer = addEntityWithTimer;

            public Action<Entity, string, LuaFunction, bool, bool> addEntityWithFlagger = addEntityWithFlagger;

            public Action<Entity> destroyEntity = destroyEntity;

            public Action destroyAll = destroyAll;
        }

        public struct BossPhase
        {
            public int phaseID;

            public int? bossHealthAt;
        }

        public readonly string Name;

        public Level Level;

        public enum MoveModes
        {
            Nodes,
            ScreenEdge,
            PlayerScreenEdge,
            PlayerPos,
            Static,
            Freeroam
        }

        public enum HurtModes
        {
            PlayerContact,
            PlayerDash,
            Explosion,
            HeadBonk,
            SidekickAttack
        }

        public readonly BossPuppet Puppet;

        public Dictionary<string, BossAttack> AllAttacks;

        public Dictionary<string, BossEvent> AllEvents;

        private readonly UserFileReader userFileReader;

        public int Health { get; private set; }

        private float bossHitCooldown;

        public List<BossPattern> Patterns;

        private readonly int nodeCount;

        private int currentPatternIndex;

        private readonly Coroutine currentPattern;

        public List<int> patternOrder;

        private int currentNodeOrIndex;

        private int currentPhase;

        private bool playerHasMoved;

        private bool isAttacking;

        private readonly bool startAttackingImmediately;

        private readonly List<Entity> activeEntities;

        private readonly Dictionary<string, EntityTimer> activeEntityTimers;

        private readonly Dictionary<string, EntityFlagger> activeEntityFlaggers;

        public BossController(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Name = data.Attr("bossName");
            Health = data.Int("bossHealthMax", -1);
            startAttackingImmediately = data.Bool("startAttackingImmediately");
            bossHitCooldown = data.Float("bossHitCooldown", 0.5f);
            nodeCount = data.Nodes.Length;
            currentPhase = 1;
            currentNodeOrIndex = 0;
            isAttacking = false;
            userFileReader = new UserFileReader(Name);
            AllAttacks = new Dictionary<string, BossAttack>();
            AllEvents = new Dictionary<string, BossEvent>();
            patternOrder = new List<int>();
            PopulatePatternsAndOrder();
            currentPatternIndex = patternOrder[currentNodeOrIndex];
            currentPattern = new Coroutine();
            Add(currentPattern);
            Puppet = new BossPuppet(data, offset);
            activeEntities = new List<Entity>();
            activeEntityTimers = new Dictionary<string, EntityTimer>();
            activeEntityFlaggers = new Dictionary<string, EntityFlagger>();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level = SceneAs<Level>();
            Level.Add(Puppet);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player player = scene.Tracker.GetEntity<Player>();
            PopulateAttacksAndEvents(player);
            if (scene.Tracker.GetEntity<BadelineSidekick>() == null)
            {
                (scene as Level).Add(new BadelineSidekick(player.Position));
            }
        }

        public override void Update()
        {
            base.Update();
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (!playerHasMoved && (entity != null && entity.Speed != Vector2.Zero || startAttackingImmediately))
            {
                playerHasMoved = true;
                StartAttackPattern();
            }
            foreach (KeyValuePair<string, EntityTimer> entityTimer in activeEntityTimers)
            {
                if (entityTimer.Value.Done)
                {
                    entityTimer.Value.action.Call(entityTimer.Value.target);
                    activeEntityTimers.Remove(entityTimer.Key);
                }
                else
                {
                    activeEntityTimers[entityTimer.Key] = entityTimer.Value.UpdateTimer();
                }
            }
            foreach (EntityFlagger entityFlagger in activeEntityFlaggers.Values)
            {
                if (entityFlagger.Ready)
                {
                    entityFlagger.action.Call(entityFlagger.target);
                    if (entityFlagger.resetFlag)
                    {
                        Level.Session.SetFlag(entityFlagger.flag, !entityFlagger.stateNeeded);
                    }
                    activeEntityFlaggers.Remove(entityFlagger.flag);
                }
            }
            if (!isAttacking && IsPlayerWithinSpecifiedRegion(entity.Position))
            {
                InterruptPattern();
                currentPatternIndex = (int)Patterns[currentPatternIndex].GoToPattern - 1;
                StartAttackPattern();
            }
            //TODO Currently used to test node changing
            if (Level.Session.GetFlag("wa_interrupt"))
            {
                Level.Session.SetFlag("wa_interrupt", false);
                InterruptPattern();
            }
            if (Level.Session.GetFlag("wa_advance"))
            {
                Level.Session.SetFlag("wa_advance", false);
                AdvanceNode();
                StartAttackPattern();
            }
        }

        private bool IsPlayerWithinSpecifiedRegion(Vector2 entityPos)
        {
            return Patterns[currentPatternIndex].finishMode == BossPattern.FinishMode.PlayerPositionWithin
                && Patterns[currentPatternIndex].PlayerPositionTrigger.Contains((int)entityPos.X, (int)entityPos.Y);
        }

        private void InterruptPattern()
        {
            currentPattern.Active = false;
            Patterns[currentPatternIndex].CurrentAction = 0;
            //activeEntityTimers.ForEach(timer => timer.ExecuteEarly());
            DestroyAll();
        }

        private void AdvanceNode()
        {
            currentNodeOrIndex++;
            currentPatternIndex = patternOrder[currentNodeOrIndex];
        }

        private void StartAttackPattern()
        {
            currentPattern.Replace(PerformPattern(Patterns[currentPatternIndex]));
        }

        private void PopulateAttacksAndEvents(Player player)
        {
            AttackDelegates delegates = new(AddEntity, AddEntityWithTimer, AddEntityWithFlagger, DestroyEntity, DestroyAll);
            userFileReader.ReadAttackFilesInto(ref AllAttacks, player, Puppet, delegates);
            userFileReader.ReadEventFilesInto(ref AllEvents, player, Puppet);
        }

        private void PopulatePatternsAndOrder()
        {
            userFileReader.ReadPatternFilesInto(ref Patterns);
            userFileReader.ReadPatternOrderFileInto(ref patternOrder, nodeCount);
        }

        private IEnumerator PerformPattern(BossPattern pattern)
        {
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
                if (pattern.CurrentAction >= pattern.StatePatternOrder.Length)
                {
                    if (pattern.finishMode == BossPattern.FinishMode.LoopCountGoTo)
                    {
                        loop++;
                        if (loop > pattern.IterationCount)
                        {
                            currentPatternIndex = (int) pattern.GoToPattern - 1;
                            StartAttackPattern();
                        }
                    }
                    pattern.CurrentAction = 0;
                }

                yield return PerformMethod(pattern.StatePatternOrder[pattern.CurrentAction]);

                pattern.CurrentAction++;
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
                else if (AllEvents.TryGetValue(method.ActionName, out BossEvent cutscene))
                {
                    Level.Add(cutscene);
                }
            }
            yield return method.Duration;
        }

        public void AddEntity(Entity entity)
        {
            activeEntities.Add(entity);
            entity.Scene = Level;
            Level.Add(entity);
        }

        public void AddEntityWithTimer(Entity entity, string id, LuaFunction action, float timer)
        {
            if (!activeEntityTimers.ContainsKey(id))
            {
                activeEntities.Add(entity);
                entity.Scene = Level;
                activeEntityTimers.Add(id, EntityTimer.DoActionOnEntityDelay(action, entity, id, timer));
                Level.Add(entity);
            }
        }

        public void AddEntityWithFlagger(Entity entity, string flag, LuaFunction action, bool state = true, bool resetFlag = true)
        {
            if (!activeEntityFlaggers.ContainsKey(flag))
            {
                activeEntities.Add(entity);
                entity.Scene = Level;
                activeEntityFlaggers.Add(flag, EntityFlagger.DoActionOnEntityOnFlagState(action, entity, flag, state, resetFlag));
                Level.Add(entity);
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
            activeEntityTimers.Clear();
            activeEntityFlaggers.Clear();
            activeEntities.ForEach(entity => entity.RemoveSelf());
            activeEntities.Clear();
        }
    }
}
