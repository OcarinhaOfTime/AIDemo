using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SteerState {
    Idle,
    Seek,
    Flee,
    Wander
}
/// <summary>
/// Handles the enemy movement using steering behaviors
/// </summary>
public class EnemyAI : MonoBehaviour {
    public Transform target;
    public SteerState state = SteerState.Idle;
    public float maxSpeed = 5f;
    public float maxSteering = .5f;
    private PathFind pathFind;
    public float seekThreshold = .1f;
    public float fleeSafeDist = 10f;
    public DemoManager demoManager;
    public Text stateTxt;

    Rigidbody rb;
    Stack<Vector3> seekPath = new Stack<Vector3>();
    Stack<Vector3> wanderPath = new Stack<Vector3>();

    private void Start() {
        rb = GetComponent<Rigidbody>();
        pathFind = GetComponent<PathFind>();
    }

    private void OnDrawGizmos() {
        var p = wanderPath.ToArray();
        for(int i=0; i<p.Length-1; i++) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(p[i], p[i + 1]);
        }
    }

    public void Update() {
        if(Input.GetKeyDown(KeyCode.Alpha1))
            state = SteerState.Idle;
        else if(Input.GetKeyDown(KeyCode.Alpha2)) {
            seekPath = pathFind.FindShortestPah(MapGenerator.map, transform.position, target.position);
            state = SteerState.Seek;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha3))
            state = SteerState.Flee;
        else if(Input.GetKeyDown(KeyCode.Alpha4)) {
            wanderPath.Clear();
            state = SteerState.Wander;
        }
            
        else if(Input.GetKeyDown(KeyCode.C)) {
            seekPath = pathFind.FindShortestPah(MapGenerator.map, transform.position, target.position);
            Debug.Log("Searchiing, Seek and Destroy");
        }
        stateTxt.text = "Enemy State: " + state;
    }

    public void FixedUpdate() {
        switch(state) {
            case SteerState.Wander:
                Wander();
                break;
            case SteerState.Seek:
                Seek();
                break;
            case SteerState.Flee:
                Flee();
                break;
            default:
                rb.velocity = Vector3.zero;
                break;
        }
    }

    void Wander() {
        if(wanderPath.Count == 0) {
            wanderPath = pathFind.FindShortestPah(MapGenerator.map, transform.position, demoManager.randomEmptyPoint);
            return;
        }
        
        Vector3 tar = wanderPath.Peek();
        var d = Vector3.Distance(tar, transform.position);
        if(d < seekThreshold) {
            wanderPath.Pop();
        } else {
            Seek(tar);
        }
    }

    float t = 0;
    void Seek() {
        if(seekPath.Count == 0) {
            seekPath = pathFind.FindShortestPah(MapGenerator.map, transform.position, target.position);
            rb.velocity = Vector3.zero;
            return;
        }
        Vector3 tar = seekPath.Peek();
        var d = Vector3.Distance(tar, transform.position);
        if(d < seekThreshold) {
            seekPath.Pop();
        } else {
            Seek(tar);
        }

        seekPath = pathFind.FindShortestPah(MapGenerator.map, transform.position, target.position);
    }

    void Seek(Vector3 tar) {
        Vector3 dir = (tar - transform.position).normalized;
        Vector3 desiredVelocity = dir * maxSpeed;
        Vector3 steering = Vector3.ClampMagnitude(desiredVelocity - rb.velocity, maxSteering);
        rb.velocity = desiredVelocity;
    }

    void Flee() {
        var d = Vector3.Distance(target.position, transform.position);
        if(d > fleeSafeDist) {
            state = SteerState.Idle;
        } else {
            Flee(target.position);
        }
    }

    void Flee(Vector3 tar) {
        Vector3 dir = (transform.position - tar).normalized;
        Vector3 desiredVelocity = dir * maxSpeed;
        Vector3 steering = Vector3.ClampMagnitude(desiredVelocity - rb.velocity, maxSteering);
        rb.velocity = desiredVelocity;
    }
}
