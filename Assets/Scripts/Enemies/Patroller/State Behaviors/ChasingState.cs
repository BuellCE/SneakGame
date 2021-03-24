using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * Chases the player and attempts to keep the light on it
 * To leave this state the following must happen:
 * Player leaves the AI's light for a few seconds -> investigate State
 */
public class ChasingState : IBehaviorState {

    private float closeSprintMultiplier = 2f;

    private bool hasSightsOnPlayer;
    private int timeWithoutPlayerSight;
    private StateController controller;

    void IBehaviorState.InitializeState(StateController controller) {
        this.controller = controller;
    }

    void IBehaviorState.StateStart() {
        hasSightsOnPlayer = true;
        controller.patroller.SetMovingSpeed(controller.patroller.runningSpeed);
        controller.patroller.SetCurrentRotationSpeed(controller.patroller.rotationSpeedRunning);
    }

    void IBehaviorState.FixedUpdate() {
        controller.patroller.SetPath(controller.patroller.GetPlayersLocation());
        controller.patroller.SetPositionToLookAt(controller.patroller.GetPlayersLocation());

        Vector3 location = controller.patroller.transform.position;
        Vector3 playerLocation = controller.patroller.GetPlayersLocation();

        if (Vector3.Distance(location, playerLocation) <= 2.75f) {
            controller.patroller.SetMovingSpeed(controller.patroller.runningSpeed * closeSprintMultiplier);
        } else {
            controller.patroller.SetMovingSpeed(controller.patroller.runningSpeed);
        }

        if (!hasSightsOnPlayer) {
            UpdateCounter();
        }
    }

    private void UpdateCounter() {
        timeWithoutPlayerSight++;
        if (timeWithoutPlayerSight > 135) {
            controller.SetToInvestigateState();
        }
    }

    void IBehaviorState.PlayerEnteredLight() {
        hasSightsOnPlayer = true;
        timeWithoutPlayerSight = 0;
    }

    void IBehaviorState.PlayerExitedLight() {
        hasSightsOnPlayer = false;
    }


    void IBehaviorState.HearANoise(Vector3 noiseLocation, float distance) {}

}
