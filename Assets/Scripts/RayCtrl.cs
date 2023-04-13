using TMPro;
using UnityEngine;

/// <summary>
/// 
/// This class includes obejct manipulation methods using Rays
/// 
/// </summary>
public class RayCtrl : MonoBehaviour
{
    
    public GameObject leftController;
    public GameObject rightController;
    public Material highlightMat;
    public Material lineMat;
    public GameObject leftToolMenu;
    public GameObject rightToolMenu;
    public GameObject pinpointDisplay;
    public GameObject indicatorBall;
    public TextMeshPro pinpointDisplayMsg;
    public GameObject modePanel;
    public TextMeshProUGUI modeMsg;

    private GameObject currentObj; // the object currently registered for selection

    private bool selected = false; // this is an flag set true when object is being manipuated by some tool

    private bool singleRayGrabZoomMode = false; // these three tools' menuare on the left controller
    private bool twoRayDragZoomMode = false;                                               
    private bool pinpointSelectMode = false;                                    
    private bool singleRayGrabCloseFarMode = false; // this is a tool whose menu is on the right controller

    private bool right_index = false;
    private bool right_hand = false;
    private bool left_index = false;
    private bool left_hand = false;
    private float twoPointDistance = 0;
    private bool leftToolMenuOpen = false;
    private bool rightToolMenuOpen = false;

    void Start()
    {
        leftToolMenu.SetActive(false);
        rightToolMenu.SetActive(false);
        pinpointDisplay.SetActive(false);
        modePanel.SetActive(false);
    } 

    void Update()
    {
        TriggerListener();
        IndexTriggerRay();
        OpenMenu();
        ModeListener();
    }

    // make both left and right tool menus
    void OpenMenu() {
        leftToolMenu.SetActive(leftToolMenuOpen);
        rightToolMenu.SetActive(rightToolMenuOpen);
        if (leftToolMenuOpen | rightToolMenu)
        {
            SelectTool();
        }
        else if (OVRInput.GetDown(OVRInput.RawButton.LThumbstick)) {
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
                pinpointSelectMode = false;
                twoRayDragZoomMode = false;
                leftToolMenuOpen = false;
                singleRayGrabCloseFarMode = false;
                ShowModeMsg("Single Ray Select");
            }
            else if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickLeft))
            {
                pinpointSelectMode = true;
                singleRayGrabZoomMode = false;
                twoRayDragZoomMode = false;
                leftToolMenuOpen = false;
                singleRayGrabCloseFarMode = false;
                ShowModeMsg("3D Select (pinpoint)");
            }
            else if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickRight))
            {
                twoRayDragZoomMode = true;
                singleRayGrabZoomMode = false;
                pinpointSelectMode = false;
                leftToolMenuOpen = false;
                singleRayGrabCloseFarMode = false;
                ShowModeMsg("2 Ray Drag Zoom");
            }
        }
        else if (rightToolMenuOpen) {
            if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickUp))
            {
                singleRayGrabCloseFarMode = true;
                singleRayGrabZoomMode = false;
                pinpointSelectMode = false;
                twoRayDragZoomMode = false;
                leftToolMenuOpen = false;
                ShowModeMsg("Single Ray Grab and Move Close or Far"); // need better name
            }

        }


    }

    // this method updates the displayed message and instruction
    void ShowModeMsg(string theMsg)
    {
        modePanel.SetActive(true);
        modeMsg.text = theMsg;
    }

    void TriggerListener() {
        right_index = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) > 0.9f;
        right_hand = OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger) > 0.9f;
        left_index = OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger) > 0.9f;
        left_hand = OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger) > 0.9f;

    }
    void ModeListener() {
        if (pinpointSelectMode)
        {
            if (right_index & right_hand & left_index & left_hand)
            {
                PinpointSelect();
            }
            else
            {
                selected = false;
                pinpointDisplay.SetActive(false);
            }
        }
        else if (singleRayGrabZoomMode)
        {
            if (right_index & right_hand)
            {
                SingleRayGrabZoom(rightController);
            }
            else if (left_index & left_hand)
            {
                SingleRayGrabZoom(leftController);
            }
            else
            {
                selected = false;
                currentObj.transform.parent = null;
                currentObj = null;
            }

        }
        else if (twoRayDragZoomMode)
        {
            if (right_index & right_hand & left_index & left_hand)
            {
                TwoRayZoom();
            }
            else
            {
                selected = false;
                currentObj = null;
                twoPointDistance = 0;
            }
        }
        else if (singleRayGrabCloseFarMode)
        {
            if (right_index & right_hand)
            {
                SingleRayGrabCloseFar(rightController);
            }
            else if (left_index & left_hand)
            {
                SingleRayGrabCloseFar(leftController);
            }
            else
            {
                selected = false;
                currentObj.transform.parent = null;
                currentObj = null;
            }
        }
        else
        {
            selected = false;
            currentObj.transform.parent = null;
            currentObj = null;
            twoPointDistance = 0;
            pinpointDisplay.SetActive(false);
        }

    }



    /// <summary>
    /// ABOVE THIS LINE ARE METHODS FOR UI FOR TOOL SELECTION
    /// BELOW THIS LINE ARE METHODS FOR INTERACTION TOOLS
    /// </summary>



    // single ray drag zoom mode
    // shoot ray, translate around (drag)
    // move thumbstick up and down to scale up and down (zoom)
    void SingleRayGrabZoom(GameObject controller) {

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
                else if (controller == leftController) {
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
    void SingleRayGrabCloseFar(GameObject controller)
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
                        currentObj.transform.localPosition *= (1+10*Time.deltaTime);//move further
                    }
                    else if (OVRInput.Get(OVRInput.RawButton.RThumbstickDown))
                    {
                        currentObj.transform.localPosition *= (1 - 10 * Time.deltaTime);// move closer
                    }
                }
                else if (controller == leftController)
                {
                    if (OVRInput.Get(OVRInput.RawButton.LThumbstickUp))
                    {
                        currentObj.transform.localPosition *= (1 + 10 * Time.deltaTime);
                    }
                    else if (OVRInput.Get(OVRInput.RawButton.LThumbstickDown))
                    {
                        currentObj.transform.localPosition *= (1 - 10 * Time.deltaTime);
                    }
                }

            }

        }

    }


    // two ray zoom move
    // cast two rays onto object and scale the size of object based on distance of
    // left and right ray cast hit points
    void TwoRayZoom() {
        if (Physics.Raycast(leftController.transform.position, leftController.transform.forward, out RaycastHit hitL, 3) &
            Physics.Raycast(rightController.transform.position, rightController.transform.forward, out RaycastHit hitR, 3)) {

            if (hitL.transform.gameObject.CompareTag("ball") & hitR.transform.gameObject.CompareTag("ball")) {
                GameObject leftIndicatorBall = Instantiate(indicatorBall, hitL.point, Quaternion.identity);
                GameObject rightIndicatorBall = Instantiate(indicatorBall, hitR.point, Quaternion.identity);
                Destroy(leftIndicatorBall,  0.02f);
                Destroy(rightIndicatorBall, 0.02f);

                if (!selected)
                {
                    //twoPointDistance = (leftController.transform.position - rightController.transform.position).magnitude;
                    twoPointDistance = (hitL.point - hitR.point).magnitude;
                    selected = true;
                    
                }
                currentObj = hitL.transform.gameObject;

                if (selected & twoPointDistance>0)
                {
                    currentObj.transform.localScale = Vector3.Lerp(currentObj.transform.localScale,
                ((hitL.point - hitR.point).magnitude) * currentObj.transform.localScale / twoPointDistance, 0.05f);
                }
            }
        }
        //if (selected) {
        //    currentObj.transform.localScale = Vector3.Lerp(currentObj.transform.localScale,
        //        ((leftController.transform.position - rightController.transform.position).magnitude) *currentObj.transform.localScale/twoPointDistance,
        //        0.1f);
        //}
        
    }

    // 3D select mode with two rays
    // return the position/coord of the intersection of rays
    // figure out later what to do with this information
    // bool check: pinpointSelectMode
    void PinpointSelect(){

        GameObject rr = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rr.transform.localScale = new Vector3(0.01f, 0.01f, 3);
        rr.transform.SetPositionAndRotation(rightController.transform.position, rightController.transform.rotation);
        rr.transform.parent = rightController.transform;
        rr.transform.localPosition = new Vector3(0f, 0f, 1.5f);
        Destroy(rr.GetComponent<MeshRenderer>());
        rr.tag = "rightLine";
        if ( Physics.Raycast(leftController.transform.position, leftController.transform.forward, out RaycastHit hit, 3)) {
            if (hit.transform.gameObject.CompareTag("rightLine"))
            {
                pinpointDisplay.SetActive(true);
                pinpointDisplay.transform.SetPositionAndRotation(hit.point, Quaternion.identity);
                pinpointDisplayMsg.text = hit.point.x.ToString() + ',' + hit.point.y.ToString() + ',' + hit.point.y.ToString();
                selected = true;
                GameObject theIndicatorBall = Instantiate(indicatorBall, hit.point,Quaternion.identity);
                Destroy(theIndicatorBall, 0.02f);
            }
 
        }
        Destroy(rr, 3f);

    }

    // pull trigger to shoot ray
    void IndexTriggerRay() {
        if (right_index) VisualizeRay(rightController);
        if (left_index) VisualizeRay(leftController);
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
