using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBehaviorState {

    //The state object is created
    void InitializeState(StateController controller);

    //the state is swapped to
    void StateStart(); 

    //player makes a noise
    void HearANoise(Vector3 noiseLocation, float distance); 

    void PlayerEnteredLight();
    void PlayerExitedLight();
    void FixedUpdate();

}
