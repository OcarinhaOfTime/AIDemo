using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script used for testing with custo text files
/// </summary>
public class ProcessSimpleInput : MonoBehaviour {
    public TextAsset file;
    public MeshGenerator meshGenerator;
    int[,] map;

    private void Start() {
        ProcessFile();
        DebugMap();
    }

    [ContextMenu("ProcessFile")]
	public int[,] ProcessFile() {
        string[] lines = file.text.Split('\n');
        map = new int[lines.Length, lines[0].Split(' ').Length];

        for(int i=0; i < lines.Length; i++) {
            string[] colums = lines[i].Split(' ');
            for(int j=0; j< colums.Length; j++) {
                map[j, i] = int.Parse(colums[j]);
            }
        }

        return map;
    }

    [ContextMenu("GenerateMap")]
    public void GenerateMap() {
        meshGenerator.GenerateMesh(new Map<int>(map), 1);
    }

    [ContextMenu("FindShortestPah")]
    void DebugMap() {
        var m = new Map<int>(map);
        var pf = GetComponent<PathFind>();
        var a = new Coord(2, 1);
        var b = new Coord(2, 3);
        var path = pf.FindShortestPah(m, b, a);
        foreach(var tile in path) {
            Debug.Log(m.WorldPointToCoord(tile));
        }
    }
}
