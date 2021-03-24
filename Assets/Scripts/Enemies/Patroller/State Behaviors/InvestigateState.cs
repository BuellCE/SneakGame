using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
 * AI will investigate noises made by the player in attempt to find the player
 * To leave this state one of the following must happen:
 * Player enters the AI's light -> Chasing State
 * AI is done investigating the area where the sound occured -> Patrol State
 */
public class InvestigateState : IBehaviorState {

    private const int minimumSecondsToSpendInvestigating = 15;
    private const int maximumSecondsToSpendInvestigating = 30;

    private StateController controller;
    private Vector3 goToLocation;
    private int timeSpentInvestigating;
    private int maxTimeToSpendInvestigating;
    private int timeSpentNotMoving;
    private int randomMaxTimeNotSpentMoving;

    void IBehaviorState.InitializeState(StateController controller) {
        this.controller = controller;
    }

    void IBehaviorState.StateStart() {
        SetNewInvestigatingLocation(controller.patroller.GetPlayersLocation());
    }

    void IBehaviorState.FixedUpdate() {
        Debug.DrawLine(controller.patroller.transform.position, goToLocation, Color.red);
        CheckDistanceToPlayer();
        CheckDistanceToMovingLocation();
        UpdateTotalTimeInvestigating();
    }

    private void CheckDistanceToPlayer() {
        Vector3 playerLoc = controller.patroller.GetPlayersLocation();

        if (Vector3.Distance(controller.patroller.transform.position, playerLoc) <= 3) {
            SetNewInvestigatingLocation(playerLoc);
        }
    }

    private void CheckDistanceToMovingLocation() {
        if (Vector3.Distance(controller.patroller.transform.position, goToLocation) <= 2) {
            timeSpentNotMoving++;
            if (timeSpentNotMoving > randomMaxTimeNotSpentMoving) {
                PickAnotherLocationAroundNoise();
            } else if (timeSpentNotMoving == 1) {
                SetRandomSpotToLookAt();
            }
        } else {
            controller.patroller.SetPositionToLookAt(controller.patroller.transform.position + controller.patroller.CurrentVelocity());
        }
    }

    private void UpdateTotalTimeInvestigating() {
        if (timeSpentInvestigating > maxTimeToSpendInvestigating) {
            controller.SetToPatrolState();
        } else {
            timeSpentInvestigating++;
        }
    }

    private void PickAnotherLocationAroundNoise() {
        goToLocation = GameUtility.GetRandomNavMeshPositionNearby(controller.patroller.transform.position, 10);
        timeSpentNotMoving = 0;
        UpdatePath();
    }

    private void UpdatePath() {
        randomMaxTimeNotSpentMoving = Random.Range(40, 100);
        controller.patroller.SetPath(goToLocation);
    }

    private void SetRandomSpotToLookAt() {

        bool hasSight = GameUtility.HasLineOfSightWithTaggedObject(
            controller.patroller.transform.position,
            controller.patroller.GetPlayersLocation(),
            "Player");

        if (hasSight && Random.Range(0,2) == 1) {
            controller.patroller.SetPositionToLookAt(controller.patroller.GetPlayersLocation());
        } else {
            Vector3 lookAt = GameUtility.GetRandomNavMeshPositionNearby(controller.patroller.GetPlayersLocation(), 10);
            controller.patroller.SetPositionToLookAt(lookAt);
        }
    }

    private void SetNewInvestigatingLocation(Vector3 noiseLocation) {
        goToLocation = noiseLocation;
        timeSpentInvestigating = 0;
        maxTimeToSpendInvestigating = Random.Range(minimumSecondsToSpendInvestigating * 50, maximumSecondsToSpendInvestigating * 50);
        UpdatePath();
    }


    void IBehaviorState.PlayerEnteredLight() {
        controller.SetToChasingState();
    }

    void IBehaviorState.HearANoise(Vector3 noiseLocation, float distance) {
        SetNewInvestigatingLocation(noiseLocation);
    }

    void IBehaviorState.PlayerExitedLight() {}

}
