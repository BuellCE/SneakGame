using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Randomly patrols the general area the AI was spawned in, attempting to find the player
 * To leave this state one of the following must happen:
 * AI hears a noise -> Investigating State
 * Player enters the AI's light -> Chasing State
 */
public class PatrolState : IBehaviorState {

    private StateController controller;
    private Vector3 targetLocation;
    private Vector3 spawnLocation;
    private List<Vector2> walkableTiles;
    private List<Vector2> nearbyWalkableLocations;
    private int patrolAreaSize;
    private int pathCount;
    private int timeOnCurrentPath;
    private float wallScale;

    Vector3 randomTurnAroundLocation;
    float randomTurningTimer;
    float randomPickNewLocationTimer;
    float randomTurnAroundTimer;

    void IBehaviorState.InitializeState(StateController controller) {
        nearbyWalkableLocations = new List<Vector2>();
        this.controller = controller;
        spawnLocation = controller.patroller.GetSpawnLocation();
        walkableTiles = controller.patroller.GetTerrain().GetMovingAreaTiles();
        wallScale = controller.patroller.GetTerrain().GetWallSize();
        patrolAreaSize = controller.patroller.patrolAreaSize;
        SetCloseTiles();
    }

    void IBehaviorState.StateStart() {
        controller.patroller.SetMovingSpeed(controller.patroller.walkingSpeed);
        controller.patroller.SetCurrentRotationSpeed(controller.patroller.rotationSpeedWalking);
        PickNewLocationToWalkTo();
    }

    void IBehaviorState.HearANoise(Vector3 noiseLocation, float distance) {
        controller.patroller.SetPositionToLookAt(controller.patroller.GetPlayersLocation());
        timeOnCurrentPath = 0;
        controller.SetToInvestigateState();
    }

    void IBehaviorState.PlayerEnteredLight() {
        controller.SetToChasingState();
    }

    void IBehaviorState.FixedUpdate() {
        Vector3 location = controller.patroller.transform.position;

        if (Vector3.Distance(location, targetLocation) <= 5) {
            pathCount++;
            if (pathCount == 1) {
                SetRandomTimers();
            }else if (pathCount > randomPickNewLocationTimer) {
                PickNewLocationToWalkTo();
            } else if (pathCount > randomTurnAroundTimer) {
                controller.patroller.SetPositionToLookAt(controller.patroller.transform.position + randomTurnAroundLocation);
            }
        } else if (timeOnCurrentPath > randomTurningTimer) {
            controller.patroller.SetPositionToLookAt(controller.patroller.transform.position + controller.patroller.CurrentVelocity());
        }


        timeOnCurrentPath++;
    }

    void SetRandomTimers() {
        randomTurnAroundLocation = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
        randomPickNewLocationTimer = Random.Range(100, 200);
        randomTurningTimer = Random.Range(1, 70);
        randomTurnAroundTimer = Random.Range(20, 99);
    }

    void PickNewLocationToWalkTo() {
        pathCount = 0;
        timeOnCurrentPath = 0;
        int random = Random.Range(0, nearbyWalkableLocations.Count - 1);
        Vector2 location = nearbyWalkableLocations[random];
        targetLocation = new Vector3(location.x, 1, location.y);
        controller.patroller.SetPath(targetLocation);
    }

    void IBehaviorState.PlayerExitedLight() { }

    private void SetCloseTiles() {
        Vector2 spawnLocation2d = new Vector2(spawnLocation.x, spawnLocation.z);
        foreach (Vector2 tile in walkableTiles) {
            Vector2 tileLocation = new Vector2(tile.x * wallScale, tile.y * wallScale);
            if (Vector2.Distance(spawnLocation2d, tileLocation) <= patrolAreaSize) {
                nearbyWalkableLocations.Add(tileLocation);
            }
        }
    }

}
