---@meta Celeste

---@module "Monocle"

---@class Celeste
---@field MiniTextbox fun(dialogId: string): Entity
---@field LevelLoader fun(session: Session, respawn: Vector2): Scene
---@field WindController fun(patterns: userdata): Entity
local celeste = {}

---@module "Celeste.Mod"
celeste.Mod = {}

---@class Actor : Entity
celeste.Actor = {}

---@class Audio
---@field Play fun(event: string, position?: Vector2): FMOD.Studio.EventInstance
---@field CurrentMusic string
celeste.Audio = {}

---@class AudioState
---@field Apply fun(self: AudioState, forceSixteenthNoteHack?: boolean)
---@field Music AudioTrackState
celeste.AudioState = {}

---@class AudioTrackState
---@field Event string
---@field Progress integer
---@field Layer fun(self: AudioTrackState, layer: integer, value: number|boolean)
celeste.AudioTrackState = {}

---@class BadelineOldsite : Entity
---@overload fun(position: Vector2, index: integer): BadelineOldsite
celeste.BadelineOldsite = {}

---@class BloomRenderer
---@field Strength number
celeste.BloomRenderer = {}

---@class Dialog
---@field Get fun(name: string)
celeste.Dialog = {}

---@class EntityData
---@field ID integer
---@field Level Level
---@field Position Vector2
---@field Width integer
---@field Height integer
---@field Values System.Collections.Generic.Dictionary<string, any>
celeste.EntityData = {}

---@class EntityID
---@overload fun(level: string, id: integer): EntityID
celeste.EntityID = {}

---@class Key : Entity
---@field ID EntityID
---@overload fun(player: Player, id: EntityID): Key
celeste.Key = {}

---@class Level : Scene
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
celeste.Level = {}

---@class LevelData
---@field Bounds Rectangle
celeste.LevelData = {}

---@class MapData
---@field GetAt fun(self: MapData, at: Vector2): LevelData
celeste.MapData = {}

---@class Player : Actor
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
---@field DummyWalkTo fun(self: Player, x: number, walkBackwards?: boolean, speedMultiplier?: number, keepWalkingIntoWalls?: boolean): System.Collections.IEnumerator
---@field DummyRunTo fun(self: Player, x: number, fastAnimation?: boolean): System.Collections.IEnumerator
---@field Jump fun(self: Player, particles?: boolean, playSfx?: boolean)
---@field StartStarFly fun(self:Player)
---@field StartCassetteFly fun(self: Player, target: Vector2, control: Vector2)
celeste.Player = {}

---@enum IntroTypes
celeste.Player.IntroTypes = {}

---@class PlayerInventory
celeste.PlayerInventory = {}

---@class Postcard : Entity
---@field BeforeRender fun(self: Postcard)
---@field DisplayRoutine fun(self: Postcard): System.Collections.IEnumerator
---@overload fun(msg: string, sfxIn: string, sfxOut: string): Postcard
---@overload fun(msg: string, area: integer): Postcard
celeste.Postcard = {}

---@class Session
---@field Level string
---@field RespawnPoint Vector2
---@field UpdateLevelStartDashes fun(self: Session)
---@field LevelData LevelData
---@field MapData MapData
---@field DeathsInCurrentLevel integer
---@field Audio AudioState
---@field HitCheckpoint boolean
---@field Inventory PlayerInventory
---@field GetFlag fun(self: Session, flag: string): boolean
---@field SetFlag fun(self: Session, flag: string, value: boolean)
---@field Keys System.Collections.Generic.HashSet<EntityID>
---@field LevelFlags System.Collections.Generic.HashSet<string>
---@field GetLevelFlag fun(self: Session, flag: string): boolean
celeste.Session = {}

---@enum CoreModes
celeste.Session.CoreModes = {}

---@class SFX
---@field EventnameByHandle fun(handle: string): string
celeste.SFX = {}

---@class SoundSource : Component
---@overload fun(): SoundSource
---@field Play fun(self: SoundSource, event: string): SoundSource
celeste.SoundSource = {}

---@class Textbox
---@field Say fun(dialog: string): System.Collections.IEnumerator
celeste.Textbox = {}

return celeste