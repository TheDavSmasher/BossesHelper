# Bosses Helper

**Current Version**: `1.1.1`

---

This is a helper specifically designed to allow the creation of custom bosses within a Celeste map, fully customizable through input in the map editor or utilizing user-provided .xml and .lua files to execute attacks, cutscenes, and more.

## Layout

- [Boss Setup](#boss-setup)
  - [Basic Setup Entries](#basic-setup-entries)
  - [Custom File Entries](#custom-file-entries)
  - [Other Entries](#other-entries)
- [Lua Helper Functions](#lua-helper-functions)
- [File Formats](#file-formats)
  - [Hitbox Metadata](#hitbox-metadata)
  - [Patterns](#patterns)
  - [Attacks](#attacks)
  - [Events](#events)
  - [Functions](#functions)
- [Helper Objects and Components](#helper-objects-and-components)
- [Boss Health Bar](#boss-health-bar)
  - [Countdown](#countdown)
  - [Health Icons](#health-icons)
  - [Shrinking Bar](#shrinking-bar)
- [Player Health System](#player-health-system)
  - [Setup Entries](#setup-entries)
  - [Health Bar Entries](#health-bar-entries)
  - [Health System Triggers](#health-system-triggers)
- [TASing](#tasing)
- [Disclaimer](#disclaimer)

## Boss Setup

The Boss Controller Entity has multiple entries that will enable the Boss to work, as well as a few optional entries. It is important to note that the code for the Boss is separated into two main classes: the Controller and the Puppet. The Controller will run all logic regarding attacks, events, tracking health, and similar. The Puppet handles all hitboxes, collisions, and sprite logic. Due to this, no reference to the Controller is given to the user at any time, and any methods to interact with it are given through delegates or helper functions.

### Basic Setup Entries

These entries are all related to the basic appearance of the Boss and its interaction with the game engine, including gravity and the Player.

- Boss ID: A unique ID to identify a specific Boss instance from Lua.
  - While not enforced to be unique, it's useful if multiple files are used by multiple Boss instances as to differentiate between them.
- Boss Sprite: The name of the sprite the boss will use. Works the same way as you'd set the sprite of a custom NPC.
- Boss Health Max: This is the amount of health points the Boss will start with. Not all Bosses have to work on Health, however.
- Hurt Mode: This will determine how the Player can collide with the Boss to trigger a user-defined method to declare what happens, but it's usually lowering the health and start attacking again.
  - Player Contact: The Boss will activate collision as soon as the player enters into contact with it.
  - Player Dash: The Boss will activate collision when the Player's Dash is active (more specifically while their Dash Attack is active).
  - Head Bounce: The Boss will have a hitbox on top similar to that of Oshiro Boss, activating its collision the same way.
  - Sidekick Attack: Custom addition from this helper. The player will have a Badeline Dummy Follower, which can be activated with a custom bind so she begins targetting the nearest Boss (with the Sidekick Attack hurt mode) and shoots a laser towards it, and then enters cooldown.
  - Custom: The Boss will start with no hurtbox logic and it will entirely depend on the user's code in the Functions Lua file or other Lua files. This means that if no `setup()` function can be found inside of the Functions file, nothing will happen on collision.
    - It is important to note that all Hurt Modes will still execute the `setup()` method found in this file if provided. The main difference is that Custom provides _nothing_ but the code within `setup()`, whereas the other Hurt Modes will include their own logic _on top_ of anything provided inside `setup()`.
- Boss Hit Cooldown: This determines for how long the Boss is invulnerable to being collidable by the Player. While the collision will still happen, the method designed will not trigger.
- Max Fall: This determines the max fall speed of the Boss. The rate the boss reaches this speed is relative to the gravity multiplier. This speed can still be surpassed manually with speed Tweens or direct speed settings during attacks or cutscenes.
- Base Gravity Multiplier: This will set the Gravity multiplier to the given value. The gravity constant used is 900, the same as the Player.

### Custom File Entries

These entries are those that require Lua code files or XML format files, as instructed, for the reasons described after each one. This is where most of the Boss custom logic is held within. All paths provided are relative to your mod's directory. Providing the extension of the file is not required, but including it will not break the mod unless the extension is invalid, but that would break the mod anyway.

To learn how to set up each specific file, check out the [File Formats section](#file-formats) further in this markdown.

- Hitbox Metadata Path: This XML file will hold all data pertaining to the hitboxes and hurboxes the Boss will use. ([Format](#hitbox-metadata))
- Attacks Path: This path should point to a directory that holds all the Lua files that will be used in Attacks. ([Format](#attacks))
- Events Path: This path should point to a directory that holds any Lua files that will be used as Cutscenes, or Events. ([Format](#events))
- Patterns Path: This XML file will hold all the data pertaining to Attack Patterns, which are ordered - or random - sets of calls to the Lua files in Attacks or Events. ([Format](#events))
- Functions Path: This Lua file is used to define any custom logic the Boss uses, like collision logic, or adding any component or coroutines before the Scene starts. ([Format](#functions))

### Other Entries

These entries are more miscellaneous and not fully necessary for all Bosses but can provide some additional logic to the Boss Fight.

- Dynamic Facing: This bool will determine if the Boss's sprite should flip horizontally when the player is on the opposite side, to avoid the Boss's back facing the player.
- Mirror Sprite: This will mirror the Boss's Sprite horizontally.
- Kill On Contact: This will make it so that the player will Die if they come in contact with the Boss's Hitbox.
- Start Attacking Immediately: Normally, the Boss will start attacking once the player moves. Marking this true will start attacking as soon as the scene starts.
- Sidekick Cooldown: Only applicable to the Sidekick Attack Hurt Mode. Determines the time Badeline will be unable to shoot another laser.
- Sidekick Freeze: Only applicable to the Sidekick Attack Hurt Mode. As Badeline is marked as a Follower, if marked false, she'll continue to follow the player even while aiming her laser. Marking this true will stop her in place while she shoots instead.

## Lua Helper Functions

Thoughout the document, there is a mention of helper functions provided to the Lua environment used to run all attacks, events, and setup. It uses all [helper functions pre-existing](https://maddie480.ovh/lua-cutscenes-documentation/modules/helper_functions.html) from the Lua Cutscenes mod made by Cruor. [Additional helper functions](boss_helper_functions.md) specific to this mod have also been added.

A [small layout](boss_helper_functions_layout.md) with hyperlinks to each of these functions is also provided.

## File Formats

Some of the files needed for the bosses require a somewhat specific format, especially in the case of XML files. The specific name of the files is mostly irrelevant as long as you provide the correct paths.

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
    <!--Optionally more Sets (...)-->

    <Hurtboxes>
        <Hitbox width="4" height="4" xOffset="2" yOffset="2"/>
        <Circle radius="12" xOffset="0" yOffset="0"/>
    </Hurtboxes>

    <Hurtboxes tag="other">
        <Hitbox width="4" height="4"/>
        <Circle radius="12" yOffset="2"/>
    </Hurtboxes>
    <!--(...)-->

    <Bouncebox width="8" height="6" xOffset="0" yOffset="-16"/>
    <Bouncebox width="8" height="6" xOffset="0"/>
    <Bouncebox tag="main" width="8" height="6" xOffset="0"/>

    <Bouncebox tag="secondary" width="8" height="6"/>
    <Bouncebox tag="secondary" width="8" height="6" xOffset="10" yOffset="-16"/>
    <!--(...)-->

    <Target radius="4" xOffset="10" yOffset="-2"/>
    <Target tag="main" radius="4" xOffset="10" yOffset="-2"/>
    <Target tag="main" radius="4"/>

    <Target tag="other" radius="4" xOffset="10" yOffset="-2"/>
    <!--(...)-->
</HitboxMetadata>
```

The entirety of the contents are inside the `HitboxMetadata` node. There can be multiple sets of any of the `Hitboxes`, `Hurtboxes`, `Bouncebox`, and Badeline Sidekick `Target` nodes, or none at all.

`Hitboxes` are used for collisions with solids and will hold the KillOnContact Player Collider if marked. `Hurtboxes` are used specifically for the Boss being hit and applying the correct collision logic. `Bouncebox` is specifically only used if the Hurt Mode is set to `Head Bounce`, and is the hitbox or hitbox group that must be collided with to bounce. `Target` is a Circle hitbox or hitbox group that Badeline Sidekick will track and aim towards whenever it tries to shoot. It will collide only with Badeline Sidekick's Lasers and react accordingly. If there exists more than one Sidekick Target, Badeline will target the closest one.

`Hitboxes` and `Hurtboxes` alike can have multiple hitboxes in the same set, comprised of Rectangular Hitboxes or Circle Hitboxes, marked with `Hitbox` and `Circle` respectively. Each set of `Hitboxes` or `Hurtboxes` can have a tag to differentiate them. If no tag is given, it will use the default tag "main" and will be hitbox set to be used when the Boss loads in by default. Likewise, a set tagged "main" will be used initially. All sets are stored in a Dictionary so make sure each set, if multiple, has unique tags. `Hitboxes` and `Hurtboxes` are stored in separate Dictionaries so duplicate tags between a set of Hitboxes and a set of Hurtboxes are acceptable. Any number of sets can be defined, even none at all.

Each Rectangular Hitbox has up to 4 attributes: `width`, `height`, `xOffset`, and `yOffset`. Each Circle Hitbox has up to 3 attributes: `radius`, `xOffset`, and `yOffset`. If a value isn't given, `width` and `height` will default to 8 (1 tile), `radius` will default to 4 (1 tile diameter), and `xOffset` and `yOffset` will default to 0. If `Bouncebox` is not provided with `height`, it will default to 6 instead of 8.

`Bouncebox` and `Target` behave somewhat similarly, but due to the fact that they are restricted to Rectangular hitboxes and Circle colliders respectively, they are formatted somewhat differently. Instead of having a Parent node to delimit a set, any number of `Bouncebox` or `Target` nodes that share the same tag (no tag is equivalent to `tag="main"`) will be considered as part of the same set. `Bouncebox` attributes are the same as those of a Rectangular Hitbox, and `Target` attributes are the same as those of a Circle Hitbox. All `Bouncebox` and `Target` hitboxes are also stored in their respective dictionaries, so shared tags between different hitbox node types are accepted.

The Tags for each set of Hitboxes are useful for switching between them with designated helper functions, such as [`changeBaseHitboxTo()`](boss_helper_functions.md#helperschangebasehitboxto-tag). Each of the 4 Hitbox Type sets (Hitbox, Hurtbox, Bouncebox, Target) has its own method, as each one is stored in separate dictionaries.

If no `Hitboxes` node is provided, the Boss will use a default Hitbox of the dimensions and position of the Boss's Sprite, aligned with the sprite's position. The same applies to `Hurtboxes`. If no `Bouncebox` node is provided, it will use a default Hitbox of the same width as the Boss's Sprite, height of 6, and aligned to the top of the sprite. If no `Target` node is provided, the Boss will use a default Circle of radius 4 and offset 0. If no file is provided, the Boss will use all of the previously mentioned default hitboxes.

### Patterns

This XML file uses the format of the following example:

```xml
<Patterns>
    <!--The following patterns are only examples of how to set this up.
    Most have comments to detail certain aspects of them.
    Explanation is found below.-->

    <!--Deterministic Looping Pattern-->
    <Pattern> <!--This is pattern 0-->
        <Wait time="2"/>
        <Attack file="third"/>
        <Wait time="2"/>
        <Attack file="first"/>
    </Pattern>

    <!--Deterministic Looping Pattern with Pre-Loop Actions-->
    <Pattern> <!--This is pattern 1-->
        <Wait time="2"/>
        <Attack file="start"/>
        <Loop/>
        <Wait time="2"/>
        <Attack file="first"/>
    </Pattern>

    <!--Deterministic Limited Loops Pattern-->
    <Pattern repeat="4" goto="3"> <!--This is pattern 3-->
        <Attack file="fourth"/>
        <Wait time="4"/>
        <Attack file="first"/>
        <Wait time="4"/>
        <Attack file="scream"/>
        <Wait time="4"/>
    </Pattern>
    
    <Pattern goto="3">
        <Wait time="1"/>
        <Attack file="second"/>
        <Wait time="2.6"/>
    </Pattern>

    <Pattern repeat="3">
        <Attack file="bullets"/>
        <Wait time="2.6"/>
    </Pattern>

    <!--Deterministic Bounded Loops Pattern-->
    <Pattern minRepeat="2" repeat="4" goto="3">
        <Attack file="fourth"/>
        <Wait time="1"/>
        <Attack file="fifth"/>
        <Wait time="4"/>
    </Pattern>

    <!--Deterministic Looping Until Player is in a Given Position Pattern-->
    <Pattern width="100" height="100" x="20" y="20" goto="1">
        <Attack file="first"/>
        <Wait time="3.6"/>
        <Attack file="third"/>
        <Wait time="4.5"/>
    </Pattern>

    <Pattern width="50" height="50" goto="4">
        <Attack file="first"/>
        <Wait time="3.6"/>
        <Attack file="third"/>
        <Wait time="4.5"/>
    </Pattern>

    <!--Boss Event Cutscenes-->
    <Event file="initialScreech_M" goto="2">

    <Event file="middleScreech_A">

    <!--Random order Pattern-->
    <Random>
        <Attack file="first" wait="2"/>
        <Attack file="second" wait="1" weight="3"/>
        <Attack file="third" wait="2"/>
        <Attack file="fourth" wait="0.6" weight="2"/>
    </Random>

    <!--Random Bounded Attacks Pattern-->
    <Random minRepeat="3" repeat="10">
        <Attack file="first" wait="2"/>
        <Attack file="second" wait="1"/>
        <Attack file="third" wait="2"/>
        <Attack file="fourth" wait="0.6"/>
    </Random>

    <!--Random Looping Until Player is in a Given Position/Limited Attacks Pattern-->
    <Random width="20" height="100" x="20" repeat="7" goto="4">
        <Attack file="first" wait="2"/>
        <Attack file="second" wait="1"/>
        <Attack file="third" wait="2"/>
        <Attack file="fourth" wait="0.6"/>
    </Random>
</Patterns>
```

The entirety of the contents are inside the `Patterns` node. There can be as many `Pattern`, `Random`, or `Event` nodes inside, but there must be at least 1 in general of either kind. All nodes in the file are stored in the same order as they are in the file, and are indexed as such as well, inside an index-0 array (meaning that the first Pattern provided is index 0).

`Pattern` nodes delimit any deterministic set of Actions that will always be performed in the same order. In contrast, `Random` nodes delimit any set of Actions that will have no set order and therefore have a different format. Any Pattern, be it deterministic or random can be manually interrupted within the Boss's Collision logic functions. `Event` nodes are used to have actual cutscenes in the middle of a fight, between attack patterns.

`Pattern` nodes can have different attributes to make them end in different ways.

- If no attributes are defined, then the Pattern will loop indefinitely unless manually interrupted as dictated by the Boss's setup.
- If a `goto` attribute is provided, the Pattern will then go to the indicated Pattern with the matching index when the Pattern ends.
  - Not providing this attribute when any other is will make it so `goTo` pattern is the one directly below the current one, or the next one in order.
- If a `repeat` attribute is provided, the Pattern will loop however many times as specified in `repeat`. A value of 0 will run the Pattern once from top to bottom and then go to the given pattern. A value of 1 will execute twice and then run. It is defined as how many _additional_ loops will run until it ends.
  - Not providing this attribute with `goTo` provided is the same as providing `repeat` with value 0.
- If a `minRepeat` attribute is provided, it acts the same way as the `repeat` attribute does, except that once this attribute's value loop count is met, a random chance of the pattern ending at every loop exists.
  - If either `repeat` or `minRepeat` is not provided, it will default to the value of the one provided.
  - If both are provided, the Pattern will run at least `minRepeat` loops and at most `repeat` loops, with a 50% chance of ending at every loop end.
  - If `minRepeat` is larger than `repeat`, only the `minRepeat` will take effect.
- If attributes `width`, `height`, `x`, and `y` are provided, these attributes will delimit a rectangle at a given position. Whenever the Player is inside the given rectangle, the Pattern's loop will end.
  - The coordinates for the `x` and `y` attributes are room coordinates.
  - If either `width` or `height` are missing, the Hitbox will not be created.
  - If `x` or `y` are missing, they will default to 0.

`Random` nodes can take the exact same attributes as `Pattern` nodes, with some differences. The main difference is that `repeat` and `minRepeat` doesn't count the number of loops, but of individual attack execution, or attack nodes used.

`Event` nodes can take up to two attributes.

- A `file` attribute is required, and must match the name of a `.lua` file inside the Events subdirectory provided.
- A `goto` attribute may be provided if the Event should go to a specific pattern after the Cutscene ends.
  - Not providing this attribute will start the next available pattern after this one, much like `Pattern` or `Random` nodes.

`Pattern` nodes can have any number of the following node types inside them.

- `Wait` nodes signify the Boss will not do anything during the specified `time` attribute.
- `Attack` nodes signify the Boss will perform the attack found inside the file matching the `file` attribute.
- `Loop` nodes can only be used once per `Pattern` node, and they delimit the Pattern's loop. If a `Loop` node is found inside a `Pattern` node, everything above it will execute once, when the pattern begins, and everything below it will execute with whatever loop logic is provided in the `Pattern` node attributes.
  - If more than one `Loop` node exists inside the same `Pattern` node, only everything after the last one will loop and only everything before the last one (but after the previous one) will execute before the loop.
  - If no `Loop` node is provided, all actions inside the parent node will be part of the pattern execution loop. If one is provided, only everything after it will be part of the loop, and everything prior will only execute at the start. By adding a node, you're essentially moving where the `while (true)` statement to that line.

The `file` attribute of both `Attack` and `Event` nodes **must not** contain the `.lua` extension.

`Random` nodes can also have any number of the following node types inside them.

- `Attack` nodes signify the Boss will perform the attack found inside the file matching the `file` attribute, and then will do nothing during the time provided in the `wait` attribute. Both attributes are required.
  - A `weight` attribute can also be provided. If one is given, that number of copies of the same Attack node will be included in the list that the random selector chooses a node from. Higher weights will make a given node more likely to be chosen.
    - Not providing this attribute is the same as providing it with a value of 1.

`Random` nodes take no `Wait` nodes because otherwise attacks would execute back to back with no pause in between them, or execute multiple waits back to back. Therefore, each node inside this one has to provide its own post-execution wait time.

This file is mandatory and at least one node must exist within the parent node. Any node inside of this will naturally execute indefinitely, except for Events. No pattern will interrupt itself, unless it's by loop count or the player being in the specified region, of the Event ending. Even when a Pattern is interrupted, it must be restarted manually or go to a different pattern in the player collision logic.

### Attacks

Every Lua file inside the Attacks subdirectory should use the following format:

```lua
function onBegin()
    --Your code here
end
```

An `onBegin()` function must be provided, which holds the code the attack will execute. Given that it's also its own files, any number of local functions can be defined and used within the function. Each file is provided with a reference to the `player`, the Boss's ID under `bossID`, the Boss's `puppet`, and multiple controller delegate functions under `boss`, as well as access to all regular helper functions.

### Events

Every Lua file inside the Events subdirectory should use the following format:

```lua
function onBegin()
    --Your code here
end

function onEnd(level, wasSkipped) --optional
    --Your code here
end
```

An `onBegin()` function must be provided, which holds the code the cutscene will execute. Given that it's also its own files, any number of local functions can be defined and used within the function. Each file is provided with a reference to the `player`, the Boss's ID under `bossID`, the Boss's `puppet`, the Event file itself under `cutsceneEntity`, a couple of Delegate functions in `boss`, and access to all regular helper functions. An `onEnd(level, wasSkipped)` function is not required but is recommended for handling cleanup and cutscene skipping logic. These files follow the same rule as the LuaCutscenes helper from Cruor. Events, like Cutscenes, are Skippable by default.

### Functions

This Lua file should follow the following format:

```lua
--- All of these functions are optional by definition, but make sure to include the ones that are necessary for the specific boss.

function onContact()
    --Your code here
end

function onDash()
    --Your code here
end

function onBounce()
    --Your code here
end

function onLaser()
    --Your code here
end

function onHit()
    --Your code here
end

function setup()
    --Your code here
end
```

This file contains all code that will execute arbitrarily to the Boss either at the start of the scene or when collided with.

The functions `onContact()`, `onDasH()`, `onBounce()`, and `onLaser()` will each execute separately when the Boss's Hurtbox is collided with, depending on the Hurt Mode: `onContact()` for Player Contact, `onDasH()` for Player Dash, `onBounce()` for Head Bounce, and `onLaser()` for Sidekick Attack, respectively. The function `onHit()` is a more generalized function and will be called if no specific method is provided. For example, if Hurt Mode Player Dash is used and the given file has no `onDash()` function, it will call `onHit()` instead. If no `onHit()` function is provided there either, then no code will be executed on collision with the Boss.

These collision functions are essential to fight logic, as it is where the user must specify if the Boss takes damage if the current pattern should be interrupted, if it should wait for the current attack to end, and if a new attack pattern should start. Lua helper functions are provided for each of these necessities through delegates.

The `setup()` function will be called during load time **before the scene starts**. (If any function is called directly within the `setup()` function that requires the scene, this wil fail. Either do so in an Attack/Event or in a function added with [`addConstantBackgroundCoroutine()`](boss_helper_functions.md#helpersaddconstantbackgroundcoroutine-func-).) It can be used to give the Boss additional components, sprites, or starting values. This function is not necessary. If the Hurt Mode was set to Custom, it's highly encouraged to add this file, since otherwise the Boss cannot be hurt. This function will still be called regardless of Hurt Mode, though.

All functions in this file are provided with a reference to the `player`, the Boss's ID under `bossID`, the Boss's `puppet`, and multiple controller delegate functions under `boss`, as well as access to all regular helper functions. The delegates provided here are different from the ones provided for Attacks, as explained in the Helper functions file.

## Helper Objects and Components

This Helper also adds a few Entities and Components for ease of use or just general usage.

Two accessible entities are included:

- Attack Actor: A generic Entity that subclasses the Actor class and has movement and collision logic.
  - Can be called from [`getNewBasicAttackEntity(params)`](boss_helper_functions.md#helpersgetnewbasicattackactor-position-hitboxes-spritename-gravmult1-maxfall90-startcollidabletrue-startsolidcollidabletrue-funconplayerkillplayer-xscale1-yscale1) helper function with these parameters.
- Attack Entity: A generic Entity that can be used for simple disjointed hitboxes.
  - Can be called from [`getNewBasicAttackActor(params)`](boss_helper_functions.md#helpersgetnewbasicattackentity-position-hitboxes-spritename-startcollidabletrue-funconplayerkillplayer-xscale1-yscale1) helper function with these parameters.

The entities returned by these functions must be manually added onto the scene.

Four Components are added with this helper for various usages.

- Entity Chain Component: Can be used to "chain" an entity to another one, so as the Entity this component is added to moves, so will the chained Entity, essentially moving as one.
  - One can be created with the [`getEntityChain()`](boss_helper_functions.md#helpersgetentitychain-entity-startchainedtrue-removefalse) helper function.
- Entity Flagger: A Component that will execute a function passed once the given session flag matches the state needed, and if the flag should be reset after used.
  - One can be created with the [`getEntityFlagger()`](boss_helper_functions.md#helpersgetentityflagger-flag-funchelpersdestroyentity-statetrue-resetflagtrue) helper function.
- Entity Timer: A Component that will execute a function passed once the timer runs to completion.
  - One can be created with the [`getEntityTimer()`](boss_helper_functions.md#helpersgetentitytimer-timer-funchelpersdestroyentity) helper function.
- Entity Checker: A Component that will execute the first function passed every frame until it's return value--which must be a boolean--matches the state needed, and if it should remove itself after such is the case.
  - One can be created with the [`getEntityChecker()`](boss_helper_functions.md#helpersgetentitychecker-checker-funchelpersdestroyentity-statetrue-removetrue) helper function.

All components returned by these functions must be added manually to the Entity that they will execute on.

A basic collider can be obtained with the [`getHitbox()`](boss_helper_functions.md#helpersgethitbox-width-height-x0-y0) or [`getCircle()`](boss_helper_functions.md#helpersgetcircle-radius-x0-y0) helper functions, which can be combined with the [`getColliderList()`](boss_helper_functions.md#helpersgetcolliderlist-) function. A basic vector2 object can be obtained with `vector2(x,y)`.

## Boss Health Bar

Due to the fact that not all Bosses will require or use Health Bars, a separate entity is included for those that do.

There are three different kind of Health "Bars" provided: a numerical Countdown, health point icons, or an actual bar (which can grow left, right, or be centered).

All Health Bar displays share the following entries:

- Health Bar X: The X coordinate the display will be on the screen. This is relative to the camera, and is limited by the game's resoultion.
- Health Bar Y: The Y coordinate the display will be on the screen. This is relative to the camera, and is limited by the game's resoultion.
- Health Scale X: The X value that will be applied to the display's scale.
- Health Scale Y: The Y value that will be applied to the display's scale.
- Bar Type: The type of the display that the entity will use:
  - Countdown: The health value will be represented as a number displayed on screen.
  - Health Icons: The health display will be represented with sprites provided, one per health point.
  - Bar: The Health Display will be represented by a rectangle that will shrink proportionally to its total length and the health value. The width and height of this rectangle are obtained from the Scale value.
    - Left: The Bar will grow to the Left and shrink back to the right. The Position determines it's right side.
    - Right: The Bar will grow to the Right and shrink back to the left. The Position determines it's left side.
    - Centered: The Bar will grow on both ends and shrink back to the center. The Position determines it's middle.
- Start Visible: Whether the Health Display will start Visible immidiately.

Based on the different Type placed for the Bar Type via the placements provided, extra entries are given.

### Countdown

- Base Color: The color the number display.

### Health Icons

- Health Icon: The sprite each Health Point icon should use.
- Health Icon Create Anim: The animation each of the icons' sprites should use when created.
- Health Icon Remove Anim: The animation each of the icons' sprites should use when a Health Point is lost.
- Health Icons Separation: How much distance is between each Health Icon. This distance is measured from any two Icon's left side, so this value should include the sprite's width if no overlap is desired.

### Shrinking Bar

- Base Color: The color the solid rectangle will use.

## Visibility Trigger

Due to the fact that a Health Bar can be added while not visible, a Trigger, Boss Health Bar Visible Trigger, is provided to change the Visibility of the Boss Health Bar closest to the trigger's Node.

## Player Health System

In addition to Bosses, this helper also implements a health System for the player, hooked to the Player's `Die` method. It is implemented with a generalized base for public use. When the Health System is added onto the scene, it starts disabled and must be activated in one of the provided ways explained below.

Due to the nature of the Health System and how it's managed, only one instance of a Health System can exist. If one is added after one already exists, it will simply override its values. Likewise, if a System exists and a new one is provided without any parameters, those values will be kept from the already existing Health System.

### Setup Entries

The following entries are provided in Loenn for setting up a Health System.

- Player Health: How many Health Points the player should have.
- Damage Cooldown: How much time must pass before the player is able to take damage again.
- Crush Effect: How the Health System should react when the player is crushed to death.
  - Push Out: The player will take damage and will be pushed out the nearest end of the crusher it can.
  - Solid on Invincible Player: The player will take damage and the crushing solid will become intangible while the Player remains inside of it.
  - Instant Death: The player will lose all Health Points they have and will die instantly, much like Vanilla would act without a Health System.
  - Keep Old Value: The Health System will keep using whatever its old crush effect was.
- Offscreen Effect: How the Health System should react when the player dies due to falling offstage.
  - Bounce Up: The player will take damage and will bounce, similar to what would happen if the Invincibility Assist was turned on.
    - While not fully encompassing with other mods, it still works with Gravity Helper's dying up offstage.
  - Bubble Back: The player will take damage and will be bubbled back to the last safe ground they were on.
    - The Bubble Back works similar to the Bubble of Cassettes or Keys.
    - Last Safe Ground is determined by the player's own `onSafeGround` value.
    - Loading into a level will always store the spawn point position as an initial Safe Ground, just in case there's none around and one is needed.
  - Instant Death: The player will lose all Health Points they have and will die instantly, much like Vanilla would act without a Health System.
  - Keep Old Value: The Health System will keep using whatever its old Offscreen effect was.
- Global Controller: If the Health System controller should stay as a global entity. If marked false, leaving the room or reloading the map in any way will remove it from the scene.
- Global Health: If the player should keep the same health values even across screens. Only really applicable if Global Controller is marked true. Marking this false will refill the Player's health up to the max value when a transition is hit, whereas false will keep the same health values.
- Activation Flag: The flag this Controller should wait for to be true to enable the Health System.
- Apply System Instantly: If the Controller should be active as soon as it is added onto the scene.
- Player Blink: If the player should "blink", representing a visual way to know if the player's damage cooldown is still active.
- Player Stagger: If the player's movement and control should be interrupted when taking damage as the player staggers back from the hit.
- On Damage Function: This takes a path to a Lua file, whose `onDamage()` method will execute every time the player takes damage.
  - Make sure your main code is inside the `function onDamage() ... end` block.

### Health Bar Entries

The Health System is accompanied by a Health Bar, which is the other half of the entries and only encompasses the visual side of the Health System.

- Health Icon: The sprite each Health Point icon should use.
- Health Icon Create Anim: The animation each of the icons' sprites should use when created.
- Health Icon Remove Anim: The animation each of the icons' sprites should use when a Health Point is lost.
- Health Icon Screen X: The X coordinates on the screen where the Health Bar will start.
- Health Icon Screen Y: The Y coordinates on the screen where the Health Bar will start.
- Health Icon Scale X: Each Health Icon's Sprite's X scale.
- Health Icon Scale Y: Each Health Icon's Sprite's Y scale.
- Health Icon Separation: How much distance is between each Health Icon. This distance is measured from any two Icon's left side, so this value should include the sprite's width if no overlap is desired.
- Start Visible: If the Health Bar should start being visible when added onto the scene.

### Health System Triggers

Alongside the Controller, the Health System comes with three triggers.

- Add Health System Trigger: This can be used to add an instance of a Health System, and works the exact same way as adding the Controller would, except it's delegated to be added onto the scene when entering the Trigger.
  - If a Health System already exists, the values provided will override those of the existing System.
  - If a specific value is not provided, the pre-existing value for that entry will be kept, similar to adding another Health System manually.
- Enable Health Trigger: This can be used to toggle the Enabled state of the existing Health System.
- Health Bar Visible Trigger: This can be used to toggle the Visible state of all the icons of the Health Bar.
- Recover Health Trigger: This can be used as a quick simple way to recover some hit points for the player. `Only Once` will remove the trigger once entered, and `Permanent` will make it so that the trigger doesn't load again.

### Miscellaneous Additions

Due to the fact that the Player's last Safe Ground is updated every frame the player is standing on safe ground, FlyBack from falling offscreen with the BubbleBack Offscreen mode will return at all times to that last point, regardless of where it is. A new entity is added, called `Update Safe Blocker`, which prevents the player's last safe ground from being updated. Changing spawnpoint will still, however, update this, because save points are considered universal safe ground for the purposes of this.

A `Update Safe Unblocker` is also added that will remove any active Blockers.

## Global Save Point

This helper now includes a Global Save Point entity alongside a Global Save Point Changer component. This Global Save Point functionality is similar to a save point in a Metroidvania or Souls-like game, where even in different rooms, upon death, will spawn back at the last saved spot.

The Entity containes the following entries:

- Respawn Type: The intro type the player should use when spawning back into the Save Point.
  - Use Old Value will make it so the same intro type as the previously saved-at Save Point will be used.
- Lua File: An Optional Lua file that can be provided for additional logic and functionality when interacting with the Save Point.
- Save Point Sprite: The sprite this Save Point should use.

The node provided with the placement will try to find the closest Spawn Point to it and use that spawn point. If there's no spawn point set within 10 tiles (80 px) of the node, the node itself will be used as the spawn point.

An additional entity and trigger are provided for more automatic save points.

- Auto Save Point Set: This entity acts the same way a Global Save Point would work, except the save point is set immediately upon being added upon the scene. This entity is useful for map start save points or points where save points should be set without the player's direct input.

- Save Point Set Trigger: This will set a save point in the same way an Auto Save Point Set trigger would, but whenever the player enters it instead of instantly when added onto a scene.

### Global Save Point Changer Component

Within this Component is where the main logic and data storage lies. As such, it is added as an export so any entity can become a Save Point.

This component, if used for entities that the mod being used on has full control over and can write code in them, can be used by simply creating one, adding it, and, when desired to be activated, such as being interacted with, the component's `Update` method (or setting `Active` to true) will trigger the Save Point Change. The logic is contained in the `Update` method so, even without having the type of the Component, it can still be activated. Also because of this, its state is naturally inactive, and calling `Update` will also inactivate it once done.

When this component is wanted for other, cross-mod entities, special steps must be taken. A second export is given, `CreateGlobalSavePointOnEntityOnMethod`. This export is responsible for creating the Component, adding it to the Entity passed, and creating an IL Hook on the entity's method specified. The purpose of the IL Hook is so any method call within the given entity can be used to activate this change. This hook only inserts a single call to a delegate that will call `Update` on the first Global Save Point Changer component the entity has, if any. This way, entities such as the Lobby benches or the Metroid inspired teleporters or any other entity can become a Global Save Point.

**Disclaimer**: Other bugs were fixed in the general helper and the IL Hook has not been tested fully. Expect this to be done by next update.

## Bosses Helper API

This Helper includes a few things exported with ModInterop, namely:

- **GetEntityChainComponent**: Returns an Entity Chain Component to keep two entities's positions tied together.
- **GetEntityTimerComponent**: Returns an Entity Timer Component to easily execute a function on the entity added once the timer runs to completion.
- **GetEntityFlaggerComponent**: Returns an Entity Flagger Component to easily execute a function on the entity added to once the flag state matches the needed state.
- **GetCurrentPlayerHealth**: Useful to know if a Health System is active on Scene and if so, what the Player's health value is at. Returns -1 if no Health System is active.
- **RecoverPlayerHealth**: Makes the player recover health points as many as the value given. This can go over the PLayer's default health value.
- **MakePlayerTakeDamage**: Useful to extend utility from the Health System to set custom parameters to take damage.
- **GetBossHealthTrackerComponent**: Returns a component that allows any entity that can track a number to use the Boss Health Bar entity display for it.
  - The integer can represent anything, and can be represented in multiple ways.
- **GetGlobalSavePointChangerComponent**: Returns a Global Save Point Changer Component.
  - If added to an entity your mod cannot edit, it **must** be added with the **CreateGlobalSavePointOnEntityOnMethod** export, so the IL Hook can be applied.
  - If added to an entity your mode is able to edit, to activate its Save Point Changing method, call the Component's `Update` method or make it `Active`. Each change or usage should do this. The component starts inactive, and calling Update will force it to be inactive. The logic is kept in `Update` so it can be called without having it be its specific type when handled.
- **CreateGlobalSavePointOnEntityOnMethod**: Creates and adds a Global Save Point Changer component on the entity provided, and creates an IL hook on that entity's method that will trigger the Save Point update.
  - As of current release, this IL hook has not been fully tested, but many other bugs have been fixed that an update release was needed.

The Mod Import Name is `"BossesHelper"`.

## TASing

Due to the high TAS nature of Celeste, the way RNG is handled for random patterns can be changed. RNG is always seeded, but what the seed is depends on what is desired. The normal way the Boss seeds its RNG is based on Active Time plus a crc32 hash of the Boss's Entity ID key. However, a command line command is provided with the helper: `set_boss_seed`. This command-line command will override the Active Time part of the RNG seed, still added to the crc32 hash of the Boss's Entity ID key, which is deterministic. If you've used this command and want to go back to the Active Time seeding method, use the command and set the seed to 0, which will make the Boss use Active Time once again.

## Disclaimer

Currently, I have referenced and used code from Ricky06's Conqueror's Peak, Cruor's LuaCutscenes, and IsaGoodFriend's BadelineFollower, all of which either have their code under license or have explicitly allowed me to do code reference and copying. All other code is original or from Vanilla Celeste.
