using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour {
    public Camera cam1;
    public Camera cam2;
    bool cam1On = true;

    void Update () {
        if(Input.GetKeyDown(KeyCode.S)) {
            cam1On = !cam1On;
            cam1.gameObject.SetActive(cam1On);
            cam2.gameObject.SetActive(!cam1On);
        }
    }
}
