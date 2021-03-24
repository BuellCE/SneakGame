using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightBox : MonoBehaviour {

    FlashlightNotify notify;

    void Start() {
        notify = transform.parent.GetComponent<FlashlightNotify>();
        if (notify == null) {
            Debug.Log("Parent object of FlashlightBox must implement FlashlightNotify interface");
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")){
            notify.PlayerEnteredLight();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            notify.PlayerExitedLight();
        }
    }



}
