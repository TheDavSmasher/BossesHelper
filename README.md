# Bosses Helper

This is a helper specifically designed to allow the creation of custom bosses within a Celeste map, fully customizable through input in the map editor or utilizing user-provided .xml and .lua files to execute attacks, cutscenes, and more.

## Layout

- [Boss Setup](#boss-setup)
  - [Basic Setup Entries](#basic-setup-entries)
  - [Custom File Entries](#custom-file-entries)
  - [Other Entries](#other-entries)
- [File Formats](#file-formats)
  - [Hitbox Metadata](#hitbox-metadata)
  - [Patterns](#patterns)
  - [Attacks](#attacks)
  - [Events](#events)
  - [Functions](#functions)
- [Helper Objects](#helper-objects)
- [Player Health System](#player-health-system)
  - [Setup Entries](#setup-entries)
  - [Health Bar Entries](#health-bar-entries)
  - [Health System Triggers](#health-system-triggers)
- [Disclaimer](#disclaimer)

## Boss Setup

The Boss Controller Entity has multiple entries that will enable the Boss to work, as well as a few optional entries. It is important to note that the code for the Boss is separated into two main classes: the Controller and the Puppet. The Controller will run all logic regarding attacks, events, tracking health, and similar. The Puppet handles all hitboxes, collisions, and sprite logic. Due to this, no reference to the Controller is given to the user at any time, and any methods to interact with it are given through delegates or helper functions.

### Basic Setup Entries

These entries are all related to the basic appeareance of the Boss and its interaction with the game engine, incliding gravity and the Player.

- Boss ID: A unique ID to identify a specific Boss instance from Lua.
  - While not enforced to be unique, it's useful if multiple files are used by multiple Boss instances as to difference between them.
- Boss Sprite: The name of the sprite the boss will use. Works the same way as you'd set the sprite of a custom NPC.
- Boss Health Max: This is the amount of health points the Boss will start with. Not all Bosses have to work on Health, however.
- Hurt Mode: This will determine how the Player can collide with the Boss to trigger a user-defined method to declare what happens, but it's usually lowering the health and start attacking again.
  - Player Contact: The Boss will activate collision as soon as the player enters in contact with it.
  - Player Dash: The Boss will active collision when the Player's Dash is active (more specifically while their Dash Attack is active).
  - Head Bounce: The Boss will have a hitbox on top similar to that of Oshiro Boss, activating its collision the same way.
  - Sidekick Attack: Custom addition from this helper. The player will have a Badeline Dummy Follower, which can be activated with a custom bind so she begins targetting the nearest Boss (with the Sidekick Attack hurt mode) and shoots a laser towards it, and then enters cooldown.
  - Custom: The Boss will start with no hurtbox logic and it will entirely depend on the user's code in the Functions Lua file or other Lua files. This means that if no `setup()` function can be found inside of the Functions file, nothing will happen on collision.
    - It is important to note that all Hurt Modes will still execute the `setup()` method found in this file if provided. The main difference is that Custom provides _nothing_ but the code within `setup()`, whereas the other Hurt Modes will include their own logic _on top_ of anything provided inside `setup()`.
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

`Hitboxes` are used for collisions with solids and will hold the KillOnContact Player Collider if marked. `Hurtboxes` are used specifically for the Boss being hit and applying the correct collision logic. `Bouncebox` is specifically only used if the Hurt Mode is set to `Head Bounce`, and is the hitbox or hibox group that must be collided with to bounce. `Target` is a Circle hitbox or hitbox group that Badeline Sidekick will track and aim towards whenever it tries to shoot. It will collide only with Badeline Sidekick's Lasers and react accordingly. If there exists more than one Sidekick Target, Badeline will target the closest one.

`Hitboxes` and `Hurtboxes` alike can have multiple hitboxes in the same set, comprised of Rectangular Hitboxes or Circle Hitboxes, marked with `Hitbox` and `Circle` respectively. Each set of `Hitboxes` or `Hurtboxes` can have a tag to differentiate them. If no tag is given, it will use the default tag "main" and will be hitbox set to be used when the Boss loads in by default. Likewise, a set tagged "main" will be used initially. All sets are stored in a Dictionary so make sure each set, if multiple, had unique tags. `Hitboxes` and `Hurtboxes` are stored in separate Dictionaries so duplicate tags between a set of Hitboxes and a set of Hurtboxes is acceptable. Any number of sets can be defined, even none at all.

Each Rectangular Hitbox has up to 4 attributes: `width`, `height`, `xOffset`, and `yOffset`. Each Circle Hitbox has up to 3 attributes: `radius`, `xOffset`, and `yOffset`. If a value isn't given, `width` and `height` will default to 8 (1 tile), `radius` defaults to 4 (1 tile diameter), and `xOffset` and `yOffset` will default to 0. If `Bouncebox` is not provided with `height`, it will default to 6 instead of 8.

`Bouncebox` and `Target` behave somewhat similarly, but due to the fact that they are restricted to Rectangular hitboxes and Circle colliders respectively, they are formatted somewhat differently. Instead of having a Parent node to delimit a set, any number of `Bouncebox` or `Target` nodes that share the same tag (no tag is equivalent to `tag="main"`) will be considered as part of the same set. `Bouncebox` attributes are the same as those of a Rectangular Hitbox, and `Target` attributes are the same as those of a Circle Hitbox. All `Bouncebox` and `Target` hitboxes are also stored in their respective dictionaries, so shared tags between different hitbox node type is accepted.

The Tags for each set of Hitboxes is useful for switching between them with designated helper functions, such as `changeBaseHitboxTo()`. Each of the 4 Hitbox Type sets (Hitbox, Hurtbox, Bouncebox, Target) has their own method, as each one is stored in separate dictionaries.

If no `Hitboxes` node is provided, the Boss will use a default Hitbox of the dimensions and position of the Boss's Sprite, aligned with the sprite's position. Same applies for `Hurtboxes`. If no `Bouncebox` node is provided, it will use a default Hitbox of the same width of the Boss's Sprite, height of 6, and aligned to the top of the sprite. If no `Target` node is provided, the Boss will use a default Circle of radius 4 and offset 0. If no file is provided, the Boss will use all of the previously mentioned default hitboxes.

### Patterns

This XML file uses the format of the following example:

```xml
<Patterns>
    <!--Deterministic Looping Pattern-->
    <Pattern>
        <Wait time="2"/>
        <Attack file="third"/>
        <Wait time="2"/>
        <Attack file="first"/>
    </Pattern>

    <!--Deterministic Looping Pattern with Pre-Loop Actions-->
    <Pattern>
        <Wait time="2"/>
        <Attack file="start"/>
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

    <!--Deterministic Looping Until Player is in a Given Position Pattern-->
    <Pattern x="20" y="20" width="100" height="100" goto="1">
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
        <Attack file="second" wait="1"/>
        <Attack file="third" wait="2"/>
        <Attack file="fourth" wait="0.6"/>
    </Random>
</Patterns>
```

The entirety of the contents are inside the `Patterns` node. There can be as many `Pattern`, `Random`, or `Event` nodes inside, but there must be at least 1 in general of either kind. All nodes in the file are stored in the same order as they are in the file, and are indexed as such as well, inside an index-0 array (meaning that the first Pattern provided is index 0).

`Pattern` nodes delimits any deterministic set of Actions that will always be performed in the same order. In contrast, `Random` nodes deilimit any set of Actions that will have no set order, and therefore has a different format. Any Pattern, be deterministic or random can be manually interrupted within the Boss's Collision logic functions. `Event` nodes are used to have actual cutscenes in the middle of a fight, between attack patterns.

`Pattern` nodes can have different attributes to make them end in different ways.

- If no attributes are defined, then the Pattern will loop indefinitely unless manually interrupted when the Boss is collided with.
- If a `goto` attribute is provided, the Pattern will then go to the indicated Pattern with the matching index when the Pattern ends.
- If a `repeat` attribute is provided alongside a `goto` attribute, the Pattern will loop however many times as specified in repeat. A value of 0 will run the Pattern once from top to bottom and then go to the given pattern. A value of 1 will execute twice and then run. It is defined as how many _additional_ loops will run until it ends.
  - Only providing `goto` with no `repeat` is the same as providing `repeat` with value 0.
  - Only providing `repeat` with no `goto` will make it so goTo pattern is the one directly below the current one.
- Alternatively, alongside a `goto` attribute, attributes `x`, `y`, `width`, and `height` can be provided. These attributes will delimit a rectangle at a given position. Whenever the Player is inside the given rectangle, it will go to the given pattern once the current action ends.
  - The coordinates for the `x` and `y` attributes are room coordinates.

`Random` nodes take no attributes.

`Event` nodes can take up to two attributes.

- A `file` attribute is required, and must match the name of a `.lua` file inside the Events subdirectory provided.
- A `goto` attribute may be provided if the Event should go to a specific pattern after the Cutscene ends.
  - Not providing this attribute will start the next available pattern after this one.

`Pattern` nodes can have any number of nodes inside them.

- `Wait` nodes signify the Boss will not do anything during the specified `time` attribute.
- `Attack` nodes signify the Boss will perform the attack found inside the file mathcing the `file` attribute.
- `Loop` nodes can only be used once per `Pattern` node, and they delimit the Pattern's loop. If a `Loop` node is found inside a `Pattern` node, everything above it will execute once, when the pattern begins, and everything below it will execute with whatever loop logic is provided in the `Pattern` node attributes.
  - If more than one `Loop` node exists inside the same `Pattern` node, only everything after the last one will loop and only everything before the last one (but after the previous one) will execute before the loop.
  - If no `Loop` node is provided, all actions inside the parent node will be part of the pattern execution loop. If one is provided, only everything after it will be part of the loop, and everything prior will only execute at the start. By adding a node, you're essentially moving where the `while (true)` statement to that line.

The `file` attribute of both `Attack` and `Event` nodes **must not** contain the `.lua` extension.

`Random` nodes can also have any number of nodes inside them.

- `Attack` nodes signify the Boss will perform the attack found inside the file mathcing the `file` attribute, and then will do nothing during the time provided in the `wait` attribute.

`Random` nodes take no `Wait` nodes because otherwise attacks would execute back to back with no pause in between them, or execute multiple waits back to back. Therefore, each node inside this one has to provide their own post-execution wait time.

This file is mandatory and at least one node must exist within the parent node. Any node inside of this will naturally execute indefinitely, except for Events. No pattern will interrupt itself, unless it's by loop count or player being in the specified region, of the Event ending. Even when a Pattern is interrupted, it must be restarted manually or go to a different pattern in the player collision logic.

### Attacks

Every Lua file inside the Attacks subdirectory should use the following format:

```lua
function onBegin()
    --Your code here
end
```

An `onBegin()` function must be provided, which holds the code the attack will execute. Given that it's also its own files, any number of local functions can be defined and used within the function. Each file is provided with a reference to the `player`, the Boss's ID under `bossID`, the Boss's `puppet`, and multiple controller delegate functions under `bossAttack`, as well as access to all regular helper functions.

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

An `onBegin()` function must be provided, which holds the code the cutscene will execute. Given that it's also its own files, any number of local functions can be defined and used within the function. Each file is provided with a reference to the `player`, the Boss's ID under `bossID`, the Boss's `puppet`, the Event file itself under `cutsceneEntity`, and access to all regular helper functions. An `onEnd(level, wasSkipped)` function is not required, but is recommended for handling cleanup and cutscene skipping logic. These files follow the same rule as the LuaCutscenes helper from Cruor. Events, like Cutscenes, are Skippable by default.

### Functions

This Lua file should follow the following format:

```lua
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

This file contains all code that will execute arbitrarily to the Boss either at the start of scene or when collided with.

The functions `onContact()`, `onDasH()`, `onBounce()`, and `onLaser()` will each execute separetely when the Boss's Hurtbox is collided with, depending on the Hurt Mode: `onContact()` for Player Contact, `onDasH()` for Player Dash, `onBounce()` for Head Bounce, and `onLaser()` for Sidekick Attack, respectively. The function `onHit()` is a more generalized function and will be called if no specific method is provided. For example, if Hurt Mode Player Dash is used and the given file has no `onDash()` function, it will call `onHit()` instead. If no `onHit()` function is provided there either, then no code will be executed on collision with the Boss.

These collision functions are essential to fight logic, as its where the user must specify if the Boss takes damage, if the current pattern should be interrupted, if it should wait for the current attack to end, and a new attack pattern should start. Lua helper functions are provided for each of these necessities through delegates.

The `setup()` function will be called during load time, before the scene starts. It can be used to give the Boss additional components, sprites, or starting values. This function is not necessary. If the Hurt Mode was set to Custom, it's highly encouraged to add this file, since otherwise the Boss cannot be hurt. This function will still be called regardless of Hurt Mode, though.

All function in this file are provided with a reference to the `player`, the Boss's ID under `bossID`, the Boss's `puppet`, and multiple controller delegate functions under `boss`, as well as access to all regular helper functions. The delegates provided here are different from the ones provided for Attacks, as explained in the Helper functions file.

## Helper Objects

This Helper also adds a few Entities and Components for ease of use or just general usage.

- Entity Chain Component: Can be used to "chain" an entity to another one.
  - Constructor Parameters:
    - Entity entity: The Entity this chain will be set on. This is the additional entity.
    - bool chainPosition: If the chained entity should move around as the chained to one moves, essentially movind as one.
    - bool active: If the Entity should be Active.
    - bool visible: If the Entity should be Visible.
  - Can be called from Lua with `celeste.Mod.BossesHelper.Code.Components.EntityChainComp(params)`.
- Attack Actor: A generic Entity which subclasses the Actor class and has movement and collision logic.
  - Constructor Parameters:
    - Vector2 position: Where the Actor will spawn.
    - Collider attackbox: The Collider Hitbox the Actor will use.
    - LuaFunction onPlayer: The function to call when the player collides with the Actor's hitbox.
    - bool startCollidable: If the Actor's hitbox should start collidable when added.
    - string spriteName: The name of the sprite to use.
    - float gravMult: The multiplier on the Gravity constant (900, same as the player) that should apply to this Actor.
    - float maxFall: The max speed gravity will accelerate this Actor to by normal means. This value can be surpassed manually.
    - float xScale: The Actor's Sprite's X scale. Defaults to 1.
    - float yScale: The Actor's Sprite's Y scale. Defaults to 1.
  - Can be called from Lua with `celeste.Mod.BossesHelper.Code.Entities.AttackActor(params)`.
- Attack Entity: A generic Entity which can be used for simple disjointed hitboxes.
  - Constructor Parameters:
    - Vector2 position: Where the Entity will spawn.
    - Collider attackbox: The Collider Hitbox the Entity will use.
    - LuaFunction onPlayer: The function to call when the player collides with the Entity's hitbox.
    - bool startCollidable: If the Entity's hitbox should start collidable when added.
    - string spriteName: The name of the sprite to use.
    - float xScale: The Entity's Sprite's X scale. Defaults to 1.
    - float yScale: The Entity's Sprite's Y scale. Defaults to 1.
  - Can be called from Lua with `celeste.Mod.BossesHelper.Code.Entities.AttackEntity(params)`.
- Entity Collider: A Generic Typed Component that can be used to enable/track collisions between the parent entity and the Entity type specified and execute a function.
  - In order to add one to an entity within Lua, call the `addEntityColliderTo()` helper function.
    - The second parameter, the one used to define the type of the Entity it should collide with, like Springs or Spinners, can be either an instance of the entity itself or a string with the absolute path to it, including namespace, but still relative to the `Celeste.` namespace.
      - For Springs, for example, you can provide either a Spring object or the "Spring" name.
      - For Maddie Helping Hand MoreCustomNPC, for example, you can also provide an instance or the "Mod.MaxHelpingHand.Entities.MoreCustomNPC" name.
  - This Component is also created such that it can be used by anything.

A basic collider can be obtained with the `getHitbox()` or `getCircle()` helper functions, which can be combined with the `getColliderList()` function. A basic vector2 object can be obtained with `vector2(x,y)`.

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
  - Keep Old Value: The Health System will keep using whatever it's old crush effect was.
- Offscreen Effect: How the Health System should react whent the player dies due to falling offstage.
  - Bounce Up: The player will take damage and will bounce, similar to what would happen if the Invinciblity Assist was turned on.
    - While not fully encompassing with other mods, it should at least still work with Gravity Helper's dying up offstage.
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
- Player Stagger: If the player's movement and controll should be interrupted when taking damage as the player staggers back from the hit.
- On Damage Function: This takes a path to a Lua file, whose `onDamage()` method will execute every time the player takes damage.
  - Make sure your main code is inside the `function onDamage() ... end` block.

### Health Bar Entries

The Health System is accompanied with a Health Bar, which are the other half of the entries, and only encompass the visual side of the Health System.

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

- Add Health System Trigger: Can be used to add an instance of a Health System, and works the exact same way as adding the Controller would, except it's delegated to be added onto the scene when entering the Trigger.
  - If a Health System already exists, the values provided will override those of the existing System.
  - If a specific value is not provided, the pre-exisiting value for that entry will be kept, similar to adding another Health System manually.
- Enable Health Trigger: Can be used to toggle the Enabled state of the existing Health System.
- Health Bar Visible Trigger: Can be used to toggle the Visible state of all the icons of the Health Bar.
- Recover Health Trigger: Can be used for a quick simple way to recover some hit points for the player. `Only Once` will remove the trigger once entered, `Permanent` will make it so that the trigger doesn't load again

## Bosses Helper API

This Helper includes a few things exported with ModInterop, namely:

- **GetEntityColliderComponent**: Returns an Entity Collider Component ready to be used by any entity with any other entity Type.
- **GetEntityChainComponent**: Returns an Entity Chain Component to keep two entities's positions tied together.
- **GetCurrentPlayerHealth**: Useful to know if a Health System is active on Scene and if so, what the Player's health value is at. Returns -1 if no Health System is active.
- **RecoverPlayerHealth**: Makes the player recover health points as many as the value given. This can go over the PLayer's default health value.
- **MakePlayerTakeDamage**: Useful to extend utility from the Health System so as to set custom parameters to taking damage.

The Mod Import Name is `"BossesHelper"`.

## Disclaimer

Currently I have referenced and used code from Ricky06's Conqueror's Peak, Cruor's LuaCutscenes, and IsaGoodFriend's BadelineFollower, all of which either have their code under license or have explicitly allowed me to do code reference and copying. All other code is original or from Vanilla Celeste.
