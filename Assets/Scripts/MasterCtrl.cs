using System;
using Meta.WitAi;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Oculus.Interaction.UnityCanvas.CanvasRenderTexture;

/// <summary>
/// 
/// This class includes obejct manipulation methods using Rays
/// 
/// </summary>
public class MasterCtrl : MonoBehaviour
{

    public GameObject leftController;
    public GameObject rightController;
    public GameObject userRig;
    public GameObject leftClipper;
    public GameObject rightClipper;
    public GameObject imDrone;
    public GameObject miniMap;
    public GameObject birdCamDisplay;
    public GameObject eb;

    public Camera birdCam;

    public Material highlightMat;
    public Material lineMat;

    // UI elements
    public GameObject leftToolMenu;
    public GameObject rightToolMenu;
    public GameObject pinpointDisplay;
    public GameObject indicatorBall;
    public GameObject restartPanel;
    public TextMeshPro pinpointDisplayMsg;
    public GameObject instructionPanel;
    public TextMeshProUGUI instructionMsg;
    public GameObject modePanel;
    public TextMeshProUGUI modeMsg;
    public GameObject dronePanel;
    public TextMeshProUGUI droneMsg;


    // bools related to tool modes
    private bool leftToolMenuOpen = false;
    private bool rightToolMenuOpen = false;
    //private bool pinpointSelectMode = false;
    private bool singleRayGrabZoomMode = false; // these three tools' menuare on the left controller
    private bool twoRayDragZoomMode = false;
    private bool droneMode = false;
    private bool singleRayGrabCloseFarMode = false; // this is a tool whose menu is on the right controller
    private bool singleRayDepthCamMode = false;
    private bool sliceMode = false;
    

    // helper var for tool use
    private GameObject currentObj; // the object currently registered for selection
    private GameObject cameraIndicatorBall = null; // this object is only used for single ray move camera; can't find a way around it
    private bool selected = false; // this is an flag set true when object is being manipuated by some tool
    private bool isDrone = false;
    private bool isRestart = false;
    private float twoPointDistance = 0;
    private Vector3 preDronePos = Vector3.one;
    private Quaternion preDroneRot = Quaternion.identity;

    // private bools for trigger controls
    private bool right_index = false;
    private bool right_hand = false;
    private bool left_index = false;
    private bool left_hand = false;
    

    void Start()
    {
        ShowInstructionMsg("Interaction Demo open Left/Right Tool Menu by\n pressing Left/Right Thunbstick");
        leftToolMenu.SetActive(false);
        rightToolMenu.SetActive(false);
        pinpointDisplay.SetActive(false);
        modePanel.SetActive(false);
        birdCamDisplay.SetActive(false);
        birdCam.enabled = false;
        miniMap.SetActive(false);
        leftClipper.SetActive(false);
        rightClipper.SetActive(false);
        dronePanel.SetActive(false);
        restartPanel.SetActive(false);
    }

    void Update()
    {
        TriggerListener();
        IndexTriggerRay();
        OpenMenu();
        ModeListener();
        Restart();
    }


    // make both left and right tool menus
    void OpenMenu()
    {
        leftToolMenu.SetActive(leftToolMenuOpen);
        rightToolMenu.SetActive(rightToolMenuOpen);
        if (leftToolMenuOpen)
        {
            SelectTool();
        }
        else if (rightToolMenuOpen)
        {
            SelectTool();
        }
        else if (OVRInput.GetDown(OVRInput.RawButton.LThumbstick))
        {
            leftToolMenuOpen = true;
            rightToolMenuOpen = false;
            modePanel.SetActive(false);
            leftToolMenu.transform.SetPositionAndRotation(leftController.transform.position, leftController.transform.rotation);
            leftToolMenu.transform.parent = leftController.transform;
        }
        else if (OVRInput.GetDown(OVRInput.RawButton.RThumbstick))
        {
            rightToolMenuOpen = true;
            leftToolMenuOpen = false;
            modePanel.SetActive(false);
            rightToolMenu.transform.SetPositionAndRotation(rightController.transform.position, rightController.transform.rotation);
            rightToolMenu.transform.parent = rightController.transform;
        }

    }

    // sets booleans for tools
    void SelectTool()
    {
        if (leftToolMenuOpen)
        {
            if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickUp))
            {
                singleRayGrabZoomMode = true;
                //pinpointSelectMode = false;
                twoRayDragZoomMode = false;
                leftToolMenuOpen = false;
                singleRayGrabCloseFarMode = false;
                singleRayDepthCamMode = false;
                sliceMode = false;
                droneMode = false;
                ShowModeMsg("Single Ray Select");
                ShowInstructionMsg("...insert rule for this mode here...");
            }
            else if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickLeft))
            {
                droneMode = true;
                singleRayGrabZoomMode = false;
                twoRayDragZoomMode = false;
                leftToolMenuOpen = false;
                singleRayGrabCloseFarMode = false;
                singleRayDepthCamMode = false;
                sliceMode = false;
                ShowModeMsg("Drone Mode");
                ShowInstructionMsg("...insert rule for this mode here...");
            }
            else if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickRight))
            {
                twoRayDragZoomMode = true;
                singleRayGrabZoomMode = false;
                //pinpointSelectMode = false;
                leftToolMenuOpen = false;
                singleRayGrabCloseFarMode = false;
                singleRayDepthCamMode = false;
                sliceMode = false;
                droneMode = false;
                ShowModeMsg("2 Ray Drag Zoom");
                ShowInstructionMsg("...insert rule for this mode here...");
            }
        }
        else if (rightToolMenuOpen)
        {
            if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickUp))
            {
                singleRayGrabCloseFarMode = true;
                singleRayGrabZoomMode = false;
                //pinpointSelectMode = false;
                twoRayDragZoomMode = false;
                rightToolMenuOpen = false;
                singleRayDepthCamMode = false;
                sliceMode = false;
                droneMode = false;
                ShowModeMsg("Single Ray Grab and Move Close or Far"); // need better name
                ShowInstructionMsg("...insert rule for this mode here...");
            }
            else if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickLeft))
            {
                singleRayDepthCamMode = true;
                //pinpointSelectMode = false;
                singleRayGrabZoomMode = false;
                twoRayDragZoomMode = false;
                rightToolMenuOpen = false;
                singleRayGrabCloseFarMode = false;
                sliceMode = false;
                droneMode = false;
                ShowModeMsg("Single Ray Depth Move Camera)");
                ShowInstructionMsg("...insert rule for this mode here...");
            }
            else if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickRight))
            {
                sliceMode = true;
                twoRayDragZoomMode = false;
                singleRayGrabZoomMode = false;
                //pinpointSelectMode = false;
                rightToolMenuOpen = false;
                singleRayGrabCloseFarMode = false;
                singleRayDepthCamMode = false;
                droneMode = false;
                ShowModeMsg("Slicer");
                ShowInstructionMsg("Hold both triggers on either controller and move to slice");
            }

        }

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
    void ModeListener()
    {
        if (droneMode)
        {
            Drone();
        }
        else if (singleRayGrabZoomMode)
        {
            SingleRayGrabZoom();

        }
        else if (twoRayDragZoomMode)
        {
            TwoRayZoom();
        }
        else if (singleRayGrabCloseFarMode)
        {
            SingleRayGrabCloseFar();
        }
        else if (singleRayDepthCamMode)
        {
            SingleRayDepthCam();
        }
        else if (sliceMode)
        {
            Slice();
        }
        else
        {
            // these are just extra protection, probably not nessary
            selected = false;
            ShowInstructionMsg("Interaction Demo open Left/Right Tool Menu by\n pressing Left/Right Thunbstick");
        }

    }

    // this method updates the displayed message and instruction
    void ShowModeMsg(string theMsg)
    {
        modePanel.SetActive(true);
        modeMsg.text = theMsg;
    }
    // this method updates the displayed message and instruction
    void ShowInstructionMsg(string theMsg)
    {
        instructionPanel.SetActive(true);
        instructionMsg.text = theMsg;
    }

    // restart function
    void Restart()
    {
        if (OVRInput.Get(OVRInput.RawButton.A))
        {
            restartPanel.SetActive(true);
            isRestart = true;
        }
        if (OVRInput.Get(OVRInput.RawButton.Y) & isRestart)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else if (OVRInput.Get(OVRInput.RawButton.X) & isRestart)
        {
            isRestart = false;
            restartPanel.SetActive(false);
        }

    }

    /// <summary>
    /// Below this line is strictly interaction methods
    /// </summary>


    // single ray drag zoom mode
    // shoot ray, translate around (drag)
    // move thumbstick up and down to scale up and down (zoom)
    void SingleRayGrabZoom()
    {
        if (right_index & right_hand)
        {
            SingleRayGrabZoomEachHand(rightController);
        }
        else if (left_index & left_hand)
        {
            SingleRayGrabZoomEachHand(leftController);
        }
        else
        {
            selected = false;
            currentObj.transform.parent = null;
            currentObj = null;
        }
    }
    void SingleRayGrabZoomEachHand(GameObject controller)
    {

        if (Physics.Raycast(controller.transform.position, controller.transform.forward, out RaycastHit hit, 3))
        {
            // lock on, and maintail until drop 
            if (hit.transform.gameObject.CompareTag("ball"))
            {
                selected = true;
                currentObj = hit.transform.gameObject;
                currentObj.transform.parent = controller.transform;

                GameObject theIndicatorBall = Instantiate(indicatorBall, hit.point, Quaternion.identity);
                Destroy(theIndicatorBall, 0.02f);

                if (controller == rightController)
                {
                    if (OVRInput.Get(OVRInput.RawButton.RThumbstickUp))
                    {
                        currentObj.transform.localScale = Vector3.Lerp(currentObj.transform.localScale,
                            currentObj.transform.localScale * (1 + 10 * Time.deltaTime), 0.1f);
                    }
                    else if (OVRInput.Get(OVRInput.RawButton.RThumbstickDown))
                    {
                        currentObj.transform.localScale = Vector3.Lerp(currentObj.transform.localScale,
                            currentObj.transform.localScale * (1 - 10 * Time.deltaTime), 0.1f);
                    }
                }
                else if (controller == leftController)
                {
                    if (OVRInput.Get(OVRInput.RawButton.LThumbstickUp))
                    {
                        currentObj.transform.localScale = Vector3.Lerp(currentObj.transform.localScale,
                            currentObj.transform.localScale * (1 + 10 * Time.deltaTime), 0.1f);
                    }
                    else if (OVRInput.Get(OVRInput.RawButton.LThumbstickDown))
                    {
                        currentObj.transform.localScale = Vector3.Lerp(currentObj.transform.localScale,
                            currentObj.transform.localScale * (1 - 10 * Time.deltaTime), 0.1f);
                    }
                }
            }
        }
    }


    // single ray drag zoom mode
    // shoot ray, translate around (drag)
    // move thumbstick up and down to move object closer or further to user (closefar) -- stupid name... distance?
    // (ironicall visually this could look very similar to the effect of scaling with a single ray)
    void SingleRayGrabCloseFar()
    {
        if (right_index & right_hand)
        {
            SingleRayGrabCloseFarEachHand(rightController);
        }
        else if (left_index & left_hand)
        {
            SingleRayGrabCloseFarEachHand(leftController);
        }
        else
        {
            selected = false;
            currentObj.transform.parent = null;
            currentObj = null;
        }
    }
    void SingleRayGrabCloseFarEachHand(GameObject controller)
    {
        if (Physics.Raycast(controller.transform.position, controller.transform.forward, out RaycastHit hit, 3))
        {
            // lock on, and maintail until drop 
            if (hit.transform.gameObject.CompareTag("ball"))
            {
                selected = true;
                currentObj = hit.transform.gameObject;
                currentObj.transform.parent = controller.transform;

                GameObject theIndicatorBall = Instantiate(indicatorBall, hit.point, Quaternion.identity);
                Destroy(theIndicatorBall, 0.02f);

                if (controller == rightController)
                {
                    if (OVRInput.Get(OVRInput.RawButton.RThumbstickUp))
                    {
                        currentObj.transform.localPosition *= (1 + 1 * Time.deltaTime);//move further
                    }
                    else if (OVRInput.Get(OVRInput.RawButton.RThumbstickDown))
                    {
                        currentObj.transform.localPosition *= (1 - 1 * Time.deltaTime);// move closer
                    }
                }
                else if (controller == leftController)
                {
                    if (OVRInput.Get(OVRInput.RawButton.LThumbstickUp))
                    {
                        currentObj.transform.localPosition *= (1 + 1 * Time.deltaTime);
                    }
                    else if (OVRInput.Get(OVRInput.RawButton.LThumbstickDown))
                    {
                        currentObj.transform.localPosition *= (1 - 1 * Time.deltaTime);
                    }
                }
            }
        }
    }


    //// 3D select mode with a single ray
    //// bool check: singleRaydDepth
    void SingleRayDepthCam()
    {
        if (!selected)
        {
            if (right_index & right_hand)
            {
                cameraIndicatorBall = Instantiate(indicatorBall, rightController.transform.position, Quaternion.identity);
                cameraIndicatorBall.transform.parent = rightController.transform;
                cameraIndicatorBall.transform.localPosition = new Vector3(0f, 0f, 0.5f);

                birdCam.enabled = true;
                birdCam.transform.SetPositionAndRotation(cameraIndicatorBall.transform.position, Quaternion.Euler(90, userRig.transform.rotation.eulerAngles.y, 0));
                birdCam.transform.parent = cameraIndicatorBall.transform.transform;
                birdCam.transform.localPosition = new Vector3(0, 1, 0);

                birdCamDisplay.SetActive(true);
                pinpointDisplay.SetActive(true);

                miniMap.SetActive(true);
                miniMap.transform.SetPositionAndRotation(leftController.transform.position + new Vector3(0, 0.1f * imDrone.transform.localScale.magnitude, 0), leftController.transform.rotation); ;
                miniMap.transform.parent = leftController.transform;

                selected = true;
            }
            else if (left_index & left_hand)
            {
                cameraIndicatorBall = Instantiate(indicatorBall, leftController.transform.position, Quaternion.identity);
                cameraIndicatorBall.transform.parent = leftController.transform;
                cameraIndicatorBall.transform.localPosition = new Vector3(0f, 0f, 0.5f);

                birdCam.enabled = true;
                birdCam.transform.SetPositionAndRotation(cameraIndicatorBall.transform.position, Quaternion.Euler(90, userRig.transform.rotation.eulerAngles.y, 0));
                birdCam.transform.parent = cameraIndicatorBall.transform.transform;
                birdCam.transform.localPosition = new Vector3(0, 1, 0);

                birdCamDisplay.SetActive(true);
                pinpointDisplay.SetActive(true);

                miniMap.SetActive(true);
                miniMap.transform.SetPositionAndRotation(rightController.transform.position + new Vector3(0, 0.1f * imDrone.transform.localScale.magnitude, 0), rightController.transform.rotation); ;
                miniMap.transform.parent = rightController.transform;

                selected = true;
            }
        }
        else
        {
            if (right_index & right_hand)
            {
                SingleRayDepthCamEachHand(rightController, cameraIndicatorBall);
            }
            else if (left_index & left_hand)
            {
                SingleRayDepthCamEachHand(leftController, cameraIndicatorBall);
            }
            else
            {
                selected = false;
                pinpointDisplay.SetActive(false);
                birdCam.transform.parent = null;
                birdCam.enabled = false;
                birdCamDisplay.SetActive(false);
                miniMap.SetActive(false);   
                Destroy(cameraIndicatorBall);
            }
        }
    }
    void SingleRayDepthCamEachHand(GameObject controller, GameObject ib)
    {
        if (controller == rightController)
        {
            if (OVRInput.Get(OVRInput.RawButton.RThumbstickUp))
            {
                ib.transform.localPosition *= (1 + 1 * Time.deltaTime);
            }
            else if (OVRInput.Get(OVRInput.RawButton.RThumbstickDown))
            {
                ib.transform.localPosition *= (1 - 1 * Time.deltaTime);
            }
        }
        else if (controller == leftController)
        {
            if (OVRInput.Get(OVRInput.RawButton.LThumbstickUp))
            {
                ib.transform.localPosition *= (1 + 1 * Time.deltaTime);
            }
            else if (OVRInput.Get(OVRInput.RawButton.LThumbstickDown))
            {
                ib.transform.localPosition *= (1 - 1 * Time.deltaTime);
            }
        }

        if (controller == rightController)
        {
            if (OVRInput.Get(OVRInput.RawButton.LThumbstickUp))
            {
                birdCam.transform.localPosition *= (1 + 1 * Time.deltaTime);
            }
            else if (OVRInput.Get(OVRInput.RawButton.LThumbstickDown))
            {
                birdCam.transform.localPosition *= (1 - 1 * Time.deltaTime);
            }
        }
        else if (controller == leftController)
        {
            if (OVRInput.Get(OVRInput.RawButton.RThumbstickUp))
            {
                birdCam.transform.localPosition *= (1 + 1 * Time.deltaTime);
            }
            else if (OVRInput.Get(OVRInput.RawButton.RThumbstickDown))
            {
                birdCam.transform.localPosition *= (1 - 1 * Time.deltaTime);
            }
        }

        pinpointDisplay.transform.SetPositionAndRotation(ib.transform.position, Quaternion.Euler(0f, userRig.transform.rotation.eulerAngles.y, 0f));
        pinpointDisplayMsg.text = "Camera Depth " + ib.transform.localPosition.z.ToString("F2") + "m\n" +
            "Camera Height " + birdCam.transform.localPosition.y.ToString("F2") + "m";

        Vector3 babyIbPos = ib.transform.position-eb.transform.position;
        float babyIbScale = miniMap.transform.lossyScale.magnitude/eb.transform.lossyScale.magnitude;
        GameObject theBabyIndicatorBall = Instantiate(indicatorBall, miniMap.transform.position+babyIbPos*babyIbScale, Quaternion.identity);
        //theBabyIndicatorBall.transform.localScale *= babyIbScale;
        Destroy(theBabyIndicatorBall, 0.02f);

    }


    // two ray zoom move
    // cast two rays onto object and scale the size of object based on distance of
    // left and right ray cast hit points
    void TwoRayZoom()
    {

        if (right_index & right_hand & left_index & left_hand)
        {
            if (Physics.Raycast(leftController.transform.position, leftController.transform.forward, out RaycastHit hitL, 3) &
            Physics.Raycast(rightController.transform.position, rightController.transform.forward, out RaycastHit hitR, 3))
            {

                if (hitL.transform.gameObject.CompareTag("ball") & hitR.transform.gameObject.CompareTag("ball"))
                {
                    GameObject leftIndicatorBall = Instantiate(indicatorBall, hitL.point, Quaternion.identity);
                    GameObject rightIndicatorBall = Instantiate(indicatorBall, hitR.point, Quaternion.identity);
                    Destroy(leftIndicatorBall, 0.02f);
                    Destroy(rightIndicatorBall, 0.02f);

                    if (!selected)
                    {
                        twoPointDistance = (hitL.point - hitR.point).magnitude;
                        selected = true;

                    }
                    currentObj = hitL.transform.gameObject;

                    if (selected & twoPointDistance > 0)
                    {
                        currentObj.transform.localScale = Vector3.Lerp(currentObj.transform.localScale,
                    ((hitL.point - hitR.point).magnitude) * currentObj.transform.localScale / twoPointDistance, 0.05f);
                    }
                }
            }
        }
        else
        {
            selected = false;
            currentObj = null;
            twoPointDistance = 0;
        }
    }

    //// 3D select mode with two rays
    //// return the position/coord of the intersection of rays
    //// figure out later what to do with this information
    //// bool check: pinpointSelectMode
    //void PinpointSelect(){

    //    if (right_index & right_hand & left_index & left_hand)
    //    {
    //        GameObject rr = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //        rr.transform.localScale = new Vector3(0.01f, 0.01f, 3);
    //        rr.transform.SetPositionAndRotation(rightController.transform.position, rightController.transform.rotation);
    //        rr.transform.parent = rightController.transform;
    //        rr.transform.localPosition = new Vector3(0f, 0f, 1.5f);
    //        Destroy(rr.GetComponent<MeshRenderer>());
    //        rr.tag = "rightLine";

    //        if (Physics.Raycast(leftController.transform.position, leftController.transform.forward, out RaycastHit hit, 3))
    //        {
    //            if (hit.transform.gameObject.CompareTag("rightLine"))
    //            {
    //                selected = true;
    //                pinpointDisplay.SetActive(true);
    //                pinpointDisplay.transform.SetPositionAndRotation(hit.point, Quaternion.Euler(0f, userRig.transform.rotation.eulerAngles.y, 0f));
    //                GameObject theIndicatorBall = Instantiate(indicatorBall, hit.point, Quaternion.Euler(0, userRig.transform.rotation.eulerAngles.y, 0));
    //                Destroy(theIndicatorBall, 0.02f);

    //                birdCam.enabled = true;
    //                birdCamDisplay.SetActive(true);
    //                birdCam.transform.position = new Vector3(hit.point.x, birdCam.transform.position.y, hit.point.z);
    //                birdCam.transform.rotation = Quaternion.Euler(90, userRig.transform.rotation.eulerAngles.y, 0);  // make it point down

    //                //this does not work because camera position keeps being reset and it is hard to track
    //                if (OVRInput.Get(OVRInput.RawButton.LThumbstickUp) | OVRInput.Get(OVRInput.RawButton.RThumbstickUp))
    //                {
    //                    birdCam.transform.position += 3 * Time.deltaTime * Vector3.up;
    //                }
    //                else if (OVRInput.Get(OVRInput.RawButton.LThumbstickDown) | OVRInput.Get(OVRInput.RawButton.RThumbstickDown))
    //                {
    //                    birdCam.transform.position += 3 * Time.deltaTime * Vector3.down;
    //                }

    //                pinpointDisplayMsg.text = "camera position:\n"+
    //                    birdCam.transform.position.x.ToString("F3") + ",\n" +
    //                    birdCam.transform.position.y.ToString("F3") + ",\n" +
    //                    birdCam.transform.position.z.ToString("F3");

    //            }
    //            else
    //            {
    //                selected = false;
    //            }
    //        }
    //        Destroy(rr, 3f);
    //    }
    //    else
    //    {
    //        selected = false;
    //        pinpointDisplay.SetActive(false);
    //        birdCam.enabled = false;
    //        birdCamDisplay.SetActive(false);
    //    }
    //}

    // slice mode stuff
    // want to allow both hands to slice at the same time
    void Slice()
    {
        if (right_index & right_hand)
        {
            if (!rightClipper.activeSelf)
            {
                rightClipper.SetActive(true);
                rightClipper.transform.SetLocalPositionAndRotation(rightController.transform.position, rightController.transform.rotation);
                rightClipper.transform.parent = rightController.transform;
            }
        }
        else
        {
            rightClipper.transform.parent = null;
            rightClipper.SetActive(false);
        }
        if (left_index & left_hand)
        {
            if (!leftClipper.activeSelf)
            {
                leftClipper.SetActive(true);
                leftClipper.transform.SetLocalPositionAndRotation(leftController.transform.position, leftController.transform.rotation);
                leftClipper.transform.parent = leftController.transform;
            }
        }
        else
        {
            leftClipper.transform.parent = null;
            leftClipper.SetActive(false);
            // the plane under the clipper is not disabled somehow...
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
    void Drone()
    {
        if (!isDrone)
        {
            isDrone = true;
            preDronePos = imDrone.transform.position;
            preDroneRot = imDrone.transform.rotation;
            dronePanel.SetActive(true);
        }
        else
        {
            OperateDrone();
            miniMap.SetActive(true);
            miniMap.transform.SetPositionAndRotation(leftController.transform.position + new Vector3(0, 0.1f * imDrone.transform.localScale.magnitude, 0), leftController.transform.rotation); ;
            miniMap.transform.parent = leftController.transform;

            if ((OVRInput.Get(OVRInput.RawButton.X)))
            {
                miniMap.SetActive(false);
                imDrone.transform.localScale = Vector3.one;
                imDrone.transform.SetPositionAndRotation(preDronePos, preDroneRot);
                isDrone = false;
                droneMode = false;
                dronePanel.SetActive(false);
            }
            
        }
    }
    void OperateDrone()
    {
        //move up and down
        if (OVRInput.Get(OVRInput.RawButton.LThumbstickUp))
        {
            imDrone.transform.position += 3 * Time.deltaTime * imDrone.transform.up;
        }
        else if (OVRInput.Get(OVRInput.RawButton.LThumbstickDown))
        {
            imDrone.transform.position -= 3 * Time.deltaTime * imDrone.transform.up;
        }

        //pan left and right
        if (OVRInput.Get(OVRInput.RawButton.LThumbstickRight))
        {
            imDrone.transform.position += 3 * Time.deltaTime * imDrone.transform.right;
        }
        else if (OVRInput.Get(OVRInput.RawButton.LThumbstickLeft))
        {
            imDrone.transform.position -= 3 * Time.deltaTime * imDrone.transform.right;
        }

        //forward and back
        if (OVRInput.Get(OVRInput.RawButton.RThumbstickUp))
        {
            imDrone.transform.position += 3 * Time.deltaTime * imDrone.transform.forward;
        }
        else if (OVRInput.Get(OVRInput.RawButton.RThumbstickDown))
        {
            imDrone.transform.position -= 3 * Time.deltaTime * imDrone.transform.forward;
        }

        //look left and right
        if (OVRInput.Get(OVRInput.RawButton.RThumbstickRight))
        {
            imDrone.transform.RotateAround(imDrone.transform.position, imDrone.transform.up, 30 * Time.deltaTime);
        }
        else if (OVRInput.Get(OVRInput.RawButton.RThumbstickLeft))
        {
            imDrone.transform.RotateAround(imDrone.transform.position, imDrone.transform.up, -30 * Time.deltaTime);
        }

        // scale my size
        if (right_index)
        {
            imDrone.transform.localScale *= (1 + 1 * Time.deltaTime);
        }
        else if (left_index)
        {
            imDrone.transform.localScale *= (1 - 1 * Time.deltaTime);
        }

        droneMsg.text = "position:" +
                        imDrone.transform.position.x.ToString("F3") + ", " +
                        imDrone.transform.position.y.ToString("F3") + ", " +
                        imDrone.transform.position.z.ToString("F3") + "\n" +
                    "drone scale:" +
                        imDrone.transform.localScale.x.ToString("F3");

        Vector3 babyDroneIbPos = imDrone.transform.position - eb.transform.position;
        float babyDroneIbScale = miniMap.transform.lossyScale.magnitude / eb.transform.lossyScale.magnitude;
        GameObject theBabyDoneIndicatorBall = Instantiate(indicatorBall, miniMap.transform.position + babyDroneIbPos * babyDroneIbScale, Quaternion.identity);
        //theBabyIndicatorBall.transform.localScale *= babyIbScale;
        Destroy(theBabyDoneIndicatorBall, 0.02f);

    }


    // pull trigger to shoot ray
    void IndexTriggerRay()
    {
        if (!sliceMode & !droneMode)
        {
            if (right_index) VisualizeRay(rightController);
            if (left_index) VisualizeRay(leftController);
        }
    }

    //visualize the ray, 0.5m long laser, 3m ray
    void VisualizeRay(GameObject controller)
    {
        GameObject myLine = new();
        //if (controller == rightController) myLine.tag = "rightLine";
        myLine.transform.position = controller.transform.position;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        if (!selected)
            lr.material = lineMat;
        else lr.material = highlightMat;
        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
        lr.SetPosition(0, controller.transform.position);
        lr.SetPosition(1, controller.transform.TransformPoint(Vector3.forward * 1f));
        Destroy(myLine, 0.02f);
    }


}