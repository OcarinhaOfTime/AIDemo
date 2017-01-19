using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Used to warp the player position
/// </summary>
public class MoveToMouse : MonoBehaviour {
    public Camera cam;
    private void Update() {
        if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
            Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
            pos.y = transform.position.y;
            transform.position = pos;
        }
    }
}
