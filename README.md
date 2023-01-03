# Stealing Wizards Treasure
This repository contains an example of a 2D tile-based game made in Unity. The player controls a thief who has broken into a wizard's tower. The goal is to run around collecting treasure without letting the wizard touch you. After stealing as much treasure as possible, the player heads for the staircase and the game ends.
<br />
<br />

## A* Pathfinding
The main highlight of this example game is A* pathfinding. The *Wizards.cs* script component attached to the *Wizard* gameobject finds the shortest path between the Wizard's current location and the location occupied by the player. The floor tilemap is used to automatically generate a 2D array of nodes, each node representing an individual floor tile. During the generation of that array, the wall tilemap is examine to determine if there is a wall over the current floor tile. If there is, the node is marked as not "walkable."

In the *FixedUpdate* method in the *Wizard.cs* script component, the *UpdateDestination* method is called every one second. In that method, the destination is only updated if the distance the player has moved since the last time the destination was updated is greater than 0.5 (accessible via a property called *distanceToRecalulate* which can be set via the inspector on the *Wizard* gameobject.) When the destination is updated, the Wizard stops and then refresh the path to the player's location. The timing of events makes it so that the Wizard never appears to stop moving.

Special consideration is taken for situations where the player moves out-of-bounds or to a not "walkable" node. This is possible if the player presses the backslash key on the keyboard or the select button on a controller to enable *no-clip mode.* The Wizard knows the player is cheating, stops moving, and screams.
<br />
<br />

## Unity's New Input System
Another highlight of this project is the use of Unity's new input system. WASD keyboard input or controller d-pad input is used to move the player up, down, left, right. Diagonal movement is disabled for the player (and also for the Wizard). The input system also handles enabling/disabling *no-clip mode* by pressing the backslash key or the select button on the controller.
<br />
<br />

## Other Highlights
This project showcases the ability to balance movement speeds of the player and an enemy which (combined with thoughtful level design) achieves a reasonable amount of difficulty. It also showcases several sprite-based animations: idle, walking, casting, etc.
<br />
<br />

# Additional Information

- This example project wascreated using Unity version 2021.3.4f1
- The sprites are borrowed from the MS-DOS EGA version of Ultima V
- The music is borrowed from the the Apple II version of Ultima IV (played through the Mockingbird sound card)
- The sound effects are borrowed from Sinistar, Wolfenstein 3D, Doom, and The Legend of Zelda