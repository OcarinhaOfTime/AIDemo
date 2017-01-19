This project uses 3 kinds of AI topics: procedural content generation and marching cubes, steering behaviors (seek,flee and wander) 
 and A* path finding.
 The procedural content generation is used to make random levels for the demo. 
 The algorithm uses a binary grid based discretization (where 0 is an empty space and 1 is a wall) 
 and consist of the following steps:
 1. random grid points generation: with a given seed, the algorithm randomly fills the
 grid map, with a fillPercent parameter that determine how dense / sparse the grid will be.
 2. map smoothing: based on celular automata rules, the grid is smoothed in order to produce
 better looking caves
 3. border generation: to assure borders around the entire cave, there is a step for generating a border for the map
 4. region detection: in order to detect regions for culling and room connection, the flood flow algorithm is used
 5. isolated rooms / sparse islands culling: in order to avoid small isolated rooms / walls
 6. rooms connection: using a distance based aproach, connects all the rooms and do a second step to ensure room connectivity
 in the map, the algorithm performs a step of culling the rooms givem a minimum size
 7. 3d mesh generation: using the marching squares algorithm, turns the binary grid in to a 3D mesh

 The steering behaviors are used to control the enemy agents. They have the following states, that are
 setted using player input:
 1 seek: the enemy's velocity is smoothly changed towards the target. In order to avoid the cave walls,
 the A* algorithm is used to generate a set of points that will make the shortest path to the player.
 The path is represented as a stack of vector3 and the seek behavior moves toward one point at time, till
 the path is empty meaning he has reached it's goal. The path is recalculated each frame to allow the enemy's
 target to move while the enemy is seeking for it
 2. flee: the enemy simply try to move away from the player, using a steering force to smoothly change it's direction
 3. wander: reusing part of the code of the seek behavior, wander randomly picks an empty coordinate of the map,
 then calculates the shortest path to it, and finally, uses the same logic of seek to reach the random spot. When he
 arrives at the destination, he picks another random empty coordinate and start again
 
A* path finding:
The a* algorithm is applied over the binary grid map used to discretize the cave. Given the start
position and the player position, the algorithm searches over all the map avoiding the wall tiles.
On the path reconstruction step, the grid coodinates are converted to world space points and store in
a stack for future use in the steering behavior script