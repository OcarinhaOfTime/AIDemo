using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float speed = 10;
    public float turnSpeed = 10;
    Rigidbody rb;

    private void Start() {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        var h = Input.GetAxis("Horizontal");
        rb.MoveRotation(Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + h * turnSpeed, transform.rotation.eulerAngles.z));
    }
}
