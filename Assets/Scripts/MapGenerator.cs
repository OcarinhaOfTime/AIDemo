using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
    public int width;
    public int height;

    public string seed;
    public bool randomSeed;

    [Range(0, 100)]
    public int randomFillPercent = 48;
    public int smoothSteps = 5;
    public int roomThresholdSize = 50;
    public int borderSize = 2;
    public int passageSize = 5;

    public static Map<int> map { get; private set; }

    public void GenerateMap() {
        map = new Map<int>(width, height);
        RandomFillMap();

        for(int i = 0; i < smoothSteps; i++) {
            SmoothMap();
        }

        CullMap();

        Map<int> borderedMap = new Map<int>(width + borderSize * 2, height + borderSize * 2);

        for(int x = 0; x < borderedMap.width; x++) {
            for(int y = 0; y < borderedMap.height; y++) {
                if(x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize) {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                } else {
                    borderedMap[x, y] = 1;
                }
            }
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(borderedMap, 1);
    }

    void CullMap() {
        List<List<Coord>> wallRegions = GetRegions(1);

        foreach(List<Coord> wallRegion in wallRegions) {
            if(wallRegion.Count < roomThresholdSize) {
                foreach(Coord tile in wallRegion) {
                    map[tile.x, tile.y] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0);
        List<Room> survivingRooms = new List<Room>();

        foreach(List<Coord> roomRegion in roomRegions) {
            if(roomRegion.Count < roomThresholdSize) {
                foreach(Coord tile in roomRegion) {
                    map[tile.x, tile.y] = 1;
                }
            } else {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }
        survivingRooms.Sort();
        survivingRooms[0].isMainRoom = true;
        survivingRooms[0].isAccessibleFromMainRoom = true;

        ConnectClosestRooms(survivingRooms);
    }

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false) {

        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if(forceAccessibilityFromMainRoom) {
            foreach(Room room in allRooms) {
                if(room.isAccessibleFromMainRoom) {
                    roomListB.Add(room);
                } else {
                    roomListA.Add(room);
                }
            }
        } else {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach(Room roomA in roomListA) {
            if(!forceAccessibilityFromMainRoom) {
                possibleConnectionFound = false;
                if(roomA.connectedRooms.Count > 0) {
                    continue;
                }
            }

            foreach(Room roomB in roomListB) {
                if(roomA == roomB || roomA.IsConnected(roomB)) {
                    continue;
                }

                for(int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++) {
                    for(int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++) {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.x - tileB.x, 2) + Mathf.Pow(tileA.y - tileB.y, 2));

                        if(distanceBetweenRooms < bestDistance || !possibleConnectionFound) {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }
            if(possibleConnectionFound && !forceAccessibilityFromMainRoom) {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if(possibleConnectionFound && forceAccessibilityFromMainRoom) {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if(!forceAccessibilityFromMainRoom) {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB) {
        Room.ConnectRooms(roomA, roomB);
        List<Coord> line = GetLine(tileA, tileB);

        foreach(Coord c in line) {
            IncreasePassage(c, passageSize);
        }
    }

    void IncreasePassage(Coord tile, int r) {
        for(int x = -r; x <= r; x++) {
            for(int y = -r; y <= r; y++) {
                if(x * x + y * y <= r * r) {
                    int x_ = tile.x + x;
                    int y_ = tile.y + y;
                    if(map.IsInMapRange(x_, y_)) {
                        map[x_, y_] = 0;
                    }
                }
            }
        }
    }

    List<Coord> GetLine(Coord from, Coord to) {
        List<Coord> line = new List<Coord>();

        int x = from.x;
        int y = from.y;

        int dx = to.x - from.x;
        int dy = to.y - from.y;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if(longest < shortest) {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for(int i = 0; i < longest; i++) {
            line.Add(new Coord(x, y));

            if(inverted) {
                y += step;
            } else {
                x += step;
            }

            gradientAccumulation += shortest;
            if(gradientAccumulation >= longest) {
                if(inverted) {
                    x += gradientStep;
                } else {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    List<List<Coord>> GetRegions(int tileType) {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for(int x = 0; x < width; x++) {
            for(int y = 0; y < height; y++) {
                if(mapFlags[x, y] == 0 && map[x, y] == tileType) {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach(Coord tile in newRegion) {
                        mapFlags[tile.x, tile.y] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY) {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while(queue.Count > 0) {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            map.MapNeighborIter(tile, (v, x, y) => {
                if(mapFlags[x, y] == 0 && map[x, y] == tileType) {
                    mapFlags[x, y] = 1;
                    queue.Enqueue(new Coord(x, y));
                }
            });
        }

        return tiles;
    }


    void RandomFillMap() {
        if(randomSeed) {
            seed = Time.time.ToString();
        }
        randomSeed = true;

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for(int x = 0; x < width; x++) {
            for(int y = 0; y < height; y++) {
                if(x == 0 || x == width - 1 || y == 0 || y == height - 1) {
                    map[x, y] = 1;
                } else {
                    map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap() {
        for(int x = 0; x < width; x++) {
            for(int y = 0; y < height; y++) {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if(neighbourWallTiles > 4)
                    map[x, y] = 1;
                else if(neighbourWallTiles < 4)
                    map[x, y] = 0;

            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY) {
        int wallCount = 0;
        for(int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
            for(int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
                if(map.IsInMapRange(neighbourX, neighbourY)) {
                    if(neighbourX != gridX || neighbourY != gridY) {
                        wallCount += map[neighbourX, neighbourY];
                    }
                } else {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }
}