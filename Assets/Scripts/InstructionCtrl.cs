using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionCtrl : MonoBehaviour
{
    public GameObject controller;

    // Update is called once per frame
    void Update()
    {
        if (this.isActiveAndEnabled) {
            transform.SetPositionAndRotation(controller.transform.position, controller.transform.rotation);
            transform.parent = controller.transform;
        }
    }
}
