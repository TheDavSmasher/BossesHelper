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

        private static int Next => random.Next();

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

        public struct AttackDelegates(Player playerRef, BossPuppet puppetRef, Action<Entity> addEntity, Action<Entity, string, LuaFunction, float> addEntityWithTimer,
            Action<Entity, string, LuaFunction, bool, bool> addEntityWithFlagger, Action<Entity> destroyEntity, Action destroyAll)
        {
            public Player playerRef = playerRef;

            public BossPuppet puppetRef = puppetRef;

            public Action<Entity> addEntity = addEntity;

            public Action<Entity, string, LuaFunction, float> addEntityWithTimer = addEntityWithTimer;

            public Action<Entity, string, LuaFunction, bool, bool> addEntityWithFlagger = addEntityWithFlagger;

            public Action<Entity> destroyEntity = destroyEntity;

            public Action destroyAll = destroyAll;
        }

        public struct OnHitDelegates(Player playerRef, BossPuppet puppetRef, Func<int> getHealth, Action<int> setHealth, Action<int> decreaseHealth, Func<IEnumerator> waitForAttack,
            Action interruptPattern, Func<int> currentPattern, Action<int> startAttackPattern, Action<int, int, bool> savePhaseChangeToSession)
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
        }

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

        private readonly Dictionary<string, EntityTimer> activeEntityTimers;

        private readonly Dictionary<string, EntityFlagger> activeEntityFlaggers;

        private BossFunctions OnInterrupt;

        private readonly string attacksPath;

        private readonly string eventsPath;

        private readonly string interruptPath;

        private readonly string patternsPath;

        private readonly string customSetupPath;

        public BossController(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Health = data.Int("bossHealthMax", -1);
            startAttackingImmediately = data.Bool("startAttackingImmediately");
            attacksPath = data.Attr("attacksPath");
            eventsPath = data.Attr("eventsPath");
            interruptPath = data.Attr("interruptPath");
            patternsPath = data.Attr("patternsPath");
            customSetupPath = data.Attr("customSetupPath");
            isAttacking = false;
            AllAttacks = new Dictionary<string, BossAttack>();
            AllEvents = new Dictionary<string, BossEvent>();
            PopulatePatterns();
            currentPatternIndex = 0;
            currentPattern = new Coroutine();
            Add(currentPattern);
            Puppet = new BossPuppet(data, offset);
            activeEntities = new List<Entity>();
            activeEntityTimers = new Dictionary<string, EntityTimer>();
            activeEntityFlaggers = new Dictionary<string, EntityFlagger>();
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
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player player = scene.Tracker.GetEntity<Player>();
            PopulateAttacksEventsAndInterrupt(player);
            Puppet.SetOnInterrupt(OnInterrupt);
            Puppet.SetCustomBossSetup(customSetupPath, player);
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
                    currentPatternIndex = (int)Patterns[currentPatternIndex].GoToPattern;
                    StartAttackPattern(currentPatternIndex);
                }
            }
            foreach (KeyValuePair<string, EntityTimer> entityTimer in activeEntityTimers)
            {
                if (entityTimer.Value.Done && !Level.Entities.ToAdd.Contains(entityTimer.Value.target))
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
                if (entityFlagger.Ready && !Level.Entities.ToAdd.Contains(entityFlagger.target))
                {
                    entityFlagger.action.Call(entityFlagger.target);
                    if (entityFlagger.resetFlag)
                    {
                        Level.Session.SetFlag(entityFlagger.flag, !entityFlagger.stateNeeded);
                    }
                    activeEntityFlaggers.Remove(entityFlagger.flag);
                }
            }
        }

        private bool IsPlayerWithinSpecifiedRegion(Vector2 entityPos)
        {
            return Patterns[currentPatternIndex].FinishMode == BossPattern.FinishModes.PlayerPositionWithin
                && Patterns[currentPatternIndex].PlayerPositionTrigger.Contains((int)entityPos.X, (int)entityPos.Y);
        }

        private void StartAttackPattern(int goTo = -1)
        {
            if (goTo > 0)
            {
                currentPatternIndex = goTo;
            }
            currentPattern.Replace(PerformPattern(Patterns[currentPatternIndex]));
        }

        private void PopulateAttacksEventsAndInterrupt(Player player)
        {
            UserFileReader.ReadAttackFilesInto(attacksPath, ref AllAttacks,
                new(player, Puppet, AddEntity, AddEntityWithTimer, AddEntityWithFlagger, DestroyEntity, DestroyAll));
            UserFileReader.ReadEventFilesInto(eventsPath, ref AllEvents, player, Puppet);
            UserFileReader.ReadOnHitFileInto(interruptPath, ref OnInterrupt,
                new(player, Puppet, () => Health, (val) => Health = val, (val) => Health -= val, WaitForAttackToEnd, InterruptPattern, () => currentPatternIndex, StartAttackPattern, SavePhaseChangeInSession));
        }

        private void PopulatePatterns()
        {
            Patterns = new();
            UserFileReader.ReadPatternFileInto(patternsPath, ref Patterns);
        }

        private IEnumerator PerformPattern(BossPattern pattern)
        {
            if (pattern.RandomPattern)
            {
                while (true)
                {
                    int nextAttack = Next % pattern.StatePatternOrder.Length;
                    yield return PerformMethod(pattern.StatePatternOrder[nextAttack]);
                }
            }
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
                    if (pattern.FinishMode == BossPattern.FinishModes.LoopCountGoTo && ++loop > pattern.IterationCount)
                    {
                        currentPatternIndex = (int) pattern.GoToPattern;
                        StartAttackPattern(currentPatternIndex);
                    }
                    currentAction = 0;
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
                else if (AllEvents.TryGetValue(method.ActionName, out BossEvent cutscene))
                {
                    Level.Add(cutscene);
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

        //Attack Delegates
        private void AddEntity(Entity entity)
        {
            Level.Add(entity);
            activeEntities.Add(entity);
            entity.Scene = Level;
        }

        private void AddEntityWithTimer(Entity entity, string id, LuaFunction action, float timer)
        {
            if (!activeEntityTimers.ContainsKey(id))
            {
                Level.Add(entity);
                activeEntities.Add(entity);
                entity.Scene = Level;
                activeEntityTimers.Add(id, EntityTimer.DoActionOnEntityDelay(action, entity, id, timer));
            }
        }

        private void AddEntityWithFlagger(Entity entity, string flag, LuaFunction action, bool state = true, bool resetFlag = true)
        {
            if (!activeEntityFlaggers.ContainsKey(flag))
            {
                Level.Add(entity);
                activeEntities.Add(entity);
                entity.Scene = Level;
                activeEntityFlaggers.Add(flag, EntityFlagger.DoActionOnEntityOnFlagState(action, entity, flag, state, resetFlag));
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
            activeEntityTimers.Clear();
            activeEntityFlaggers.Clear();
            activeEntities.ForEach(entity => entity.RemoveSelf());
            activeEntities.Clear();
        }
    }
}
