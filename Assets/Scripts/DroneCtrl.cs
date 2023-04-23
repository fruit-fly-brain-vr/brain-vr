using System;
using Meta.WitAi;
using TMPro;
using UnityEngine;

/// <summary>
/// 
/// This class defined the drone mode
/// "don't just drive the tank, become one with the tank" --oversimplified

/// </summary>
public class DroneCtrl : MonoBehaviour
{
    public GameObject imDrone; // the drone is just a collider! -- with a camera attached to it 
                               // userrig needs to rescale
    public GameObject modePanel;
    public TextMeshProUGUI modeMsg;

    //public GameObject miniMap;

    private Vector3 preDronePos = Vector3.one;
    private Quaternion preDroneRot = Quaternion.identity;

    private bool droneMode = false;
    private bool right_index = false;
    private bool right_hand = false;
    private bool left_index = false;
    private bool left_hand = false;

    void Start()
    {
        modePanel.SetActive(false);
    }

    void Update()
    {
        TriggerListener();
        DroneMockInterface();
        OperateDrone();
    }


    /// <summary>
    /// ABOVE THIS LINE ARE METHODS FOR UI FOR TOOL SELECTION
    /// BELOW THIS LINE ARE METHODS FOR INTERACTION TOOLS
    /// </summary>

    void TriggerListener()
    {
        right_index = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) > 0.9f;
        right_hand = OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger) > 0.9f;
        left_index = OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger) > 0.9f;
        left_hand = OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger) > 0.9f;

    }

    void DroneMockInterface()
    {
        if (OVRInput.Get(OVRInput.RawButton.Y))
        {
            if (!droneMode) {
                droneMode = true;
                preDronePos = imDrone.transform.position;
                preDroneRot = imDrone.transform.rotation;
                ShowModeMsg("started drone");
            }
            
        }
        else if (OVRInput.Get(OVRInput.RawButton.X))
        {
            ExitDrone();
        }
    }

    // this method updates the displayed message and instruction
    void ShowModeMsg(string theMsg)
    {
        modePanel.SetActive(true);
        modeMsg.text = theMsg;
    }

    void ExitDrone() {
        if (droneMode) {
            imDrone.transform.localScale = Vector3.one;
            imDrone.transform.position = preDronePos;
            imDrone.transform.rotation = preDroneRot;
            droneMode = false;
            ShowModeMsg("exited drone");
        }
    }

    // "don't just drive the tank, become one with the tank" --oversimplified
    // just position and rotation change here 
    // fix orientation to the headset for view, but does not change primary movement directions
    // left thumbstick control position translation, strictly left and right and up and down
    // the triggers are for forwards and backwards
    // right thumbstick for rotations (left and right only) 
    // first person viewing angel is strictly attached to userRig, and does not interfere with drone movement
    // this is basically the broom control logic from hogwartz legacy
    void OperateDrone()
    {
        if (droneMode) {
            //move up and down
            if (OVRInput.Get(OVRInput.RawButton.LThumbstickUp))
            {
                imDrone.transform.position += 3 * imDrone.transform.up * Time.deltaTime;
            }
            else if (OVRInput.Get(OVRInput.RawButton.LThumbstickDown))
            {
                imDrone.transform.position -= 3 * imDrone.transform.up * Time.deltaTime;
            }

            //pan left and right
            if (OVRInput.Get(OVRInput.RawButton.LThumbstickRight))
            {
                imDrone.transform.position += 3 * imDrone.transform.right * Time.deltaTime;
            }
            else if (OVRInput.Get(OVRInput.RawButton.LThumbstickLeft))
            {
                imDrone.transform.position -= 3 * imDrone.transform.right * Time.deltaTime;
            }

            //forward and back
            if (OVRInput.Get(OVRInput.RawButton.RThumbstickUp))
            {
                imDrone.transform.position += 3 * imDrone.transform.forward * Time.deltaTime;
            }
            else if (OVRInput.Get(OVRInput.RawButton.RThumbstickDown))
            {
                imDrone.transform.position -= 3 * imDrone.transform.forward * Time.deltaTime;
            }

            //look left and right
            if (OVRInput.Get(OVRInput.RawButton.RThumbstickRight))
            {
                imDrone.transform.RotateAround(imDrone.transform.position, imDrone.transform.up, 10 * Time.deltaTime);
            }
            else if (OVRInput.Get(OVRInput.RawButton.RThumbstickLeft))
            {
                imDrone.transform.RotateAround(imDrone.transform.position, imDrone.transform.up, -10 * Time.deltaTime);
            }

            // scale my size ahhhhhh
            // problem: is scaling with respect to origin somehow...'
            // need to scale with first person
            if (right_index)
            {
                imDrone.transform.localScale *= (1 + 1 * Time.deltaTime);
            }
            else if (left_index)
            {
                imDrone.transform.localScale *= (1 - 1 * Time.deltaTime);
            }

            ShowModeMsg("drone position:\n" +
                            imDrone.transform.position.x.ToString("F3") + ",\n" +
                            imDrone.transform.position.y.ToString("F3") + ",\n" +
                            imDrone.transform.position.z.ToString("F3") + ".\n" +
                        "drone scale: \n" +
                            imDrone.transform.localScale.x.ToString("F3"));

        }

    }


}

