using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    GameObject player;
    private Vector3 startingPosition;
    public Vector3 locationFromPlayer;

    private float moveSpeed = 0.25f;
    private Vector3 moveToLocation;

    void Start() {
        moveToLocation = transform.position;
        startingPosition = transform.position;
    }

    void FixedUpdate() {
        float panSpeed = moveSpeed;
        if (player != null) { 
            panSpeed = 5f;
            moveToLocation = player.transform.position;
        }
        this.transform.position = Vector3.Lerp(this.transform.position, moveToLocation + locationFromPlayer, panSpeed * Time.fixedDeltaTime);
    }

    public void SetMoveLocation(Vector3 newLocation) {
        moveToLocation = newLocation;
        UntargetPlayer();
    }

    public void UntargetPlayer() {
        player = null;
    }

    public void TargetPlayer() {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public Vector3 GetStartingPosition() {
        return startingPosition;
    }
}
