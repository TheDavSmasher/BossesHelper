---@meta CelesteMod

---@class _G
---@field luanet any Luanet server

---@class System.Collections.IEnumerator

---@class FMOD.Studio.EventInstance

---@class Celeste.Audio
---@field Play fun(event: string, position?: Microsoft.Xna.Framework.Vector2): FMOD.Studio.EventInstance
---@field CurrentMusic string

---@class Celeste.SFX
---@field EventnameByHandle fun(handle: string): string

---@class Celeste.SoundSource : Monocle.Component
---@field Play fun(self: Celeste.SoundSource, event: string): Celeste.SoundSource

---@class Celeste.LevelData
---@field Bounds Microsoft.Xna.Framework.Rectangle

---@class Celeste.MapData
---@field GetAt fun(self: Celeste.MapData, at: Microsoft.Xna.Framework.Vector2): Celeste.LevelData

---@class Celeste.AudioTrackState
---@field Event string
---@field Progress integer
---@field Layer fun(self: Celeste.AudioTrackState, layer: integer, value: number|boolean)

---@class Celeste.AudioState
---@field Apply fun(self: Celeste.AudioState, forceSixteenthNoteHack?: boolean)
---@field Music Celeste.AudioTrackState

---@class Celeste.PlayerInventory

---@class Celeste.BloomRenderer
---@field Strength number

---@class Celeste.EntityID

---@class System.Collections.Generic.HashSet<T>
---@field Add fun(self: System.Collections.Generic.HashSet, value: `T`)
---@field Remove fun(self: System.Collections.Generic.HashSet, value: `T`)

---@class Celeste.Session
---@field Level string
---@field RespawnPoint Microsoft.Xna.Framework.Vector2
---@field UpdateLevelStartDashes fun(self: Celeste.Session)
---@field LevelData Celeste.LevelData
---@field MapData Celeste.MapData
---@field DeathsInCurrentLevel integer
---@field Audio Celeste.AudioState
---@field HitCheckpoint boolean
---@field Inventory Celeste.PlayerInventory
---@field CoreModes CoreModes
---@field GetFlag fun(self: Celeste.Session, flag: string): boolean
---@field SetFlag fun(self: Celeste.Session, flag: string, value: boolean)
---@field Keys System.Collections.Generic.HashSet<Celeste.EntityID>
---@field LevelFlags System.Collections.Generic.HashSet<string>
---@field GetLevelFlag fun(self: Celeste.Session, flag: string): boolean

---@class Monocle.Scene
---@field Add fun(self: Monocle.Scene, entity: Monocle.Entity)
---@field Remove fun(self: Monocle.Scene, entity: Monocle.Entity)

---@class Microsoft.Xna.Framework.Rectangle
---@field Left number
---@field Bottom number
---@field X number
---@field Y number

---@enum CoreModes

---@class Monocle.Camera
---@field Position Microsoft.Xna.Framework.Vector2

---@class Celeste.Level : Monocle.Scene
---@field Session Celeste.Session
---@field GetSpawnPoint fun(self: Celeste.Level, at: Microsoft.Xna.Framework.Vector2)
---@field Bounds Microsoft.Xna.Framework.Rectangle
---@field CompleteArea fun(self: Celeste.Level, spotlightWipe?: boolean, skipScreenWipe?: boolean, skipCompleteScreen?: boolean)
---@field Shake fun(self: Celeste.Level, time?: number)
---@field DirectionalShake fun(self: Celeste.Level, dir: Microsoft.Xna.Framework.Vector2, time?: number)
---@field CameraOffset Microsoft.Xna.Framework.Vector2
---@field LevelOffset Microsoft.Xna.Framework.Vector2
---@field Bloom Celeste.BloomRenderer
---@field CoreMode CoreModes
---@field SnapColorGrade fun(self: Celeste.Level, next: string)
---@field NextColorGrade fun(self: Celeste.Level, next: string, time?: number)
---@field Camera Monocle.Camera
---@field CanRetry boolean
---@field PauseLock boolean
---@field InCutscene boolean
---@field CancelCutscene fun(self: Celeste.Level)

---@class Monocle.Component

---@class Monocle.Collider : Monocle.Component A Monocle Collider object.

---@class Monocle.ColliderList : Monocle.Collider A Monocle ColliderList object, combining multiple Colliders.

---@class Monocle.Engine The Monocle Engine
---@field Scene Monocle.Scene

---@class Monocle.Sprite : Monocle.Component
---@field Play fun(self: Monocle.Sprite, anim: string)

---@class Celeste.Mod.BossesHelper.Code.Components.Stopwatch : Monocle.Component
---@field TimeLeft number
---@field Reset fun(self: Celeste.Mod.BossesHelper.Code.Components.Stopwatch)

---@class System.Random
---@field Next fun(self: System.Random): integer

---@class Microsoft.Xna.Framework.Vector2
---@field X number The x component of the vector
---@field Y number The y component of the vector
---@field Length fun(self: Microsoft.Xna.Framework.Vector2): number
---@operator add(Microsoft.Xna.Framework.Vector2): Microsoft.Xna.Framework.Vector2
---@operator sub(Microsoft.Xna.Framework.Vector2): Microsoft.Xna.Framework.Vector2

---@class System.Collections.Generic.Dictionary<K, V>
---@field Add fun(self: System.Collections.Generic.Dictionary, key: `K`, value: `V`)

---@class Celeste.EntityData An Everest EntityData object.
---@field ID integer
---@field Level Celeste.Level
---@field Position Microsoft.Xna.Framework.Vector2
---@field Width integer
---@field Height integer
---@field Values System.Collections.Generic.Dictionary<string, any>

---@class Monocle.Entity
---@field Add fun(self: Monocle.Entity, component: Monocle.Component) Adds a component to the Entity
---@field Position Microsoft.Xna.Framework.Vector2
---@field Center Microsoft.Xna.Framework.Vector2
---@field Collidable boolean

---@class Celeste.Actor : Monocle.Entity

---@class Celeste.BadelineOldsite : Monocle.Entity

---@class Monocle.Calc
---@field SafeNormalize fun(vector2: Microsoft.Xna.Framework.Vector2, length?: number): Microsoft.Xna.Framework.Vector2

---@class Monocle.Circle : Monocle.Collider

---@class Monocle.Hitbox : Monocle.Collider

---@alias Monocle.Easer fun(t: number): number

---@class Monocle.Ease : { [string]: Monocle.Easer }
---@field Invert fun(easer: Monocle.Easer): Monocle.Easer

---@class Monocle Namespace
---@field Ease Monocle.Ease
---@field Engine Monocle.Engine
---@field Calc Monocle.Calc
---@field Circle fun(radius: number, x: number, y: number): Monocle.Circle
---@field Hitbox fun(width: number, height: number, x: number, y: number): Monocle.Hitbox

---@class Celeste.Textbox
---@field Say fun(dialog: string): System.Collections.IEnumerator

---@class Celeste.Dialog
---@field Get fun(name: string)

---@class Celeste.Postcard : Monocle.Entity
---@field BeforeRender fun(self: Celeste.Postcard)
---@field DisplayRoutine fun(self: Celeste.Postcard): System.Collections.IEnumerator

---@class Celeste.Key : Monocle.Entity
---@field ID Celeste.EntityID

---@class Monocle.StateMachine
---@field State integer
---@field Locked boolean

---@enum Celeste.Player.IntroTypes

---@class Celeste.Player : Celeste.Actor
---@field IntroType Celeste.Player.IntroTypes
---@field StateMachine Monocle.StateMachine
---@field Dead boolean
---@field DummyFriction boolean
---@field DummyAutoAnimate boolean
---@field AutoJump boolean
---@field AutoJumpTimer number
---@field Speed Microsoft.Xna.Framework.Vector2
---@field Sprite Monocle.Sprite
---@field Die fun(self: Celeste.Player, dir: Microsoft.Xna.Framework.Vector2, evenIfInvincible?: boolean, registerDeathInStats?: boolean)
---@field OnGround fun(self: Celeste.Player, at: number?): boolean
---@field DummyWalkTo fun(self: Celeste.Player, x: number, walkBackwards?: boolean, speedMultiplier?: number, keepWalkingIntoWalls?: boolean): System.Collections.IEnumerator
---@field DummyRunTo fun(self: Celeste.Player, x: number, fastAnimation?: boolean): System.Collections.IEnumerator
---@field Jump fun(self: Celeste.Player, particles?: boolean, playSfx?: boolean)
---@field StartStarFly fun(self:Celeste.Player)
---@field StartCassetteFly fun(self: Celeste.Player, target: Microsoft.Xna.Framework.Vector2, control: Microsoft.Xna.Framework.Vector2)
player = {}

---@class Celeste.Mod.Logger
---@field Error fun(tag: string, message: string)
---@field Info fun(tag: string, message: string)

---@class Celeste.Mod.BossesHelper.Code.Entities.AttackEntity : Monocle.Entity
---@field Sprite Monocle.Sprite
---@field PlayAnim fun(self: Celeste.Mod.BossesHelper.Code.Entities.AttackEntity, anim: string)
---@field SetCollisionActive fun(self: Celeste.Mod.BossesHelper.Code.Entities.AttackEntity, state: boolean)
---@alias BossesHelper.AttackEntity Celeste.Mod.BossesHelper.Code.Entities.AttackEntity

---@class Celeste.Mod.BossesHelper.Code.Entities.AttackActor : Celeste.Actor
---@field Sprite Monocle.Sprite
---@field PlayAnim fun(self: Celeste.Mod.BossesHelper.Code.Entities.AttackEntity, anim: string)
---@field SetCollisionActive fun(self: Celeste.Mod.BossesHelper.Code.Entities.AttackEntity, state: boolean)
---@field Speed Microsoft.Xna.Framework.Vector2
---@field GravityMult number
---@field SolidCollidable boolean
---@field Grounded boolean
---@field SetSolidCollisionActive fun(self: Celeste.Mod.BossesHelper.Code.Entities.AttackActor, value: boolean)
---@field SetEffectiveGravityMult fun(self: Celeste.Mod.BossesHelper.Code.Entities.AttackActor, mult: number)
---@alias BossesHelper.AttackActor Celeste.Mod.BossesHelper.Code.Entities.AttackActor

---@class Celeste.Mod.BossesHelper.Code.Components.EntityChecker : Monocle.Component
---@alias BossesHelper.EntityChecker Celeste.Mod.BossesHelper.Code.Components.EntityChecker

---@class Celeste.Mod.BossesHelper.Code.Components.EntityTimer : Monocle.Component
---@alias BossesHelper.EntityTimer Celeste.Mod.BossesHelper.Code.Components.EntityTimer

---@class Celeste.Mod.BossesHelper.Code.Components.EntityFlagger : Monocle.Component
---@alias BossesHelper.EntityFlagger Celeste.Mod.BossesHelper.Code.Components.EntityFlagger

---@class Celeste.Mod.BossesHelper.Code.Components.EntityChain : Monocle.Component
---@alias BossesHelper.EntityChain Celeste.Mod.BossesHelper.Code.Components.EntityChain

---@class Celeste.Mod.BossesHelper.Code.Components Namespace
---@field EntityChecker fun(checker: (fun(): boolean), onCheck: fun(entity: Monocle.Entity), state?: boolean, removeOnComplete?: boolean): BossesHelper.EntityChecker
---@field EntityTimer fun(timer: number, onTimer: fun(entity: Monocle.Entity)): BossesHelper.EntityTimer
---@field EntityFlagger fun(flag: string, onFlag: fun(entity: Monocle.Entity), state?: boolean, reset?: boolean): BossesHelper.EntityFlagger
---@field EntityChain fun(entity: Monocle.Entity, startChained: boolean, removeTogether?: boolean): BossesHelper.EntityChain

---@class Celeste.Mod.BossesHelper.Code.Entities Namespace
---@field AttackEntity fun(position: Microsoft.Xna.Framework.Vector2, hitboxes: Monocle.Collider, funcOnPlayer: fun(self: Monocle.Entity, player: Celeste.Player), startCollidable: boolean, spriteName: string, xScale?: number, yScale?: number) : BossesHelper.AttackEntity
---@field AttackActor fun(position: Microsoft.Xna.Framework.Vector2, hitboxes: Monocle.Collider, funcOnPlayer: fun(self: Monocle.Entity, player: Celeste.Player), startCollidable: boolean, startSolidCollidable: boolean, spriteName: string, gravMult: number, maxFall: number, xScale?: number, yScale?: number): BossesHelper.AttackActor

---@class Celeste.Mod.BossesHelper.BossesHelperModule
---@field PlayerHealth integer
---@field GiveIFrames fun(time: number)
---@field MakeEntityData fun(): Celeste.EntityData

---@class Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils
---@field PlayAnim fun(self: Monocle.Sprite, anim: string): System.Collections.IEnumerator
---@field PositionTween fun(self: Monocle.Entity, target: Microsoft.Xna.Framework.Vector2, time: number, easer?: Monocle.Easer)

---@class Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper
---@field GetFileContent fun(file: string): string
---@field GetColliderListFromLuaTable fun(...: Monocle.Collider): Monocle.ColliderList
---@field AddConstantBackgroundCoroutine fun(entity: Monocle.Entity, func: function)
---@field Say fun(dialog: string, funcs: function[]): System.Collections.IEnumerator
---@field DoMethodAfterDelay fun(func: function, delay: number)

---@class Celeste.Mod.BossesHelper.Code.Helpers.Lua.LuaMethodWrappers
---@field TeleportTo fun(scene: Monocle.Scene, player: Celeste.Player, room: string, intro?: Celeste.Player.IntroTypes, nearest?: Microsoft.Xna.Framework.Vector2)
---@field InstantTeleport fun(scene: Monocle.Scene, player: Celeste.Player, room: string, relative: boolean, posX: number, postY: number)
---@field GetEntities fun(name: string, prefix?: string): table
---@field GetAllEntities fun(name: string, prefix?: string): table
---@field GetEntity fun(name: string, prefix?: string): any
---@field GetFirstEntity fun(name: string, prefix?: string): any
---@field GetComponents fun(name: string, prefix?: string): table
---@field GetAllComponents fun(name: string, prefix?: string): table
---@field GetComponent fun(name: string, prefix?: string): any
---@field GetFirstComponent fun(name: string, prefix?: string): any
---@field GetAllComponentsOnType fun(name: string, entity: string, prefix?: string, entityPrefix?: string): table
---@field GetFirstComponentOnType fun(name: string, entity: string, prefix?: string, entityPrefix?: string): any
---@field GetComponentsFromEntity fun(entity: Monocle.Entity, name: string, prefix?: string): table
---@field GetComponentFromEntity fun(entity: Monocle.Entity, name: string, prefix?: string): any
---@field EntityHasComponent fun(entity: Monocle.Entity, name: string, prefix?: string): boolean

---@class Celeste.Mod.BossesHelper.Code.Helpers.Lua Namespace
---@field LuaMethodWrappers Celeste.Mod.BossesHelper.Code.Helpers.Lua.LuaMethodWrappers

---@class Celeste.Mod.BossesHelper.Code.Helpers Namespace
---@field BossesHelperUtils Celeste.Mod.BossesHelper.Code.Helpers.BossesHelperUtils
---@field LuaBossHelper Celeste.Mod.BossesHelper.Code.Helpers.LuaBossHelper
---@field Lua Celeste.Mod.BossesHelper.Code.Helpers.Lua

---@class Celeste.Mod.BossesHelper.Code Namespace
---@field Components Celeste.Mod.BossesHelper.Code.Components
---@field Entities Celeste.Mod.BossesHelper.Code.Entities
---@field Helpers Celeste.Mod.BossesHelper.Code.Helpers

---@class Celeste.Mod.BossesHelper Namespace
---@field BossesHelperModule Celeste.Mod.BossesHelper.BossesHelperModule
---@field Code Celeste.Mod.BossesHelper.Code

---@class Celeste.Mod.LuaCutscenes.ChoicePrompt : Monocle.Entity
---@field Choice integer
---@field Prompt fun(...: string): System.Collections.IEnumerator

---@class Celeste.Mod.LuaCutscenes Namespace
---@field ChoicePrompt Celeste.Mod.LuaCutscenes.ChoicePrompt

---@class Celeste.Mod Namespace
---@field Logger Celeste.Mod.Logger
---@field LuaCutscenes Celeste.Mod.LuaCutscenes
---@field BossesHelper Celeste.Mod.BossesHelper

---@class Celeste Namespace
---@field Mod Celeste.Mod
---@field PlayerInventory Celeste.PlayerInventory
---@field Textbox Celeste.Textbox
---@field Dialog Celeste.Dialog
---@field Player Celeste.Player
---@field Audio Celeste.Audio
---@field SFX Celeste.SFX
---@field EntityID fun(level: string, id: integer): Celeste.EntityID
---@field MiniTextbox fun(dialogId: string): Monocle.Entity
---@field Postcard fun(msg: string, sfxIn: string, sfxOut: string): Celeste.Postcard
---@field Postcard fun(msg: string, area: integer): Celeste.Postcard
---@field LevelLoader fun(session: Celeste.Session, respawn: Microsoft.Xna.Framework.Vector2): Monocle.Scene
---@field BadelineOldsite fun(position: Microsoft.Xna.Framework.Vector2, index: integer): Celeste.BadelineOldsite
---@field Key fun(player: Celeste.Player, id: Celeste.EntityID): Celeste.Key
---@field WindController fun(patterns: userdata): Monocle.Entity
---@field SoundSource fun(): Celeste.SoundSource

---@class Celeste.Mod.BossesHelper.Code.Entities.BossPuppet : Celeste.Actor
---@field Speed Microsoft.Xna.Framework.Vector2
---@field Grounded boolean
---@field gravityMult number
---@field groundFriction number
---@field airFriction number
---@field Sprite Monocle.Sprite
---@field SolidCollidable boolean
---@field BossHitCooldown number
---@field BossDamageCooldown Celeste.Mod.BossesHelper.Code.Components.Stopwatch
---@field PlayBossAnim fun(self : BossesHelper.BossPuppet, anim: string)
---@field Set1DSpeedDuring fun(self: BossesHelper.BossPuppet, speed: number, isX: boolean, time: number) *
---@field Speed1DTween fun(self: BossesHelper.BossPuppet, start: number, target: number, time: number, isX: boolean, easer?: Monocle.Easer) *
---@field ChangeHitboxOption fun(self : BossesHelper.BossPuppet, tag: string) *
---@field ChangeHurtboxOption fun(self : BossesHelper.BossPuppet, tag: string) *
---@field ChangeBounceboxOption fun(self : BossesHelper.BossPuppet, tag: string) *
---@field ChangeTargetOption fun(self : BossesHelper.BossPuppet, tag: string) *
puppet = {}
---@alias BossesHelper.BossPuppet Celeste.Mod.BossesHelper.Code.Entities.BossPuppet

---@class Celeste.Mod.BossesHelper.Code.Entities.BossController : Monocle.Entity
---@field Health integer
---@field IsActing boolean *
---@field CurrentPatternIndex integer
---@field CurrentPatternName string *
---@field Random System.Random
---@field AddEntity fun(self: BossesHelper.BossController, entity: Monocle.Entity) *
---@field DestroyEntity fun(self: BossesHelper.BossController, entity: Monocle.Entity) *
---@field DestroyAll fun(self: BossesHelper.BossController)
---@field InterruptPattern fun(self: BossesHelper.BossController)
---@field GetPatternIndex fun(self: BossesHelper.BossController, name: string): integer *
---@field StartAttackPattern fun(self: BossesHelper.BossController, index: number)
---@field ForceNextAttack fun(self: BossesHelper.BossController, index: number) *
---@field SavePhaseChangeToSession fun(self: BossesHelper.BossController, health: integer, index: integer, startImmediately: boolean) *
---@field RemoveBoss fun(self: BossesHelper.BossController, permanent: boolean) *
---@field StoreObject fun(self: BossesHelper.BossController, key: string, value: any) *
---@field GetStoredObject fun(self: BossesHelper.BossController, key: string): any *
---@field DeleteStoredObject fun(self: BossesHelper.BossController, key: string) *
---@field DecreaseHealth fun(self: BossesHelper.BossController, amount?: integer)
boss = {}
---@alias BossesHelper.BossController Celeste.Mod.BossesHelper.Code.Entities.BossController

---@class Celeste.Mod.EverestModuleMetadata
---@field Name string
modMetaData = {}

---@type string
bossId = ""