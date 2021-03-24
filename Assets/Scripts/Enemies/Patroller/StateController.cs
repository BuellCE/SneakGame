using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Handles the operating states of an EnemyPatroller which decides how the AI will act
//Uses the State Design Pattern
public class StateController{

    public EnemyPatroller patroller;

    IBehaviorState currentState;

    //All Possible States
    public PatrolState patrollingState = new PatrolState();
    public ChasingState chasingState = new ChasingState();
    public InvestigateState investigatingState = new InvestigateState();

    public StateController(EnemyPatroller patroller) {
        this.patroller = patroller;
        ((IBehaviorState) patrollingState).InitializeState(this);
        ((IBehaviorState) chasingState).InitializeState(this);
        ((IBehaviorState) investigatingState).InitializeState(this);

        ChangeState(patrollingState);
    }

    private void StateStart() {
        currentState.StateStart();
    }

    public IBehaviorState CurrentState() {
        return currentState;
    }

    public void SetToPatrolState() {
        ChangeState(patrollingState);
    }

    public void SetToInvestigateState() {
        ChangeState(investigatingState);
    }

    public void SetToChasingState() {
        ChangeState(chasingState);
    }

    public void SetToRefindState() {

    }

    private void ChangeState(IBehaviorState newState) {
        currentState = newState;
        StateStart();
    }

}
