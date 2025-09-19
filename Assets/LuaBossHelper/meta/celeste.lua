---@meta Celeste

---@module "Monocle"

---@class Celeste
local celeste = {}

---@module "Celeste.Mod"
celeste.Mod = {}

---@class Actor : Entity
celeste.Actor = {}

--#region Audio
---@class Audio
---@field CurrentMusic string
celeste.Audio = {}

---Play the given audio event at the given global position.
---@param event string The event name to play.
---@param position? Vector2 Where to play the audio.
---@return FMOD.Studio.EventInstance # The Audio EventInstance
function celeste.Audio.Play(event, position) end
--#endregion

--#region AudioState
---@class AudioState
---@field Music AudioTrackState
celeste.AudioState = {}

---Apply the current changes to the Audio State
---@param forceSixteenthNoteHack? boolean
function celeste.AudioState:Apply(forceSixteenthNoteHack) end
--#endregion

--#region AudioTrackState
---@class AudioTrackState
---@field Event string
---@field Progress integer
celeste.AudioTrackState = {}

---Update the given audio layer.
---@param layer integer The layer to update.
---@param value boolean|number The value to give the layer.
function celeste.AudioTrackState:Layer(layer, value) end
--#endregion

---@class BadelineOldsite : Entity
---@overload fun(position: Vector2, index: integer): BadelineOldsite
celeste.BadelineOldsite = {}

---@class BloomRenderer
---@field Strength number
celeste.BloomRenderer = {}

--#region Dialog
---@class Dialog
celeste.Dialog = {}

---Get the Dialog given its name
---@param name string The name of the dialog to get.
---@return string
function celeste.Dialog.Get(name) end
--#endregion

---@class EntityData
---@field ID integer
---@field Level Level
---@field Position Vector2
---@field Width integer
---@field Height integer
---@field Values Dictionary<string, any>
celeste.EntityData = {}

---@class EntityID
---@overload fun(level: string, id: integer): EntityID
celeste.EntityID = {}

---@class Key : Entity
---@field ID EntityID
---@overload fun(player: Player, id: EntityID): Key
celeste.Key = {}

--#region Level
---@class Level : Scene
---@field Session Session
---@field Bounds Rectangle
---@field CameraOffset Vector2
---@field LevelOffset Vector2
---@field Bloom BloomRenderer
---@field CoreMode CoreModes
---@field Camera Camera
---@field CanRetry boolean
---@field PauseLock boolean
---@field InCutscene boolean
celeste.Level = {}

---Get the Spawnpoint closest to the target.
---@param at Vector2 Where to search a Spawnpoint.
---@return Vector2
function celeste.Level:GetSpawnPoint(at) end

---Complete the current area/level
---@param spotlightWipe? boolean
---@param skipScreenWipe? boolean
---@param skipCompleteScreen? boolean
---@return userdata Screenwipe
function celeste.Level:CompleteArea(spotlightWipe, skipScreenWipe, skipCompleteScreen) end

---Start a screen shake.
---@param time? number The duration of the shake.
function celeste.Level:Shake(time) end

---Start a screen shake with a direction.
---@param dir Vector2
---@param time? number The duration of the shake.
function celeste.Level:DirectionalShake(dir, time) end

---Instantly change to the next colorgrade.
---@param next string The next colorgrade.
function celeste.Level:SnapColorGrade(next) end

---Change to the other colorgrade over the timer.
---@param next string The next colorgrade
---@param time? number The time to make the change over.
function celeste.Level:NextColorGrade(next, time) end

---Cancel the current Cutscene.
function celeste.Level:CancelCutscene() end
--#endregion

---@class LevelData
---@field Bounds Rectangle
celeste.LevelData = {}

---@class LevelLoader : Scene
---@overload fun(session: Session, respawn: Vector2): LevelLoader
celeste.LevelLoader = {}

--#region MapData
---@class MapData
celeste.MapData = {}

---Get the LevelData at the Offset.
---@param at Vector2 Offset to get the data from.
---@return LevelData
function celeste.MapData:GetAt(at) end
--#endregion

---@class MiniTextbox : Entity
---@overload fun(dialogId: string): MiniTextbox
celeste.MiniTextbox = {}

--#region Player
---@class Player : Actor
---@field StateMachine StateMachine
---@field Dead boolean
---@field DummyFriction boolean
---@field DummyAutoAnimate boolean
---@field AutoJump boolean
---@field AutoJumpTimer number
---@field Speed Vector2
---@field Sprite Sprite
celeste.Player = {}

---Make the player Die
---@param dir Vector2
---@param evenIfInvincible? boolean
---@param registerDeathInStats? boolean
---@return userdata deadBody
function celeste.Player:Die(dir, evenIfInvincible, registerDeathInStats) end

---Check if the Player is touching ground.
---@param at? number Vertical offset to check at.
---@return boolean
function celeste.Player:OnGround(at) end

---Make the player walk in Dummy state.
---@param x number The target x.
---@param walkBackwards? boolean
---@param speedMultiplier? number
---@param keepWalkingIntoWalls? boolean
---@return IEnumerator
function celeste.Player:DummyWalkTo(x, walkBackwards, speedMultiplier, keepWalkingIntoWalls) end

---Make the player run in Dummy state.
---@param x number The target x.
---@param fastAnimation? boolean
---@return IEnumerator
function celeste.Player:DummyRunTo(x, fastAnimation) end

---Make the player jump.
---@param particles? boolean Whether to display dust particles.
---@param playSfx? boolean Whether to play an SFX.
function celeste.Player:Jump(particles, playSfx) end

---Start the Player's StartFly state
function celeste.Player:StartStarFly() end

---Start the Player's CassetteFly state.
---@param target Vector2 The target end position.
---@param control Vector2 The flying arc middle point.
function celeste.Player:StartCassetteFly(target, control) end

---@enum IntroTypes
celeste.Player.IntroTypes = {}
--#endregion

---@class PlayerInventory
celeste.PlayerInventory = {}

--#region Postcard
---@class Postcard : Entity
---@overload fun(msg: string, sfxIn: string, sfxOut: string): Postcard
---@overload fun(msg: string, area: integer): Postcard
celeste.Postcard = {}

---Called Before Render
function celeste.Postcard:BeforeRender() end

---The Routine to display the Postcart.
---@return IEnumerator
function celeste.Postcard:DisplayRoutine() end
--#endregion

--#region Session
---@class Session
---@field Level string
---@field RespawnPoint Vector2
---@field LevelData LevelData
---@field MapData MapData
---@field DeathsInCurrentLevel integer
---@field Audio AudioState
---@field HitCheckpoint boolean
---@field Inventory PlayerInventory
---@field Keys HashSet<EntityID>
---@field LevelFlags HashSet<string>
celeste.Session = {}

---Update the Level's Starting Dashes
function celeste.Session:UpdateLevelStartDashes() end

---Get the session flag value
---@param flag string
---@return boolean
function celeste.Session:GetFlag(flag) end

---Set the session flag value.
---@param flag string
---@param value boolean
function celeste.Session:SetFlag(flag, value) end

---Get the Level Flag value
---@param flag string
---@return boolean
function celeste.Session:GetLevelFlag(flag) end

---@enum CoreModes
celeste.Session.CoreModes = {}
--#endregion

--#region SFX
---@class SFX
celeste.SFX = {}

---Get an event name by its handle.
---@param handle string
---@return string # The event name, or handle if not found.
function celeste.SFX.EventnameByHandle(handle) end
--#endregion

--#region SoundSource
---@class SoundSource : Component
---@overload fun(): SoundSource
celeste.SoundSource = {}

---Play the given event on the SoundSource.
---@param event string The event to play.
---@return SoundSource self
function celeste.SoundSource:Play(event) end
--#endregion

--#region Textbox
---@class Textbox
celeste.Textbox = {}

---Display a Textbox with the given dialog
---@param dialog string The dialog or dialogId to show
---@return IEnumerator # The display routine
function celeste.Textbox.Say(dialog) end
--#endregion

---@class WindController: Entity
---@overload fun(patterns: userdata): WindController
celeste.WindController = {}

return celeste