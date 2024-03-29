# Tank Battle

## Unity Project Structure

The 'entry point' of the application is in the Bootstrap.cs script in the Bootstrap scene.

* There is one persistent singleton GameObject in the Bootstrap scene called UnityWrapper.
* The UnityWrapper is the main bridge between our C# code and the Unity Engine.
* Any class can derive from UnityBehaviour to utilize the magic MonoBehaviour methods without the extra requirement of having an actual GameObject.


## User Interface

There is one central message router, a static class called GameUI, which any object in the game can use to send and receive UIMessages.  To send data from the (UGUI) UI, just attach a UIBUttonHandler component to any UGUI object and configure the UIMessage payload to be sent whenever the button is activated.

* UIButtonHandler uses the IPointer interface, so there is no need to manually add an OnClick event to the UI.Button.
* I use IPointer to add some extra button functionality beyond UI.Button's default behaviour, like 'tap & hold', as well as better touchscreen interactions, like dragging your finger on and off the button.

I also created a custom property drawer to make it much easier to format UIMessage payloads in the inspector.

* Add any number of payload items to your UIMessage with many supported data types, including UI.Dropdown support.
* See Assets/Editor/UIMessagePayloadItemDrawer.cs or Canvas-MainMenu/Panel/Button-NewGame in the MainMenu scene to see how the player's selected options are passed to the game.

Keyboard shortcuts are tied into this same system via UserInput.cs

* UIMessages are bound to KeyCodes and polled/broadcast on Update().


## Code Architecture

This project tries to keep a clear boundary between the Unity engine and the framework of the game.

__Game__ is a static class which controls the high-level flow of the game.
* Static game config, scene switching, and procedural level generation happens here.

__Entity__ is the core class used to create game logic.
* An entity maintains an arbitrary collection of functionality, called Modules.
* EntityFactory maintains methods for creating specific types of entities, like Tanks and Bullets.

__Module__ is where all of the fun stuff happens.
* A module can define itself with any number of ModuleTypes.
* It can also depend on any number of other ModuleTypes. These dependencies are managed and enforced via the parent Entity.

Here are some examples of important Modules:
* __TransformModule__: creates a GameObject and caches a reference to its Transform. Used by anything that wants to physically exist in the scene.
* __AgentModule__: the tanks use CharacterControllers, which must exist on the root of the GameObject and are more easily maintained via prefabs (rather than adding these components are runtime) -- so this module is of type Transform as well as Agent.
* __PrefabModule__: instantiates a prefab from Resources and attaches it to a TransformModule.
* __PrefabModule<T>__: also caches a references to a MonoBehaviour on the prefab.
* __CollisionModule__: links GameObject collision callbacks to our module. Used for all kinds of hit detection.
* __HealthModule__: manages hitpoints and lets other objects know when we die, which drives a majority of game flow.
* __PathfindingModule__: a simple A* implementation used by __NpcAgentModule__ bots to navigate the world. It's a bit janky but seems to work well enough.


## Testing the Pathfinding

To test my A* implementation I created some runtime debug options.

* Press the __P key__ to enable pathfinding debug mode.
* This will generate paths one at a time from the player to each tile in the terrain grid.
* Paths are drawn via Debug.DrawLine and appear in the Scene view in the editor, or in the Game window if you enable gizmos.
* While in this mode, press the __N key__ to generate the next path.


## Imported Code

There are two bits of code I stole from some of my older gamejams:

* AgentModule: this is a simple CharacterController interface which I adapted into this project's Entity/Module framework.
* TerrainGenerator: this is a maze generator algorithm from an old blog post of mine, which I adapted to procedurally generate levels for TankBattle. You can read more about it (and play with a demo) on my website at http://refugezero.com/index.php?path=software&sub=demos


