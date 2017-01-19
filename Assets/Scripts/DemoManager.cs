using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoManager : MonoBehaviour {
    public Transform[] agents;
    public MapGenerator mapGenerator;
    public List<Coord> emptyTiles = new List<Coord>();
    public Vector3 randomEmptyPoint { get {
            return MapGenerator.map.CoordToWorldPoint(emptyTiles[Random.Range(0, emptyTiles.Count)]);
        } }

    private void Start() {
        NewMap();
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.N))
            NewMap();
    }

    public void NewMap() {
        emptyTiles.Clear();
        mapGenerator.GenerateMap();

        MapGenerator.map.MapIter((v, x, y) => {
            if(v == 0)
                emptyTiles.Add(new Coord(x, y));
        });

        foreach(var agent in agents) {
            agent.position = randomEmptyPoint;
        }
    }
}
