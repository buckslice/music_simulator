using UnityEngine;
using System.Collections;

public class PlayerMove : MonoBehaviour {

    public float speed = 4.0f;
    public float angleSpeed = 90.0f;
    private Transform cam;

    public bool autoMove = false;

    // Use this for initialization
    void Start() {
        cam = Camera.main.transform;
    }

    // Update is called once per frame
    void Update() {
        float vert = Input.GetAxis("Vertical");
        float horiz = Input.GetAxis("Horizontal");

        Vector3 newForward = cam.forward;
        newForward.y = 0.0f;
        newForward.Normalize();

        if (autoMove && vert == 0.0f) {
            vert = 0.5f;
        }

        transform.position += (newForward * vert) * Time.deltaTime * speed;

        transform.Rotate(Vector3.up, horiz * Time.deltaTime * angleSpeed);

    }
}
