# Bosses Helper

This is a helper specifically designed to allow the creation of custom bosses within a Celeste map, fully customizable through input in the map editor or utilizing user-provided .xml and .lua files to execute attacks, cutscenes, and more.

## Setup

The Boss Controller Entity has multiple entries that will enable the Boss to work, as well as a few optional entries.

### Basic Setup Entries

These entries are all related to the basic appeareance of the Boss and its interaction with the game engine, incliding gravity and the Player.

- Boss Sprite: The name of the sprite the boss will use. Works the same way as you'd set the sprite of a custom NPC.
- Boss Health Max: This is the amount of health points the Boss will start with. Not all Bosses have to work on Health, however.
- Hurt Mode: This will determine how the Player can collide with the Boss to trigger a user-defined method to declare what happens, but it's usually lowering the health and start attacking again.
  - a
- Boss Hit Cooldown: This determines for how long the Boss is invulnerable to being collidable by the Player. While the collision will still happen, the method designed will not trigger.
- Max Fall: This determines the max fall speed of the Boss. The rate the boss reaches this speed is relative to the gravity multiplier. This speed can still be surpassed manually with speed Tweens or direct speed setting during attacks or cutscenes.
- Base Gravity Multiplier: This will set the Gravity multiplier to the given value. The gravity constant used is 900, same as the Player.

### Custom File Entries

These entries are those that require Lua code files or XML format files, as instructed, for the reasons described after each one. This is where most of the Boss custom logic is held within. All paths provided are relative to your mod's directory. Providing the extension of the file is not required, but including it will not break the mod unless the extension is invalid, but that would break the mod anyways.

To know how to set up each specific file, check out the [File Formats section](#file-formats) further into this markdown.

- Hitbox Metadata Path: This XML file will hold all data pertaining to the hitboxes and hurboxes the Boss will use. ([Format](#hitbox-metadata))
- Attacks Path: This path should point to a directory that holds all the Lua files that will be used in Attacks. ([Format](#attacks))
- Events Path: This path should point to a directory that holds any Lua files that will be used as Cutscenes, or Events. ([Format](#events))
- Patterns Path: This XML file will hold all the data pertaining to Attack Patterns, which are ordered - or random - sets of calls to the Lua files in Attacks or Events. ([Format](#events))
- Functions Path: This Lua file is used to define any custom logic the Boss uses, like collision logic, or adding any component or coroutines before the Scene starts. ([Format](#functions))

### Other Entries

These entries are more miscelaneous and not fully necessary for all Bosses, but can provide some additional logic to the Boss Fight.

- Dynamic Facing: This bool will determine if the Boss's sprite should flip horizontally when the player is on the opposide side, as to avoid the Boss's Back facing the player.
- Mirror Sprite: This will mirror the Boss's Sprite horizontally.
- Kill On Contact: This will make it so that the player will Die if they come in contact with the Boss's Hitbox.
- Start Attacking Immediately: Normally, the Boss will start attacking once the player moves. Marking this true will start attacking as soon as the scene starts.
- Sidekick Cooldown: Only applicable to the Sidekick Attack Hurt Mode. Determines the time Badeline will be unable to shoot another laser.
- Sidekick Freeze: As Badeline is marked as a Follower, if marked false, she'll continue to follow the player even while aiming her laser. Marking this true will stop her in place while she shoots instead.

## File Formats

Some of the files needed for the bosses require a somewhat specific format, especially in the case of the XML files. The specific name of the files is mostly irrelevant as long as you provide the correct paths.

### Hitbox Metadata

### Patterns

### Attacks

### Events

### Functions

## Disclaimer

Currently I have referenced and used code from Ricky06's Conqueror's Peak, Cruor's LuaCutscenes, and IsaGoodFriend's BadelineFollower, all of which either have their code under license or have explicitly allowed me to do code reference and copying. All other code is original or from Vanilla Celeste.
