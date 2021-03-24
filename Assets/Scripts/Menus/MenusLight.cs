using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenusLight : MonoBehaviour {

    public float rotateSpeed = 10f;

    void FixedUpdate() {
        this.transform.RotateAround(transform.position, transform.up, Time.fixedDeltaTime * rotateSpeed);
    }
}
