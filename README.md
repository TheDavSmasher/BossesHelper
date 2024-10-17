# Bosses Helper

This is a helper specifically designed to allow the creation of custom bosses within a Celeste map, fully customizable through input in the map editor or utilizing user-provided .xml and .lua files to execute attacks, cutscenes, and more.

## Boss Setup

The Boss Controller Entity has multiple entries that will enable the Boss to work, as well as a few optional entries.

### Basic Setup Entries

These entries are all related to the basic appeareance of the Boss and its interaction with the game engine, incliding gravity and the Player.

- Boss Sprite: The name of the sprite the boss will use. Works the same way as you'd set the sprite of a custom NPC.
- Boss Health Max: This is the amount of health points the Boss will start with. Not all Bosses have to work on Health, however.
- Hurt Mode: This will determine how the Player can collide with the Boss to trigger a user-defined method to declare what happens, but it's usually lowering the health and start attacking again.
  - Player Contact: The Boss will activate collision as soon as the player enters in contact with it.
  - Player Dash: The Boss will active collision when the Player's Dash is active (more specifically while their Dash Attack is active).
  - Head Bounce: The Boss will have a hitbox on top similar to that of Oshiro Boss, activating its collision the same way.
  - Sidekick Attack: Custom addition from this helper. The player will have a Badeline Dummy Follower, which can be activated with a custom bind so she begins targetting the nearest Boss (with the Sidekick Attack hurt mode) and shoots a laser towards it, and then enters cooldown.
  - Custom: The Boss will start with no hurtbox logic and it will entirely depend on the user's code in the Functions Lua file or other Lua files.
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
- Sidekick Freeze: Only applicable to the Sidekick Attack Hurt Mode. As Badeline is marked as a Follower, if marked false, she'll continue to follow the player even while aiming her laser. Marking this true will stop her in place while she shoots instead.

## File Formats

Some of the files needed for the bosses require a somewhat specific format, especially in the case of the XML files. The specific name of the files is mostly irrelevant as long as you provide the correct paths.

### Hitbox Metadata

This XML file uses the format of the following example:

```xml
<HitboxMetadata>
    <Hitboxes tag="main">
        <Hitbox width="8" height="8" xOffset="0" yOffset="0"/>
        <Circle radius="16" xOffset="0"/>
    </Hitboxes>

    <Hitboxes tag="unused">
        <Hitbox width="8" height="8" xOffset="0" yOffset="0"/>
        <Circle radius="16" xOffset="0"/>
    </Hitboxes>
    <!--Optionally more Sets-->

    <Hurtboxes>
        <Hitbox width="4" height="4" xOffset="2" yOffset="2"/>
        <Circle radius="12" xOffset="0" yOffset="0"/>
    </Hurtboxes>

    <Hurtboxes tag="other">
        <Hitbox width="4" height="4"/>
        <Circle radius="12" yOffset="2"/>
    </Hurtboxes>
    <!--Optionally more Sets-->

    <Bouncebox width="8" height="6" xOffset="0" yOffset="-16"/>
    <Target radius="4" xOffset="10" yOffset="-2"/>
</HitboxMetadata>
```

The entirety of the contents are inside the `HitboxMetadata` node. There can be multiple sets of `Hitboxes` and `Hurtboxes` nodes, but there can only be one `Bouncebox` and one Badeline Sidekick `Target`.

`Hitboxes` are used for collisions with solids and will hold the KillOnContact Player Collider if marked. `Hurtboxes` are used specifically for the Boss being hit and applying the correct collision logic. The `Bouncebox` is specifically only used if the Hurt Mode is set to `Head Bounce`, and is the hitbox that must be collided with to bounce. `Target` is a Circle hitbox that Badeline Sidekick will track and aim towards whenever it tries to shoot. It will collide only with Badeline Sidekick's Lasers and react accordingly.

`Hitboxes` and `Hurtboxes` alike can have multiple hitboxes in the same set, comprised of Rectangular Hitboxes or Circle Hitboxes, marked with `Hitbox` and `Circle` respectively. Each set of `Hitboxes` or `Hurtboxes` can have a tag to differentiate them. If no tag is given, it will use the default tag "main" and will be hitbox set to be used when the Boss loads in by default. All sets are stored in a Dictionary so make sure each set, if multiple, had unique tags. `Hitboxes` and `Hurtboxes` are stored in separate Dictionaries so duplicate tags between a set of Hitboxes and a set of Hurtboxes is acceptable. Any number of sets can be defined, even none at all.

Each Rectangular Hitbox has up to 4 attributes: `width`, `height`, `xOffset`, and `yOffset`. Each Circle Hitbox has up to 3 attributes: `radius`, `xOffset`, and `yOffset`. If a value isn't given, `width` and `height` will default to 8 (1 tile), `radius` defaults to 4 (1 tile diameter), and `xOffset` and `yOffset` will default to 0. If `Bouncebox` is not provided with `height`, it will default to 6 instead of 8.

If no `Hitboxes` node is provided, the Boss will use a default Hitbox of the dimensions and position of the Boss's Sprite, aligned with the sprite's position. Same applies for `Hurtboxes`. If no `Bouncebox` node is provided, it will use a default Hitbox of the same width of the Boss's Sprite, height of 6, and aligned to the top of the sprite. If no `Target` node is provided, the Boss will use a default Circle of radius 4 and offset 0. If no file is provided, the Boss will use all of the previously mentioned default hitboxes.

### Patterns

This XML file uses the format of the following example:

```xml
<Patterns>
    <!--Deterministic Looping Pattern-->
    <Pattern>
        <Wait time="2"/>
        <Attack file="third">
        <Wait time="2"/>
        <Attack file="first"/>
    </Pattern>

    <!--Deterministic Looping Pattern with Pre-Loop Actions-->
    <Pattern>
        <Wait time="2"/>
        <Event file="start"/>
        <Loop/>
        <Wait time="2"/>
        <Attack file="first"/>
    </Pattern>

    <!--Deterministic Limited Loops Pattern-->
    <Pattern repeat="4" goto="3">
        <Attack file="fourth"/>
        <Wait time="4"/>
        <Attack file="first"/>
        <Wait time="4"/>
        <Event file="scream"/>
        <Wait time="4"/>
    </Pattern>
    
    <Pattern goto="0">
        <Wait time="1"/>
        <Attack file="second"/>
        <Wait time="2.6"/>
    </Pattern>

    <!--Deterministic Looping Until Player is in a Given Position Pattern-->
    <Pattern x="20" y="20" width="100" height="100" goto="1">
        <Attack file="first"/>
        <Wait time="3.6"/>
        <Attack file="third"/>
        <Wait time="4.5"/>
    </Pattern>

    <!--Random order Pattern-->
    <Random>
        <Attack file="first" wait="2"/>
        <Attack file="second" wait="1"/>
        <Attack file="third" wait="2"/>
        <Attack file="fourth" wait="0.6"/>
    </Random>
</Patterns>
```

The entirety of the contents are inside the `Patterns` node. There can be as many `Pattern` or `Random` nodes inside, but there must be at least 1 in general of either kind. All nodes in the file are stored in the same order as they are in the file, and are indexed as such as well, inside an index-0 array (meaning that the first Pattern provided is index 0).

`Pattern` nodes delimits any deterministic set of Actions that will always be performed in the same order. In contrast, `Random` nodes deilimit any set of Actions that will have no set order, and therefore has a different format. Any Pattern, be deterministic or random can be manually interrupted within the Boss's Collision logic functions.

`Pattern` nodes can have different attributes to make them end in different ways.

- If no attributes are defined, then the Pattern will loop indefinitely unless manually interrupted when the Boss is collided with.
- If a `goto` attribute is provided, the Pattern will then go to the indicated Pattern with the matching index when the Pattern ends.
- If a `repeat` attribute is provided alongside a `goto` attribute, the Pattern will loop however many times as specified in repeat. A value of 0 will run the Pattern once from top to bottom and then go to the given pattern. A value of 1 will execute twice and then run. It is defined as how many *additional* loops will run until it ends. Only providing `goto` with no `repeat` is the same as providing `repeat` with value 0.
- Alternatively, alongside a `goto` attribute, attributes `x`, `y`, `width`, and `height` can be provided. These attributes will delimit a rectangle at a given position. Whenever the Player is inside the given rectangle, it will go to the given pattern once the current action ends.

`Random` nodes take no parameters.

`Pattern` nodes can have any number of nodes inside them.

- `Wait` nodes signify the Boss will not do anything during the specified `time` attribute.
- `Attack` nodes signify the Boss will perform the attack found inside the file mathcing the `file` attribute.
- `Event` nodes signify a Cutscene will execute as found inside the file matching the `file` attribute.
- `Loop` nodes can only be used once per `Pattern` node, and they delimit the Pattern's loop. If a `Loop` node is found inside a `Pattern` node, everything above it will execute once, when the pattern begins, and everything below it will execute with whatever loop logic is provided in the `Pattern` node attributes.
  - If more than one `Loop` node exists inside the same `Pattern` node, only everything after the last one will loop and only everything before the last one (but after the previous one) will execute before the loop.

The `file` attribute of both `Attack` and `Event` nodes **must not** contain the `.lua` extension.

`Random` nodes can also have any number of nodes inside them.

- `Attack` nodes signify the Boss will perform the attack found inside the file mathcing the `file` attribute, and then will do nothing during the time provided in the `wait` attribute.
- `Event` nodes signify a Cutscene will execute as found inside the file matching the `file` attribute, and then will do nothing during the time provided in the `wait` attribute.

`Random` nodes take no `Wait` nodes because otherwise attacks would execute back to back with no pause in between them. Therefore, each node inside this one has to provide their own post-execution wait time.

This file is mandatory and at least one node must exist within the parent node.

### Attacks

Every Lua file inside the Attacks subdirectory should use the following format:

```lua
function onBegin()
    --Your code here
end
```

An `onBegin()` function must be provided, which holds the code the attack will execute. Given that it's also its own files, any number of local functions can be defined and used within the function.

### Events

### Functions

## Health System

## Disclaimer

Currently I have referenced and used code from Ricky06's Conqueror's Peak, Cruor's LuaCutscenes, and IsaGoodFriend's BadelineFollower, all of which either have their code under license or have explicitly allowed me to do code reference and copying. All other code is original or from Vanilla Celeste.
