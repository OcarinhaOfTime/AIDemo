using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class ArrayExtention {
    public static bool IsInMapRange<T>(this T[,] map, int x, int y) {
        return x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1);
    }
}
