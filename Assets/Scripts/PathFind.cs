﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used to find the shortest path in a grid map using the A* algorithm
/// </summary>
public class PathFind : MonoBehaviour {
    Map<int> map;
    Stack<Vector3> ReconstructPath(Map<Coord> cameFrom, Coord startPos, Coord finalPos) {
        Stack<Vector3> path = new Stack<Vector3>();

        path.Push(map.CoordToWorldPoint(finalPos));
        Coord it = cameFrom[finalPos];
        while(it != startPos) {
            it = cameFrom[it];
            path.Push(map.CoordToWorldPoint(it));
        }
        path.Pop();
        path.Pop();
        return path;
    }
    public Stack<Vector3> FindShortestPah(Map<int> map, Vector3 startPos, Vector3 target) {
        return FindShortestPah(map, map.WorldPointToCoord(startPos), map.WorldPointToCoord(target));
    }

    /// <summary>
    /// The A* algorithm to find the shortest path between startPos and target
    /// </summary>
    /// <param name="map">Where the search will be made</param>
    /// <param name="startPos">Where the search will start</param>
    /// <param name="target">The goal</param>
    /// <returns>The shortest path (if it exists) to target</returns>
    public Stack<Vector3> FindShortestPah(Map<int> map, Coord startPos, Coord target) {        
        this.map = map;
        Map<bool> closedMap = new Map<bool>(map.width, map.height, false);
        Map<Coord> cameFrom = new Map<Coord>(map.width, map.height);
        Map<float> gScore = new Map<float>(map.width, map.height, Mathf.Infinity);
        Map<float> fScore = new Map<float>(map.width, map.height, Mathf.Infinity);

        PriorityQueue openSet = new PriorityQueue();

        gScore[startPos] = 0;
        fScore[startPos] = startPos.DistSqrt(target);

        openSet.Enqueue(startPos, fScore[startPos]);

        while(!openSet.Empty) {
            Coord current = openSet.Dequeue();
            closedMap[current] = true;

            if(current.Equals(target)) {
                return ReconstructPath(cameFrom, startPos, target);
            }

            map.MapNeighborIter(current, (v, neighbor) => {
                if(v == 0 && !closedMap[neighbor]) {
                    float g = gScore[current] + 1;
                    openSet.Enqueue(neighbor, fScore[neighbor]);

                    if(g < gScore[neighbor]) {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = g;
                        fScore[neighbor] = g + neighbor.DistSqrt(target);
                    }
                }
            });
        }

        throw new System.Exception("No reachable path found");
    }
}
