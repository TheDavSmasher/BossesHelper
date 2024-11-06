using Monocle;
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
        private static readonly Random random = new();

        public struct EntityTimer
        {
            public Entity target;

            public float Timer { get; set; }

            public LuaFunction action;

            public readonly bool Done
            {
                get
                {
                    return Timer <= 0;
                }
            }

            private EntityTimer(Entity target, float timer, LuaFunction action)
            {
                this.target = target;
                this.Timer = timer;
                this.action = action;
            }

            public void Execute()
            {
                action.Call(target);
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

            public static EntityTimer DoActionOnEntityDelay(LuaFunction action, Entity entity, float timer)
            {
                return new EntityTimer(entity, timer, action);
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

            public void Execute()
            {
                action.Call(target);
            }

            public static EntityFlagger DoActionOnEntityOnFlagState(LuaFunction action, Entity entity, string flag,
                bool state = true, bool resetFlag = true)
            {
                return new EntityFlagger(entity, flag, state, action, resetFlag);
            }
        }

        public struct AttackDelegates(Player playerRef, BossPuppet puppetRef, Action<Entity> addEntity,
            Action<Entity, float, LuaFunction> addEntityWithTimer, Action<Entity, string, LuaFunction, bool, bool> addEntityWithFlagger,
            Action<Entity, float, LuaFunction> addTimerToEntity, Action<Entity, string, LuaFunction, bool, bool> addFlaggerToEntity,
            Action<Entity> destroyEntity, Action destroyAll)
        {
            public Player playerRef = playerRef;

            public BossPuppet puppetRef = puppetRef;

            public Action<Entity> addEntity = addEntity;

            public Action<Entity, float, LuaFunction> addEntityWithTimer = addEntityWithTimer;

            public Action<Entity, string, LuaFunction, bool, bool> addEntityWithFlagger = addEntityWithFlagger;

            public Action<Entity, float, LuaFunction> addTimerToEntity = addTimerToEntity;

            public Action<Entity, string, LuaFunction, bool, bool> addFlaggerToEntity = addFlaggerToEntity;

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

        private readonly List<EntityTimer> activeEntityTimers;

        private readonly List<EntityFlagger> activeEntityFlaggers;

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
            activeEntityTimers = new List<EntityTimer>();
            activeEntityFlaggers = new List<EntityFlagger>();
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
        }

        private void PopulatePatterns(Vector2 levelOffset)
        {
            Patterns = new();
            UserFileReader.ReadPatternFileInto(patternsPath, ref Patterns, levelOffset);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player player = scene.Tracker.GetEntity<Player>();
            PopulateAttacksEventsAndFunctions(player);
        }

        private void PopulateAttacksEventsAndFunctions(Player player)
        {
            UserFileReader.ReadAttackFilesInto(attacksPath, ref AllAttacks, BossID,
                new(player, Puppet, AddEntity, AddEntityWithTimer, AddEntityWithFlagger, AddTimerToEntity, AddFlaggerToEntity, DestroyEntity, DestroyAll));
            UserFileReader.ReadEventFilesInto(eventsPath, ref AllEvents, BossID, player, Puppet,
                new(RemoveBoss));
            UserFileReader.ReadCustomCodeFileInto(functionsPath, out BossFunctions bossReactions, BossID,
                new(player, Puppet, () => Health, (val) => Health = val, (val) => Health -= val, WaitForAttackToEnd,
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
                    if (Patterns[currentPatternIndex].GoToPattern == null)
                    {
                        StartNextAttackPattern();
                    }
                    else
                    {
                        StartAttackPattern((int)Patterns[currentPatternIndex].GoToPattern);
                    }
                }
            }
            int index = 0;
            while (index < activeEntityTimers.Count)
            {
                EntityTimer timer = activeEntityTimers[index];
                if (EntityFullyAdded(timer.target) && timer.Done)
                {
                    timer.Execute();
                    activeEntityTimers.RemoveAt(index);
                }
                else
                {
                    activeEntityTimers[index] = timer.UpdateTimer();
                    index++;
                }
            }
            index = 0;
            while (index < activeEntityFlaggers.Count)
            {
                EntityFlagger flagger = activeEntityFlaggers[index];
                if (EntityFullyAdded(flagger.target) && flagger.Ready)
                {
                    flagger.Execute();
                    if (flagger.resetFlag)
                    {
                        Level.Session.SetFlag(flagger.flag, !flagger.stateNeeded);
                    }
                    activeEntityFlaggers.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }
        }

        private bool EntityFullyAdded(Entity entity)
        {
            return activeEntities.Contains(entity) && !Level.Entities.ToAdd.Contains(entity);
        }

        private bool IsPlayerWithinSpecifiedRegion(Vector2 entityPos)
        {
            return Patterns[currentPatternIndex].FinishMode == BossPattern.FinishModes.PlayerPositionWithin
                && Patterns[currentPatternIndex].PlayerPositionTrigger.Contains((int)entityPos.X, (int)entityPos.Y);
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
                    while (!cutscene.finished)
                    {
                        yield return null;
                    }
                }
                else
                {
                    Logger.Log(LogLevel.Error, "Bosses Helper", "Could not find specified event file.");
                }
                if (pattern.GoToPattern == null)
                {
                    StartNextAttackPattern();
                }
                StartAttackPattern((int)pattern.GoToPattern);
            }
            //Random Pattern
            if (pattern.RandomPattern)
            {
                while (true)
                {
                    int nextAttack = random.Next() % pattern.StatePatternOrder.Length;
                    yield return PerformMethod(pattern.StatePatternOrder[nextAttack]);
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
            int currentAction = 0;
            while (true)
            {
                if (currentAction >= pattern.StatePatternOrder.Length)
                {
                    loop++;
                    currentAction = 0;
                }
                if (pattern.FinishMode == BossPattern.FinishModes.LoopCountGoTo && loop > pattern.IterationCount)
                {
                    if (pattern.GoToPattern == null)
                    {
                        StartNextAttackPattern();
                    }
                    StartAttackPattern((int)pattern.GoToPattern);
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

        //Delegate methods
        //Interruption Delegates
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
            //activeEntityTimers.ForEach(timer => timer.ExecuteEarly());
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

        //Attack Delegates
        private void AddEntity(Entity entity)
        {
            if (!activeEntities.Contains(entity))
            {
                Level.Add(entity);
                activeEntities.Add(entity);
                entity.Scene = Level;
            }
        }

        private void AddTimerToEntity(Entity entity, float timer, LuaFunction action)
        {
            activeEntityTimers.Add(EntityTimer.DoActionOnEntityDelay(action, entity, timer));
        }

        private void AddFlaggerToEntity(Entity entity, string flag, LuaFunction action, bool state = true, bool resetFlag = true)
        {
            activeEntityFlaggers.Add(EntityFlagger.DoActionOnEntityOnFlagState(action, entity, flag, state, resetFlag));
        }

        private void AddEntityWithTimer(Entity entity, float timer, LuaFunction action)
        {
            AddEntity(entity);
            AddTimerToEntity(entity, timer, action);
        }

        private void AddEntityWithFlagger(Entity entity, string flag, LuaFunction action, bool state = true, bool resetFlag = true)
        {
            AddEntity(entity);
            AddFlaggerToEntity(entity, flag, action, state, resetFlag);
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
            activeEntityTimers.Clear();
            activeEntityFlaggers.Clear();
            activeEntities.ForEach(entity => entity.RemoveSelf());
            activeEntities.Clear();
        }
    }
}
