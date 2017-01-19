using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Wraps a map structure
/// </summary>
/// <typeparam name="T"></typeparam>
public class Map<T> : IEnumerable<T> {
    public int width { get { return map.GetLength(0); } }
    public int height { get { return map.GetLength(1); } }
    T[,] map;

    public Map(int x, int y) {
        map = new T[x, y];
    }

    public Map(int x, int y, T defaultValue) {
        map = new T[x, y];
        for(int i = 0; i < x; i++) {
            for(int j = 0; j < y; j++) {
                map[i, j] = defaultValue;
            }
        }

    }

    public Map(T[,] map) {
        this.map = map;
    }

    public T this[int x, int y] {
        get {
            return map[x, y];
        }
        set {
            map[x, y] = value;
        }
    }

    public T this[Coord coord] {
        get {
            return map[coord.x, coord.y];
        }
        set {
            map[coord.x, coord.y] = value;
        }
    }


    public Vector3 CoordToWorldPoint(Coord tile) {
        return new Vector3(-width / 2 + .5f + tile.x, -3.5f, -height / 2 + .5f + tile.y);
    }

    public Coord WorldPointToCoord(Vector3 pos) {
        return new Coord(Mathf.FloorToInt(pos.x + width / 2 - .5f), Mathf.FloorToInt(pos.z + height / 2 - .5f));
    }

    public IEnumerator<T> GetEnumerator() {
        for(int x = 0; x < width; x++) {
            for(int y = 0; y < width; y++) {
                yield return map[x, y];
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        for(int x = 0; x < width; x++) {
            for(int y = 0; y < width; y++) {
                yield return map[x, y];
            }
        }
    }

    public void MapNeighborIter(Coord tile, UnityAction<T, int, int> onEach) {
        for(int x = tile.x - 1; x <= tile.x + 1; x++) {
            for(int y = tile.y - 1; y <= tile.y + 1; y++) {
                if(map.IsInMapRange(x, y) && (y == tile.y || x == tile.x)) {
                    onEach.Invoke(map[x, y], x, y);
                }
            }
        }
    }

    public void MapNeighborIter(Coord tile, UnityAction<T, Coord> onEach) {
        for(int x = tile.x - 1; x <= tile.x + 1; x++) {
            for(int y = tile.y - 1; y <= tile.y + 1; y++) {
                if(map.IsInMapRange(x, y) && (y == tile.y || x == tile.x)) {
                    onEach.Invoke(map[x, y], new Coord(x, y));
                }
            }
        }
    }

    public int ObstacleInNeighborHood(Coord tile, T obstacle) {
        int obstacles = 0;
        for(int x = tile.x - 1; x <= tile.x + 1; x++) {
            for(int y = tile.y - 1; y <= tile.y + 1; y++) {
                if(map.IsInMapRange(x, y)) {
                    obstacles += ((map[x, y].Equals(obstacle)) ? 1 : 0);
                }
            }
        }

        return obstacles;
    }

    public void MapIter(UnityAction<T, int, int> onEach) {
        for(int x = 0; x < map.GetLength(0); x++) {
            for(int y = 0; y < map.GetLength(1); y++) {
                onEach.Invoke(map[x, y], x, y);
            }
        }
    }

    public bool IsInMapRange(int x, int y) {
        return x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1);
    }

}

public class Coord {
    public int x;
    public int y;

    public Coord() {
    }

    public Coord(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public float DistSqrt(Coord other) {
        return Mathf.Pow(x - other.x, 2) + Mathf.Pow(y - other.y, 2);
    }

    public float Dist(Coord other) {
        return Mathf.Sqrt(DistSqrt(other));
    }

    public bool Equals(Coord other) {
        return x == other.x && y == other.y;
    }

    public override string ToString() {
        return "{ " + x + ", " + y + " }";
    }
}

class Room : IComparable<Room> {
    public List<Coord> tiles;
    public List<Coord> edgeTiles;
    public List<Room> connectedRooms;
    public int roomSize;
    public bool isAccessibleFromMainRoom;
    public bool isMainRoom;

    public Room() {
    }

    public Room(List<Coord> roomTiles, Map<int> map) {
        tiles = roomTiles;
        roomSize = tiles.Count;
        connectedRooms = new List<Room>();

        edgeTiles = new List<Coord>();
        foreach(Coord tile in tiles) {
            for(int x = tile.x - 1; x <= tile.x + 1; x++) {
                for(int y = tile.y - 1; y <= tile.y + 1; y++) {
                    if(x == tile.x || y == tile.y) {
                        if(map[x, y] == 1) {
                            edgeTiles.Add(tile);
                        }
                    }
                }
            }
        }
    }

    public void SetAccessibleFromMainRoom() {
        if(!isAccessibleFromMainRoom) {
            isAccessibleFromMainRoom = true;
            foreach(Room connectedRoom in connectedRooms) {
                connectedRoom.SetAccessibleFromMainRoom();
            }
        }
    }
    public static void ConnectRooms(Room roomA, Room roomB) {
        if(roomA.isAccessibleFromMainRoom) {
            roomB.SetAccessibleFromMainRoom();
        } else if(roomB.isAccessibleFromMainRoom) {
            roomA.SetAccessibleFromMainRoom();
        }
        roomA.connectedRooms.Add(roomB);
        roomB.connectedRooms.Add(roomA);
    }

    public bool IsConnected(Room otherRoom) {
        return connectedRooms.Contains(otherRoom);
    }

    public int CompareTo(Room otherRoom) {
        return otherRoom.roomSize.CompareTo(roomSize);
    }
}

