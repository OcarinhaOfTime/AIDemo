# AI demo
This project uses 3 kinds of AI topics: procedural content generation, steering behaviors (seek,flee and wander) 
and A* path finding.

## Procedural cave generation
The procedural content generation is used to make random levels for the demo. 
The algorithm uses a binary grid based discretization (where 0 is an empty space and 1 is a wall) 
and consist of the following steps:
- Random Grid Points Generation: with a given seed, the algorithm randomly fills the
grid map, with a fillPercent parameter that determine how dense / sparse the grid will be;
- Map Smoothing: based on celular automata rules, the grid is smoothed in order to produce
better looking caves;
- Border Generation: to assure borders around the entire cave, there is a step for generating a border for the map;
- Region Detection: in order to detect regions for culling and room connection, the flood flow algorithm is used;
- Isolated Rooms / Sparse Islands Culling: in order to avoid small isolated rooms / walls;
- Rooms Connection: using a distance based aproach, connects all the rooms and do a second step to ensure room connectivity
in the map, the algorithm performs a step of culling the rooms givem a minimum size;
- 3d Mesh Generation: using the marching squares algorithm, turns the binary grid in to a 3D mesh.

## Steering behaviors
The steering behaviors are used to control the enemy agents. They have the following states, that are
setted using player input:
- Seek: the enemy's velocity is smoothly changed towards the target. In order to avoid the cave walls,
the A* algorithm is used to generate a set of points that will make the shortest path to the player.
The path is represented as a stack of vector3 and the seek behavior moves toward one point at time, till
the path is empty meaning he has reached it's goal. The path is recalculated each frame to allow the enemy's
target to move while the enemy is seeking for it;
- Flee: the enemy simply try to move away from the player, using a steering force to smoothly change it's direction smothly;
- Wander: reusing part of the code of the seek behavior, wander randomly picks an empty coordinate of the map,
then calculates the shortest path to it, and finally, uses the same logic of seek to reach the random spot. When he
arrives at the destination, he picks another random empty coordinate and start again.

## A* Path Finding
The a* algorithm is applied over the binary grid map used to discretize the cave. Given the start
position and the player position, the algorithm searches over all the map avoiding the wall tiles.
The algorithm uses a custom priority queue that uses the the fScore map value of the current open tile
as the priority rule. The heuristic used to estimate the distance between tiles is the squared distance,
to avoid square root operations.
On the path reconstruction step, the grid coodinates are converted to world space points and store in
a stack for future use in the steering behavior script.

## Project Modularity
In order to reuse my code in a similar but distinct context, most of the implementation was done in a modular way.
Since grids are used extensively in this demo, i have created a class Map, with the index overloading and implementation
of the IEnumerable interface to turn this data structure easy to iterate and change. The mesh generation class can be used for any 
binary map grid to make meshes in many different scenarios. PathFind is a class that just worries with the shortest path
generation between 2 points in a grid, and also can be used in many other contexts.

## Demo Scene
The demo scene shows a player (the blue capsule) and 3 enemy agents (the red spheres) at a random place in the procedural 
generated map. With the key N the user can generate a new map, then the player and the enemy agents
will be placed at a random empty point in the map. With the mouse click, the user can set manually the player position.
The number keys 1, 2, 3 and 4 are used to change the enemies steering behavior (idle, seek, flee and wander, respectively).
The S key toggles the camera perspective (as the demo tooks place in a 3D enviroment) to a top view / third person mode.
When in third person mode, the player can turn around the camera with A / D keys.