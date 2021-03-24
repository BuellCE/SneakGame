using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {

    [SerializeField]
    private float walkingForce = 20;
    [SerializeField]
    private float sprintingForce = 30;

    private Rigidbody rigidBody;

    private Vector3 moveDirection;

    private float noiseFactorWalking = 1.5f;
    private float noiseFactorSprinting = 1.5f;
    private float currentNoiseLevel;

    public Light pointLight;

    private GamePlayMap map;
    private GameController controller;

    public void Initialize() {
        map = GameObject.FindGameObjectWithTag("Map").GetComponent<GamePlayMap>();
        controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        rigidBody = GetComponent<Rigidbody>();
    }

    public float GetNoiseLevel() {
        return currentNoiseLevel;
    }

    void Update() {
        KeyboardMovement();
        PaintOnMap();
    }

    public void PaintOnMap() {
        map.PaintPlayer(transform.position);
    }

    public void KeyboardMovement() {

        if (controller.CanPlayerControl()) {

            float speed = rigidBody.velocity.magnitude;

            currentNoiseLevel = speed * noiseFactorWalking;

            Vector3 movement = new Vector3();

            if (Input.GetKey(KeyCode.A)) movement.x -= 1;
            if (Input.GetKey(KeyCode.D)) movement.x += 1;
            if (Input.GetKey(KeyCode.W)) movement.z += 1;
            if (Input.GetKey(KeyCode.S)) movement.z -= 1;

            if (Input.GetKey(KeyCode.LeftShift)) {
                currentNoiseLevel *= noiseFactorSprinting;
                moveDirection = movement.normalized * sprintingForce;
            } else {
                moveDirection = movement.normalized * walkingForce;
            }

        }
    }

    public void MouseMovement() {
        Vector3 moveTo = Vector3.zero;

        if (Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                moveTo = hit.point;

                if (hit.transform != this.transform) {

                    float distance = Vector3.Distance(moveTo, this.transform.position);

                    Vector3 movement2 = moveTo - this.transform.position;
                    moveDirection = movement2.normalized * walkingForce;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Patroller") || other.CompareTag("Pursuer")) {
            controller.RestartLevel();
        }else if (other.CompareTag("EndPoint")) {
            controller.FinishLevel();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Patroller")) {
            print("Eh");
        }
    }


    void FixedUpdate() {
        rigidBody.AddForce(moveDirection);
    }

}
