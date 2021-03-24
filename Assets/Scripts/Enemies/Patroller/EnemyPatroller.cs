using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatroller : MonoBehaviour, FlashlightNotify, IEnemy {

    GameObject player;
    PlayerController playerController;

    bool playerInLightBox = false;
    NavMeshAgent ai;

    private Vector3 path;
    private Vector3 lookAt;

    private Vector3 spawnLocation;

    public float rotationSpeedWalking = 0.5f;
    public float rotationSpeedRunning = 5f;
    public float walkingSpeed = 2.5f;
    public float runningSpeed = 5f;
    public int patrolAreaSize = 20;

    private TerrainGenerator terrain;

    private StateController stateController;

    private Vector3 nextLocationOnPath;
    private float currentMovingSpeed;
    private Rigidbody rb;
    private float currentRotationSpeed;

    private bool canMove = false;

    void IEnemy.Initialize() {
        spawnLocation = transform.position;

        terrain = GameObject.FindGameObjectWithTag("GameController").GetComponent<TerrainGenerator>();

        rb = GetComponent<Rigidbody>();
        ai = GetComponent<NavMeshAgent>();

        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        stateController = new StateController(this);
    }

    void IEnemy.StartMovement() {
        canMove = true;
    }

    void IEnemy.StopMovement() {
        canMove = false;
    }

    void Update() {
        if (player != null && canMove) {
            CheckIfPlayIsInLight();
            CheckNoiseLevel();
            nextLocationOnPath = ai.steeringTarget;
        }

    }

    void FixedUpdate() {
        if (player != null && canMove) {
            stateController.CurrentState().FixedUpdate();

            Vector3 lookAtPosition = lookAt - transform.position;

            if (lookAtPosition != null && lookAtPosition.sqrMagnitude >= 1f) {
                var targetRotation = Quaternion.LookRotation(lookAtPosition);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, currentRotationSpeed * Time.fixedDeltaTime);
            }

            Vector3 movTo = nextLocationOnPath - transform.position;
            movTo = movTo.normalized * currentMovingSpeed;

            rb.AddForce(movTo);
        }
    }


    private void CheckNoiseLevel() {
        float noise = playerController.GetNoiseLevel();

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= noise) {
            stateController.CurrentState().HearANoise(player.transform.position, distance);
        }

    }

    private void CheckIfPlayIsInLight() {
        if (playerInLightBox) {
            RaycastHit checkCover;
            Ray ray = new Ray(transform.position, player.transform.position - transform.position);
            if (Physics.Raycast(ray, out checkCover, 100)) {
                if (checkCover.collider.gameObject.CompareTag("Player")) {
                    stateController.CurrentState().PlayerEnteredLight();
                } else {
                    stateController.CurrentState().PlayerExitedLight();
                }
            }
        } else {
            stateController.CurrentState().PlayerExitedLight();
        }
    }

    public void SetMovingSpeed(float speed) {
        currentMovingSpeed = speed;
        //ai.speed = speed;
    }

    public Vector3 CurrentVelocity() {
        return rb.velocity;
    }

    public void SetPositionToLookAt(Vector3 newLookAt) {
        lookAt = newLookAt;
    }

    public void SetPath(Vector3 newPath) {
        ai.SetDestination(newPath);
        path = newPath;
    }

    public TerrainGenerator GetTerrain() {
        return terrain;
    }

    public Vector3 GetSpawnLocation() {
        return spawnLocation;
    }

    public Vector3 GetPlayersLocation() {
        return player.transform.position;
    }

    public void PlayerEnteredLight() {
        playerInLightBox = true;
    }

    public void SetCurrentRotationSpeed(float newRotationSpeed) {
        currentRotationSpeed = newRotationSpeed;
    }

    public void PlayerExitedLight() {
        playerInLightBox = false;
    }

    public void Print(string s) {
        print(s);
    }

}
