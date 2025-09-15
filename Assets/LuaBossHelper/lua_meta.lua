---@meta CelesteMod

---@class _G
---@field luanet any Luanet server

---@class IEnumerator

---@class EventInstance

---@class Audio Celeste Audio
---@field Play fun(event: string, position?: Vector2): EventInstance
---@field CurrentMusic string

---@class SFX
---@field EventnameByHandle fun(handle: string): string

---@class SoundSource : Component
---@field Play fun(self: SoundSource, event: string): SoundSource

---@class LevelData
---@field Bounds Rectangle

---@class MapData
---@field GetAt fun(self: MapData, at: Vector2): LevelData

---@class AudioTrackState
---@field Event string
---@field Progress integer
---@field Layer fun(self: AudioTrackState, layer: integer, value: number|boolean)

---@class AudioState
---@field Apply fun(self: AudioState, forceSixteenthNoteHack?: boolean)
---@field Music AudioTrackState

---@class PlayerInventory

---@class BloomRenderer
---@field Strength number

---@class EntityID

---@class HashSet<T>
---@field Add fun(self: HashSet, value: `T`)
---@field Remove fun(self: HashSet, value: `T`)

---@class Session A Monocle Session
---@field Level string
---@field RespawnPoint Vector2
---@field UpdateLevelStartDashes fun(self: Session)
---@field LevelData LevelData
---@field MapData MapData
---@field DeathsInCurrentLevel integer
---@field Audio AudioState
---@field HitCheckpoint boolean
---@field Inventory PlayerInventory
---@field CoreModes CoreModes
---@field GetFlag fun(self: Session, flag: string): boolean
---@field SetFlag fun(self: Session, flag: string, value: boolean)
---@field Keys HashSet<EntityID>
---@field LevelFlags HashSet<string>
---@field GetLevelFlag fun(self: Session, flag: string): boolean

---@class Scene A Monocle Scene
---@field Add fun(self: Scene, entity: Entity)
---@field Remove fun(self: Scene, entity: Entity)

---@class Rectangle
---@field Left number
---@field Bottom number
---@field X number
---@field Y number

---@enum CoreModes

---@class Camera
---@field Position Vector2

---@class Level : Scene A Monocle Level
---@field Session Session
---@field GetSpawnPoint fun(self: Level, at: Vector2)
---@field Bounds Rectangle
---@field CompleteArea fun(self: Level, spotlightWipe?: boolean, skipScreenWipe?: boolean, skipCompleteScreen?: boolean)
---@field Shake fun(self: Level, time?: number)
---@field DirectionalShake fun(self: Level, dir: Vector2, time?: number)
---@field CameraOffset Vector2
---@field LevelOffset Vector2
---@field Bloom BloomRenderer
---@field CoreMode CoreModes
---@field SnapColorGrade fun(self: Level, next: string)
---@field NextColorGrade fun(self: Level, next: string, time?: number)
---@field Camera Camera
---@field CanRetry boolean
---@field PauseLock boolean
---@field InCutscene boolean
---@field CancelCutscene fun(self: Level)

---@class Component A Monocle Component object.

---@class Collider : Component A Monocle Collider object.

---@class ColliderList : Collider A Monocle ColliderList object, combining multiple Colliders.

---@class Engine The Monocle Engine
---@field Scene Scene

---@class Sprite : Component
---@field Play fun(self: Sprite, anim: string)

---@class Stopwatch : Component
---@field TimeLeft number
---@field Reset fun(self: Stopwatch)

---@class Random
---@field Next fun(self: Random): integer

---@class Vector2 A Vector2 object.
---@field X number The x component of the vector
---@field Y number The y component of the vector
---@field Length fun(self: Vector2): number
---@operator add(Vector2): Vector2
---@operator sub(Vector2): Vector2

---@class Dictionary<K, V>
---@field Add fun(self: Dictionary, key: `K`, value: `V`)

---@class EntityData An Everest EntityData object.
---@field ID integer
---@field Level Level
---@field Position Vector2
---@field Width integer
---@field Height integer
---@field Values Dictionary<string, any>

---@class Entity A Monocle Entity object
---@field Add fun(self: Entity, component: Component) Adds a component to the Entity
---@field Position Vector2
---@field Center Vector2
---@field Collidable boolean

---@class BossesHelperUtils
---@field PlayAnim fun(self: Sprite, anim: string): IEnumerator
---@field PositionTween fun(self: Entity, target: Vector2, time: number, easer: Easer?)

---@class BadelineOldsite : Entity

---@class Calc Monocle.Calc
---@field SafeNormalize fun(vector2: Vector2, length?: number): Vector2

---@class Circle : Collider
---@overload fun(radius: number, x: number, y: number): Circle

---@class Hitbox : Collider
---@overload fun(width: number, height: number, x: number, y: number): Hitbox

---@class Easer A Monocle Easer, used for Tweens.

---@class Ease : { [string]: Easer }
---@field Invert fun(easer: Easer): Easer

---@class Monocle
---@field Ease Ease
---@field Engine Engine
---@field Calc Calc
---@field Circle Circle
---@field Hitbox Hitbox

---@class Textbox
---@field Say fun(dialog: string): IEnumerator

---@class Dialog
---@field Get fun(name: string)

---@class Postcard : Entity
---@field BeforeRender fun(self: Postcard)
---@field DisplayRoutine fun(self: Postcard): IEnumerator

---@class Key : Entity
---@field ID EntityID

---@class StateMachine
---@field State integer
---@field Locked boolean

---@enum IntroTypes

---@class Player : Entity
---@field IntroType IntroTypes
---@field StateMachine StateMachine
---@field Dead boolean
---@field DummyFriction boolean
---@field DummyAutoAnimate boolean
---@field AutoJump boolean
---@field AutoJumpTimer number
---@field Speed Vector2
---@field Sprite Sprite
---@field Die fun(self: Player, dir: Vector2, evenIfInvincible?: boolean, registerDeathInStats?: boolean)
---@field OnGround fun(self: Player, at: number?): boolean
---@field DummyWalkTo fun(self: Player, x: number, walkBackwards?: boolean, speedMultiplier?: number, keepWalkingIntoWalls?: boolean): IEnumerator
---@field DummyRunTo fun(self: Player, x: number, fastAnimation?: boolean): IEnumerator
---@field Jump fun(self: Player, particles?: boolean, playSfx?: boolean)
---@field StartStarFly fun(self:Player)
---@field StartCassetteFly fun(self: Player, target: Vector2, control: Vector2)
player = {}

---@class Logger
---@field Error fun(tag: string, message: string)
---@field Info fun(tag: string, message: string)

---@class BossesHelperModule
---@field MakeEntityData fun(): EntityData

---@class Helpers
---@field BossesHelperUtils BossesHelperUtils

---@class Code
---@field Components table
---@field Entities table
---@field Helpers Helpers

---@class BossesHelper
---@field BossesHelperModule BossesHelperModule
---@field Code Code

---@class ChoicePrompt : Entity
---@field Choice integer
---@field Prompt fun(...: string): IEnumerator

---@class LuaCutscenes
---@field ChoicePrompt ChoicePrompt

---@class Mod : { [string]: table }
---@field Logger Logger
---@field LuaCutscenes LuaCutscenes
---@field BossesHelper BossesHelper

---@class Celeste
---@field Mod Mod
---@field PlayerInventory PlayerInventory
---@field Textbox Textbox
---@field Dialog Dialog
---@field Player Player
---@field Audio Audio
---@field SFX SFX
---@field EntityID fun(level: string, id: integer): EntityID
---@field MiniTextbox fun(dialogId: string): Entity
---@field Postcard fun(msg: string, sfxIn: string, sfxOut: string): Postcard
---@field Postcard fun(msg: string, area: integer): Postcard
---@field LevelLoader fun(session: Session, respawn: Vector2): Scene
---@field BadelineOldsite fun(position: Vector2, index: integer): BadelineOldsite
---@field Key fun(player: Player, id: EntityID): Key
---@field WindController fun(patterns: userdata): Entity
---@field SoundSource fun(): SoundSource

---@class BossPuppet : Entity
---@field Speed Vector2
---@field Grounded boolean
---@field gravityMult number
---@field groundFriction number
---@field airFriction number
---@field Sprite Sprite
---@field SolidCollidable boolean
---@field BossHitCooldown number
---@field BossDamageCooldown Stopwatch
---@field PlayBossAnim fun(self : BossPuppet, anim: string)
---@field Set1DSpeedDuring fun(self: BossPuppet, speed: number, isX: boolean, time: number) *
---@field Speed1DTween fun(self: BossPuppet, start: number, target: number, time: number, isX: boolean, easer: Easer?) *
---@field ChangeHitboxOption fun(self : BossPuppet, tag: string) *
---@field ChangeHurtboxOption fun(self : BossPuppet, tag: string) *
---@field ChangeBounceboxOption fun(self : BossPuppet, tag: string) *
---@field ChangeTargetOption fun(self : BossPuppet, tag: string) *
puppet = {}

---@class BossController : Entity
---@field Health integer
---@field IsActing boolean *
---@field CurrentPatternIndex integer
---@field CurrentPatternName string *
---@field Random Random
---@field AddEntity fun(self: BossController, entity: Entity) *
---@field DestroyEntity fun(self: BossController, entity: Entity) *
---@field DestroyAll fun(self: BossController)
---@field InterruptPattern fun(self: BossController)
---@field GetPatternIndex fun(self: BossController, name: string): integer *
---@field StartAttackPattern fun(self: BossController, index: number)
---@field ForceNextAttack fun(self: BossController, index: number) *
---@field SavePhaseChangeToSession fun(self: BossController, health: integer, index: integer, startImmediately: boolean) *
---@field RemoveBoss fun(self: BossController, permanent: boolean) *
---@field StoreObject fun(self: BossController, key: string, value: any) *
---@field GetStoredObject fun(self: BossController, key: string): any *
---@field DeleteStoredObject fun(self: BossController, key: string) *
---@field DecreaseHealth fun(self: BossController, amount?: integer)
boss = {}

---@class ModMetadata
---@field Name string
modMetaData = {}

---@type string
bossId = ""