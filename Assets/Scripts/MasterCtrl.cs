using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

/// <summary>
/// 
/// Monolith Script for the whole project
///
/// </summary>
public class MasterCtrl : MonoBehaviour
{

    public GameObject leftController;
    public GameObject rightController;
    public GameObject userRig;
    public GameObject leftClipper;
    public GameObject rightClipper;
    public GameObject leftClipperIndicator;
    public GameObject rightClipperIndicator;
    public GameObject imDrone;
    public GameObject miniMap;
    public Camera birdCam;

    // the models and model loading menus
    public GameObject brainModel; // always keep this, this is the highest level model that is interacted with directly
    public GameObject brain;
    public GameObject eb;
    public GameObject pb;
    public GameObject elNeurons;
    public GameObject erNeurons;
    public GameObject modelMenu;
    public GameObject[] menuItems;

    // UI elements
    public GameObject leftToolMenu;
    public GameObject rightToolMenu;
    public GameObject pinpointDisplay;
    public GameObject indicatorBall;
    public GameObject minimapIndicatorBall;
    public GameObject posArrow;
    public GameObject restartPanel;
    public TextMeshPro pinpointDisplayMsg;
    public GameObject instructionPanel;
    public TextMeshProUGUI instructionMsg;
    public GameObject modePanel;
    public TextMeshProUGUI modeMsg;
    public GameObject dronePanel;
    public GameObject comfortPanel;
    public TextMeshPro droneMsg;
    public GameObject droneDirectionArrow;
    public GameObject birdCamDisplay;

    // bools related to tool modes
    private bool leftToolMenuOpen = false;
    private bool rightToolMenuOpen = false;
    private bool singleRaySelectMode = false;
    private bool singleRayManipulationMode = false;
    private bool singleRayDepthCamMode = false;
    private bool sliceMode = false;
    private bool modelLoadingMode = false;
    private bool droneMode = false;

    // helper var for tool use  
    private bool selected = false; // this is an flag set true when object is being manipuated by some tool
    private bool isDrone = false;
    private bool isComfort = false;
    private bool isRestart = false;
    private bool droneToggledPassthrough = false;
    private bool leftClipperFixed = false;
    private bool rightClipperFixed = false;
    public Material highlightMat;
    public Material lineMat;
    private GameObject cameraIndicatorBall = null; // this object is only used for single ray move camera; can't find a way around it
    private int currentMenuItem = 0;
    private Color offColor = Color.gray;
    private Color onColor = Color.green;
    private Color hoverColor = Color.cyan;

    // state storage var
    private GameObject currentObj; // the object currently registered for selection
    private Vector3 preManipulationScale = Vector3.one;
    private Vector3 preManipulationPos = Vector3.one;
    private Quaternion preManipulationRot = Quaternion.identity;
    private Vector3 preDronePos = Vector3.one;
    private Quaternion preDroneRot = Quaternion.identity;
    private Vector3 clipperPos = Vector3.one;
    private Quaternion clipperRot = Quaternion.identity;
    private Vector3 iniBrainScale = Vector3.one;
    private List<float[]> snapshotList = new List<float[]>();

    private GameObject selectedNeuronMesh = null;
    private GameObject selectedNeuronClone = null; 
    // private bools for trigger controls
    private bool right_index = false;
    private bool right_hand = false;
    private bool left_index = false;
    private bool left_hand = false;

    //private bools for toggling models;
    private bool ebOn = false;
    private bool pbOn = false;
    private bool elNeuronsOn = false;
    private bool erNeuronsOn = false;
    private bool allOn = false;

    //instuction visualiztions for displaying instruction messages
    public GameObject droneHelp_L;
    public GameObject droneHelp_R;
    public GameObject sliceHelp_L;
    public GameObject sliceHelp_R;
    public GameObject loadHelp_L;
    public GameObject loadHelp_R;
    public GameObject camHelp_leftTrigger_L;
    public GameObject camHelp_leftTrigger_R;
    public GameObject camHelp_rightTrigger_L;
    public GameObject camHelp_rightTrigger_R;
    public GameObject manipulateHelp_leftTrigger_L;
    public GameObject manipulateHelp_leftTrigger_R;
    public GameObject manipulateHelp_rightTrigger_L;
    public GameObject manipulateHelp_rightTrigger_R;
    public GameObject selectHelp_L;
    public GameObject selectHelp_R;
    public GameObject nullHelp_L;
    public GameObject nullHelp_R;


    void Start()
    {
        //todo: update the initial instruction msg once we have model loading function
        leftToolMenu.SetActive(false);
        rightToolMenu.SetActive(false);
        pinpointDisplay.SetActive(false);
        modePanel.SetActive(false);
        birdCamDisplay.SetActive(false);
        birdCam.enabled = false;
        miniMap.SetActive(false);
        leftClipperIndicator.SetActive(false);
        rightClipperIndicator.SetActive(false);
        dronePanel.SetActive(false);
        comfortPanel.SetActive(false);
        restartPanel.SetActive(false);
        droneDirectionArrow.SetActive(false);
        modelMenu.SetActive(false);

        // store some inititial value;
        clipperPos = leftClipper.transform.position;
        clipperRot = leftClipper.transform.rotation;
        iniBrainScale = brainModel.transform.localScale;

        eb.SetActive(false);
        pb.SetActive(false);
        elNeurons.SetActive(false);
        erNeurons.SetActive(false);

        droneHelp_L.SetActive(false);
        droneHelp_R.SetActive(false);
        sliceHelp_L.SetActive(false);
        sliceHelp_R.SetActive(false);
        loadHelp_L.SetActive(false);
        loadHelp_R.SetActive(false);
        camHelp_leftTrigger_L.SetActive(false);
        camHelp_leftTrigger_R.SetActive(false);
        camHelp_rightTrigger_L.SetActive(false);
        camHelp_rightTrigger_R.SetActive(false);
        manipulateHelp_leftTrigger_L.SetActive(false);
        manipulateHelp_leftTrigger_R.SetActive(false);
        manipulateHelp_rightTrigger_L.SetActive(false);
        manipulateHelp_rightTrigger_R.SetActive(false);
        selectHelp_L.SetActive(false);
        selectHelp_R.SetActive(false);
        nullHelp_L.SetActive(false);
        nullHelp_R.SetActive(false);

    }

    void Update()
    {
        TriggerListener();
        IndexTriggerRay();
        OpenMenu();
        ModelToggleListener();
        ModeListener();
        InstructionListener();
        Restart();
        BackgroundToggle();
        ResetClippers();
        StoreSnapshot();
    }

    // make both left and right tool menus
    void OpenMenu()
    {
        if (!isDrone) //should not open other tools when you are in drone mode
        {
            bool noTriggersPressed = !right_hand & !right_index & !left_hand & !left_index;
            if (OVRInput.GetDown(OVRInput.RawButton.LThumbstick) & noTriggersPressed)
            {
                leftToolMenuOpen = true;
                leftToolMenu.transform.SetPositionAndRotation(leftController.transform.position, leftController.transform.rotation);
                leftToolMenu.transform.parent = leftController.transform;
            }
            else if (OVRInput.GetDown(OVRInput.RawButton.RThumbstick) & noTriggersPressed)
            {
                rightToolMenuOpen = true;
                rightToolMenu.transform.SetPositionAndRotation(rightController.transform.position, rightController.transform.rotation);
                rightToolMenu.transform.parent = rightController.transform;
            }

            if (!modePanel.activeSelf)
                ShowInstructionMsg("Interaction Demo open Left/Right Tool Menu by\n pressing Left/Right Thumbstick");
            leftToolMenu.SetActive(leftToolMenuOpen);
            rightToolMenu.SetActive(rightToolMenuOpen);
            SelectTool();
        }

    }

    // sets booleans for tools
    void SelectTool()
    {
        if (leftToolMenuOpen)
        {
            if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickUp))
            {
                sliceMode = true;
                modelLoadingMode = false;
                singleRaySelectMode = false;
                singleRayManipulationMode = false;
                singleRayDepthCamMode = false;
                droneMode = false;

                leftToolMenuOpen = false;
                rightToolMenuOpen = false;
                ShowModeMsg("Clipper Mode");
                ShowInstructionMsg("Hold Both triggers on Either controller to \n spawn Clipper, move controller to Clip");

            }
            else if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickLeft))
            {
                droneMode = true; // todo:  bug: in droneMode, the lefttoolmenu does not close
                singleRaySelectMode = false;
                modelLoadingMode = false;
                singleRayManipulationMode = false;
                singleRayDepthCamMode = false;
                sliceMode = false;

                leftToolMenuOpen = false;
                rightToolMenuOpen = false;
                ShowModeMsg("Drone Mode");
                ShowInstructionMsg("Drone Released, Operate the Drone with Controllers");
            }
            else if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickRight))
            {
                modelLoadingMode = true;
                singleRaySelectMode = false;
                singleRayManipulationMode = false;
                singleRayDepthCamMode = false;
                sliceMode = false;
                droneMode = false;

                leftToolMenuOpen = false;
                rightToolMenuOpen = false;
                ShowModeMsg("Load Model Mode");
                ShowInstructionMsg("Select Neuropil/Celltype to Load/Remove");
            }
            else if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickDown))
            {
                leftToolMenuOpen = false;
            }
        }

        if (rightToolMenuOpen)
        {
            if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickUp))
            {
                singleRaySelectMode = true;
                modelLoadingMode = false;
                singleRayManipulationMode = false;
                singleRayDepthCamMode = false;
                sliceMode = false;
                droneMode = false;

                leftToolMenuOpen = false;
                rightToolMenuOpen = false;
                ShowModeMsg("Nueron Select Mode");
                ShowInstructionMsg("Hold Both triggers on Either controller to\nRaycast and Select neuron");
            }
            else if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickLeft))
            {
                singleRayManipulationMode = true;
                singleRaySelectMode = false;
                modelLoadingMode = false;
                singleRayDepthCamMode = false;
                sliceMode = false;
                droneMode = false;

                leftToolMenuOpen = false;
                rightToolMenuOpen = false;
                ShowModeMsg("Model Manipulation Mode");
                ShowInstructionMsg("Hold Both triggers on Either controller to\nRaycast and Grasp Model to Manipulate");
            }
            else if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickRight))
            {
                singleRayDepthCamMode = true;
                singleRaySelectMode = false;
                modelLoadingMode = false;
                singleRayManipulationMode = false;
                sliceMode = false;
                droneMode = false;

                leftToolMenuOpen = false;
                rightToolMenuOpen = false;
                ShowModeMsg("Camera Probe Mode");
                ShowInstructionMsg("Hold Both triggers on Either controller to launch Camera Probe ");
            }
            else if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickDown))
            {
                rightToolMenuOpen = false;
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
        else if (singleRaySelectMode)
        {
            SingleRaySelect();

        }
        else if (modelLoadingMode)
        {
            LoadModels();
        }
        else if (singleRayManipulationMode)
        {
            SingleRayManipulation();
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
            ShowInstructionMsg("Interaction Demo open Left/Right Tool Menu by\n pressing Left/Right Thumbstick");
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

     //this method updates the instructions that are locked to the controllers and tells user what does what;
    void InstructionListener()
    {
        if (!leftToolMenuOpen)
        {
            selectHelp_L.SetActive(singleRaySelectMode);
            droneHelp_L.SetActive(droneMode);
            loadHelp_L.SetActive(modelLoadingMode);
            sliceHelp_L.SetActive(sliceMode);
            nullHelp_L.SetActive(!modePanel.activeSelf & !droneMode);

            camHelp_leftTrigger_L.SetActive(singleRayDepthCamMode & (left_hand & left_index) & !(right_hand & right_index));
            manipulateHelp_leftTrigger_L.SetActive(singleRayManipulationMode & (left_hand & left_index) & !(right_hand & right_index));
            camHelp_rightTrigger_L.SetActive(singleRayDepthCamMode & !(left_hand & left_index) & (right_hand & right_index));
            manipulateHelp_rightTrigger_L.SetActive(singleRayManipulationMode & !(left_hand & left_index) & (right_hand & right_index));
            //if (left_hand & left_index)
            //{
            //    if (!(right_hand & right_index)) {
            //        camHelp_leftTrigger_L.SetActive(singleRayDepthCamMode);
            //        manipulateHelp_leftTrigger_L.SetActive(singleRayManipulationMode);
            //    }
            //}
            //else if (right_hand & right_index)
            //{
            //    if (!(left_hand & left_index)) {
            //        camHelp_rightTrigger_L.SetActive(singleRayDepthCamMode);
            //        manipulateHelp_rightTrigger_L.SetActive(singleRayManipulationMode);
            //    }
            //}
            //else {
            //    camHelp_leftTrigger_L.SetActive(false);
            //    manipulateHelp_leftTrigger_L.SetActive(false);
            //    camHelp_rightTrigger_L.SetActive(false);
            //    manipulateHelp_rightTrigger_L.SetActive(false);
            //}

        }
        else {
            selectHelp_L.SetActive(false);
            droneHelp_L.SetActive(false);
            loadHelp_L.SetActive(false);
            sliceHelp_L.SetActive(false);
            nullHelp_L.SetActive(false);
            camHelp_leftTrigger_L.SetActive(false);
            manipulateHelp_leftTrigger_L.SetActive(false);
            camHelp_rightTrigger_L.SetActive(false);
            manipulateHelp_rightTrigger_L.SetActive(false);
        }

        if (!rightToolMenuOpen)
        {
            selectHelp_R.SetActive(singleRaySelectMode);
            droneHelp_R.SetActive(droneMode);
            loadHelp_R.SetActive(modelLoadingMode);
            sliceHelp_R.SetActive(sliceMode);
            nullHelp_R.SetActive(!modePanel.activeSelf & !droneMode);

            camHelp_leftTrigger_R.SetActive(singleRayDepthCamMode & (left_hand & left_index) & !(right_hand & right_index));
            manipulateHelp_leftTrigger_R.SetActive(singleRayManipulationMode & (left_hand & left_index) & !(right_hand & right_index));
            camHelp_rightTrigger_R.SetActive(singleRayDepthCamMode & !(left_hand & left_index) & (right_hand & right_index));
            manipulateHelp_rightTrigger_R.SetActive(singleRayManipulationMode & !(left_hand & left_index) & (right_hand & right_index));
            //if (left_hand & left_index)
            //{
            //    if (!(right_hand & right_index))
            //    {
            //        camHelp_leftTrigger_R.SetActive(singleRayDepthCamMode);
            //        manipulateHelp_leftTrigger_R.SetActive(singleRayManipulationMode);
            //    }
            //}
            //else if (right_hand & right_index)
            //{
            //    if (!(left_hand & left_index))
            //    {
            //        camHelp_rightTrigger_R.SetActive(singleRayDepthCamMode);
            //        manipulateHelp_rightTrigger_R.SetActive(singleRayManipulationMode);
            //    }
            //}
            //else
            //{
            //    camHelp_leftTrigger_R.SetActive(false);
            //    manipulateHelp_leftTrigger_R.SetActive(false);
            //    camHelp_rightTrigger_R.SetActive(false);
            //    manipulateHelp_rightTrigger_R.SetActive(false);
            //}
        }
        else
        {
            selectHelp_R.SetActive(false);
            droneHelp_R.SetActive(false);
            loadHelp_R.SetActive(false);
            sliceHelp_R.SetActive(false);
            nullHelp_R.SetActive(false);
            camHelp_leftTrigger_R.SetActive(false);
            manipulateHelp_leftTrigger_R.SetActive(false);
            camHelp_rightTrigger_R.SetActive(false);
            manipulateHelp_rightTrigger_R.SetActive(false);
        }

    }


    // restart function
    void Restart()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.Start))
        {
            restartPanel.SetActive(true);
            instructionPanel.SetActive(false);
            modePanel.SetActive(false);
            isRestart = true;
        }
        if (OVRInput.GetDown(OVRInput.RawButton.Y) & isRestart)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else if (OVRInput.GetDown(OVRInput.RawButton.X) & isRestart)
        {
            isRestart = false;
            restartPanel.SetActive(false);
            instructionPanel.SetActive(true);
            modePanel.SetActive(true);
        }
    }

    void BackgroundToggle()
    {
        if (!droneMode) // should not toggle in drone mode, must disable passthrough in drone mode
        {
            if (OVRInput.GetDown(OVRInput.RawButton.B) & !droneMode)
            {
                if (OVRManager.instance.isInsightPassthroughEnabled)
                {
                    OVRManager.instance.isInsightPassthroughEnabled = false;
                }
                else
                {
                    OVRManager.instance.isInsightPassthroughEnabled = true;
                }
            }
        }
    }

    //TODO: now it is accessible on the tool menu, tho at the begining of the game we should also the model selection model
    void LoadModels() {
        // first line:
        if (modelMenu.activeSelf) {
            modelMenu.transform.position = leftController.transform.position + new Vector3(0, 0.2f, 0);
            modelMenu.transform.LookAt(2 * modelMenu.transform.position - userRig.transform.position);

            if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickDown))
            {
                ScrollDown();
            }
            else if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickUp))
            {
                ScrollUp();
            }
            else if (OVRInput.GetDown(OVRInput.RawButton.A) & !isRestart)
            {
                SelectMenuItem(currentMenuItem);
            }
            else if (OVRInput.GetDown(OVRInput.RawButton.X) & !isRestart)
            {
                modelLoadingMode = false;
                modePanel.SetActive(false);
            }

            MenuItemColorUpdate();
        }
    }
    void ScrollUp() {
        if (currentMenuItem > 0) currentMenuItem--;
    }
    void ScrollDown() {
        if (currentMenuItem < menuItems.Length - 1) currentMenuItem++;
    }
    void SelectMenuItem(int itemID)
    {
        if (itemID == 0) ebOn = !ebOn;
        else if (itemID == 1) pbOn = !pbOn;
        else if (itemID == 2) elNeuronsOn = !elNeuronsOn;
        else if (itemID == 3) erNeuronsOn = !erNeuronsOn;
        else if (itemID == 4)
        {
            allOn = ebOn & pbOn & elNeuronsOn & erNeuronsOn;
            if (!allOn)
            {
                ebOn = true;
                pbOn = true;
                elNeuronsOn = true;
                erNeuronsOn = true;
            }
            else
            {
                ebOn = false;
                pbOn = false;
                elNeuronsOn = false;
                erNeuronsOn = false;
            }
        }
    }
    void MenuItemColorUpdate()
    {
        allOn = ebOn & pbOn & erNeuronsOn & elNeuronsOn;

        if (ebOn) menuItems[0].GetComponent<Image>().color = onColor;
        else menuItems[0].GetComponent<Image>().color = offColor;
        if (pbOn) menuItems[1].GetComponent<Image>().color = onColor;
        else menuItems[1].GetComponent<Image>().color = offColor;
        if (elNeuronsOn) menuItems[2].GetComponent<Image>().color = onColor;
        else menuItems[2].GetComponent<Image>().color = offColor;
        if (erNeuronsOn) menuItems[3].GetComponent<Image>().color = onColor;
        else menuItems[3].GetComponent<Image>().color = offColor;
        if (allOn) menuItems[4].GetComponent<Image>().color = onColor;
        else menuItems[4].GetComponent<Image>().color = offColor;

        menuItems[currentMenuItem].GetComponent<Image>().color = hoverColor;
    }
    void ModelToggleListener() {
        modelMenu.SetActive(modelLoadingMode);
        eb.SetActive(ebOn);
        pb.SetActive(pbOn);
        elNeurons.SetActive(elNeuronsOn);
        erNeurons.SetActive(erNeuronsOn);
    }


    /// <summary>
    /// Below this line is strictly interaction methods
    /// </summary>


    // single ray select
    // return information of neuron or neuropil
    // currently only supports neuron, somehow need to make it also support neuropil
    // TODO: add hide neuron function, and/or pinning
    void SingleRaySelect()
    {
        if (right_index & right_hand)
        {
            SingleRaySelectEachHand(rightController);
        }
        else if (left_index & left_hand)
        {
            SingleRaySelectEachHand(leftController);
        }
        else
        {
            selected = false;
            pinpointDisplay.SetActive(false);

            selectedNeuronMesh.SetActive(true); // activate selected neuron
            Destroy(selectedNeuronClone); // destroy selection clone
            selectedNeuronClone = null; // reset selection clone
        }
    }
    void SingleRaySelectEachHand(GameObject controller)
    {
        if (Physics.Raycast(controller.transform.position, controller.transform.forward, out RaycastHit hit, 6))
        {
            // lock on, and maintail until drop 
            if (hit.transform.parent.gameObject.CompareTag("Neuron"))
            {
                selected = true;
                pinpointDisplay.SetActive(true);
                pinpointDisplayMsg.text = hit.transform.parent.name;
                pinpointDisplay.transform.SetLocalPositionAndRotation(hit.point, Quaternion.Euler(0f, userRig.transform.rotation.eulerAngles.y, 0f));

                if (selectedNeuronClone == null)
                {
                    selectedNeuronMesh = hit.transform.gameObject; // store new selection
                    Destroy(selectedNeuronClone); // destroy old selection clone
                    selectedNeuronClone = Instantiate(hit.transform.gameObject); // create new selection clone
                    selectedNeuronMesh.SetActive(false); // disable selected neuron
                    selectedNeuronClone.GetComponent<MeshRenderer>().material.color = Color.white; // set clone color
                    selectedNeuronClone.transform.SetParent(brain.transform, false); // set clone parent and local scale
                }
                else {
                    // only restore and update new neuron is hit
                    // this also allow the debug messa to only be updated when we have this transition
                    // meaning the msg returns the restored shader name
                    if (hit.transform.gameObject != selectedNeuronMesh)
                    {
                        selectedNeuronMesh.SetActive(true); // activate old selection
                        selectedNeuronMesh = hit.transform.gameObject; // store new selection
                        Destroy(selectedNeuronClone); // destroy old selection clone
                        selectedNeuronClone = Instantiate(hit.transform.gameObject); // create new selection clone
                        selectedNeuronMesh.SetActive(false); // disable selected neuron
                        selectedNeuronClone.GetComponent<MeshRenderer>().material.color = Color.white; // set clone color
                        selectedNeuronClone.transform.SetParent(brain.transform, false); // set clone parent and local scale
                    }
                }

                GameObject theIndicatorBall = Instantiate(indicatorBall, hit.point, Quaternion.identity);
                Destroy(theIndicatorBall, 0.02f);
            }
        }
    }

    // single ray manipulation mode
    // combined the former single ray drag zoom and single ray grab close far modes
    // use either controller, shoot ray, translate around (drag)
    // move thumbstick up and down on the ray casting controller to move object closer or further to user
    // move thumbstick up and down on the other controller to scale the object size
    void SingleRayManipulation()
    {
        if (right_index & right_hand)
        {
            if(!(left_index & left_hand))
            SingleRayManipulationEachHand(rightController);
        }
        else if (left_index & left_hand)
        {
            if(!(right_index & right_hand))
            SingleRayManipulationEachHand(leftController);
        }
        else
        { 
            selected = false;
            currentObj.transform.parent = null;
        }
    }
    void SingleRayManipulationEachHand(GameObject controller)
    {
        if (Physics.Raycast(controller.transform.position, controller.transform.forward, out RaycastHit hit, 6))
        {
            if (hit.transform.parent.gameObject.CompareTag("Neuron") & !selected)
            {
                selected = true;
                currentObj = brainModel;
                currentObj.transform.parent = controller.transform;
                preManipulationPos = currentObj.transform.position;
                preManipulationRot = currentObj.transform.rotation;
                preManipulationScale = currentObj.transform.localScale;
                
                GameObject theIndicatorBall = Instantiate(indicatorBall, hit.point, Quaternion.identity);
                Destroy(theIndicatorBall, 0.02f);
            }
        }
        if (currentObj != null)
        {
            if (controller == rightController)
            {
                // move up close far
                if (OVRInput.Get(OVRInput.RawButton.RThumbstickUp))
                {
                    currentObj.transform.localPosition *= (1 + 1 * Time.deltaTime);//move further
                }
                else if (OVRInput.Get(OVRInput.RawButton.RThumbstickDown))
                {
                    currentObj.transform.localPosition *= (1 - 1 * Time.deltaTime);// move closer
                }
                // scale large small
                else if (OVRInput.Get(OVRInput.RawButton.RThumbstickLeft))
                {
                    currentObj.transform.localScale = Vector3.Lerp(currentObj.transform.localScale,
                        currentObj.transform.localScale * (1 - 10 * Time.deltaTime), 0.1f);
                }
                else if (OVRInput.Get(OVRInput.RawButton.RThumbstickRight))
                {
                    currentObj.transform.localScale = Vector3.Lerp(currentObj.transform.localScale,
                        currentObj.transform.localScale * (1 + 10 * Time.deltaTime), 0.1f);
                }
                //rotate 
                else if (OVRInput.Get(OVRInput.RawButton.LThumbstickUp))
                {
                    currentObj.transform.RotateAround(currentObj.transform.position, transform.right, -30 * Time.deltaTime);
                }
                else if (OVRInput.Get(OVRInput.RawButton.LThumbstickDown))
                {
                    currentObj.transform.RotateAround(currentObj.transform.position, transform.right, 30 * Time.deltaTime);
                }
                else if (OVRInput.Get(OVRInput.RawButton.LThumbstickLeft))
                {
                    currentObj.transform.RotateAround(currentObj.transform.position, transform.up, -30 * Time.deltaTime);
                }
                else if (OVRInput.Get(OVRInput.RawButton.LThumbstickRight))
                {
                    currentObj.transform.RotateAround(currentObj.transform.position, transform.up, 30 * Time.deltaTime);
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

                else if (OVRInput.Get(OVRInput.RawButton.LThumbstickLeft))
                {
                    currentObj.transform.localScale = Vector3.Lerp(currentObj.transform.localScale,
                        currentObj.transform.localScale * (1 - 10 * Time.deltaTime), 0.1f);
                }
                else if (OVRInput.Get(OVRInput.RawButton.LThumbstickRight))
                {
                    currentObj.transform.localScale = Vector3.Lerp(currentObj.transform.localScale,
                        currentObj.transform.localScale * (1 + 10 * Time.deltaTime), 0.1f);
                }
                else if (OVRInput.Get(OVRInput.RawButton.RThumbstickUp))
                {
                    currentObj.transform.RotateAround(currentObj.transform.position, transform.right, -30 * Time.deltaTime);
                }
                else if (OVRInput.Get(OVRInput.RawButton.RThumbstickDown))
                {
                    currentObj.transform.RotateAround(currentObj.transform.position, transform.right, 30 * Time.deltaTime);
                }
                else if (OVRInput.Get(OVRInput.RawButton.RThumbstickLeft))
                {
                    currentObj.transform.RotateAround(currentObj.transform.position, transform.up, -30 * Time.deltaTime);
                }
                else if (OVRInput.Get(OVRInput.RawButton.RThumbstickRight))
                {
                    currentObj.transform.RotateAround(currentObj.transform.position, transform.up, 30 * Time.deltaTime);

                }
            }

            if (OVRInput.GetDown(OVRInput.RawButton.X) & !isRestart)
            {
                currentObj.transform.localScale = preManipulationScale;
                currentObj.transform.SetPositionAndRotation(preManipulationPos, preManipulationRot);
            }

            ShowModeMsg("model position:" +
                currentObj.transform.position.x.ToString("F3") + ", " +
                currentObj.transform.position.y.ToString("F3") + ", " +
                currentObj.transform.position.z.ToString("F3") + "\n" +
                "model scale:" +
                currentObj.transform.lossyScale.x.ToString("F3"));
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
                cameraIndicatorBall = Instantiate(indicatorBall, rightController.transform.position, rightController.transform.rotation);
                cameraIndicatorBall.transform.parent = rightController.transform;
                cameraIndicatorBall.transform.localPosition = new Vector3(0f, 0f, 0.5f);

                birdCam.enabled = true;
                birdCamDisplay.SetActive(true);

                miniMap.SetActive(true);
                miniMap.transform.SetPositionAndRotation(leftController.transform.position + new Vector3(0, 0.15f, 0), leftController.transform.rotation); ;
                miniMap.transform.parent = leftController.transform;

                selected = true;
            }
            else if (left_index & left_hand)
            {
                cameraIndicatorBall = Instantiate(indicatorBall, leftController.transform.position, leftController.transform.rotation);
                cameraIndicatorBall.transform.parent = leftController.transform;
                cameraIndicatorBall.transform.localPosition = new Vector3(0f, 0f, 0.5f);

                birdCam.enabled = true;
                birdCamDisplay.SetActive(true);

                miniMap.SetActive(true);
                miniMap.transform.SetPositionAndRotation(rightController.transform.position + new Vector3(0, 0.15f, 0), rightController.transform.rotation); ;
                miniMap.transform.parent = rightController.transform;

                selected = true;
            }
        }
        else
        {
            if (right_index & right_hand)
            {
                if (!(left_index & left_hand))
                SingleRayDepthCamEachHand(rightController, cameraIndicatorBall);
            }
            else if (left_index & left_hand)
            {
                if (!(right_index & right_hand))
                SingleRayDepthCamEachHand(leftController, cameraIndicatorBall);
            }
            else
            {
                selected = false;
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
            birdCam.transform.position = ib.transform.position;
            birdCam.transform.parent = cameraIndicatorBall.transform;

            if (OVRInput.Get(OVRInput.RawButton.RThumbstickUp))
            {
                ib.transform.localPosition *= (1 + Time.deltaTime);
            }
            else if (OVRInput.Get(OVRInput.RawButton.RThumbstickDown))
            {
                ib.transform.localPosition *= (1 - Time.deltaTime);
            }
            else if (OVRInput.Get(OVRInput.RawButton.RThumbstickRight))
            {
                birdCam.fieldOfView *= (1 + Time.deltaTime);
            }
            else if (OVRInput.Get(OVRInput.RawButton.RThumbstickLeft))
            {
                birdCam.fieldOfView *= (1 - Time.deltaTime);
            }

            if (OVRInput.Get(OVRInput.RawButton.LThumbstickUp))
            {
                birdCam.transform.RotateAround(birdCam.transform.position, birdCam.transform.right, -15 * Time.deltaTime);
            }
            else if (OVRInput.Get(OVRInput.RawButton.LThumbstickDown))
            {
                birdCam.transform.RotateAround(birdCam.transform.position, birdCam.transform.right, 15 * Time.deltaTime);
            }
            if (OVRInput.Get(OVRInput.RawButton.LThumbstickRight))
            {
                birdCam.transform.RotateAround(birdCam.transform.position, birdCam.transform.up, 15 * Time.deltaTime);
            }
            else if (OVRInput.Get(OVRInput.RawButton.LThumbstickLeft))
            {
                birdCam.transform.RotateAround(birdCam.transform.position, birdCam.transform.up, -15 * Time.deltaTime);
            }

            birdCam.transform.localRotation = Quaternion.Euler(birdCam.transform.localRotation.eulerAngles.x,
                                                                        birdCam.transform.localRotation.eulerAngles.y,
                                                                        -rightController.transform.rotation.eulerAngles.z);
        }

        else if (controller == leftController)
        {
            birdCam.transform.position = ib.transform.position;
            birdCam.transform.parent = cameraIndicatorBall.transform;

            if (OVRInput.Get(OVRInput.RawButton.LThumbstickUp))
            {
                ib.transform.localPosition *= (1 + Time.deltaTime);
            }
            else if (OVRInput.Get(OVRInput.RawButton.LThumbstickDown))
            {
                ib.transform.localPosition *= (1 - Time.deltaTime);
            }
            else if (OVRInput.Get(OVRInput.RawButton.LThumbstickRight))
            {
                birdCam.fieldOfView *= (1 + Time.deltaTime);
            }
            else if (OVRInput.Get(OVRInput.RawButton.LThumbstickLeft))
            {
                birdCam.fieldOfView *= (1 - Time.deltaTime);
            }

            if (OVRInput.Get(OVRInput.RawButton.RThumbstickUp))
            {
                birdCam.transform.RotateAround(birdCam.transform.position, birdCam.transform.right, -15 * Time.deltaTime);
            }
            else if (OVRInput.Get(OVRInput.RawButton.RThumbstickDown))
            {
                birdCam.transform.RotateAround(birdCam.transform.position, birdCam.transform.right, 15 * Time.deltaTime);
            }
            if (OVRInput.Get(OVRInput.RawButton.RThumbstickRight))
            {
                birdCam.transform.RotateAround(birdCam.transform.position, birdCam.transform.up, 15 * Time.deltaTime);
            }
            else if (OVRInput.Get(OVRInput.RawButton.RThumbstickLeft))
            {
                birdCam.transform.RotateAround(birdCam.transform.position, birdCam.transform.up, -15 * Time.deltaTime);
            }
            birdCam.transform.localRotation = Quaternion.Euler(birdCam.transform.localRotation.eulerAngles.x,
                                                                        birdCam.transform.localRotation.eulerAngles.y,
                                                                        -leftController.transform.rotation.eulerAngles.z);
        }
        if (OVRInput.GetDown(OVRInput.RawButton.X) & !isRestart)
        {
            birdCam.fieldOfView = 60f;
            birdCam.transform.localRotation = Quaternion.Euler(0,0,0);
        }

        ShowModeMsg("Camera Mode\nCam Depth" + ib.transform.localPosition.z.ToString("F2") +
            " m\n FOV "+((int)birdCam.fieldOfView).ToString() +
            "\ndir (" + ((int)birdCam.transform.localRotation.eulerAngles.x).ToString()+","
            + ((int)birdCam.transform.localRotation.eulerAngles.y).ToString() + ",0)");

        Vector3 babyIbPos = brainModel.transform.InverseTransformPoint(ib.transform.position);
        Quaternion babyIbRot = Quaternion.Inverse(brainModel.transform.rotation) * ib.transform.rotation;
        GameObject theMinimapArrow = Instantiate(posArrow, miniMap.transform.position, Quaternion.identity);
        theMinimapArrow.transform.parent = miniMap.transform;
        theMinimapArrow.transform.SetLocalPositionAndRotation(babyIbPos, babyIbRot);
        theMinimapArrow.transform.localScale = 666.7f * iniBrainScale.magnitude * new Vector3(imDrone.transform.localScale.x*0.66f, imDrone.transform.localScale.y, imDrone.transform.localScale.z) /brainModel.transform.localScale.magnitude;
        Destroy(theMinimapArrow, 0.02f);
    }



    // slice mode stuff
    // want to allow both hands to slice at the same time
    void Slice()
    {
        if (right_index & right_hand)
        {
            // todo: how the fuck does this line teleport the clipper ???
            // how is it not in my hands legit what the fuck
            if (!rightClipperIndicator.activeSelf)
            {
                rightClipper.transform.parent = null; // this line is needed to prevent a bug -- after clipping and returning to slice mode, the first trigger press results in offsetted plane clipper, which is corrected after a second press
                rightClipperFixed = false;
                rightClipperIndicator.SetActive(true);
                rightClipper.transform.SetLocalPositionAndRotation(rightController.transform.position, rightController.transform.rotation);
                rightClipper.transform.parent = rightController.transform;
            }
        }
        else
        {
            if (rightClipperFixed)
            {
                rightClipper.transform.parent = brainModel.transform;
            }
            else {
                rightClipper.transform.parent = null;
            }
            rightClipperIndicator.SetActive(false);
        }
        if (left_index & left_hand)
        {
            if (!leftClipperIndicator.activeSelf)
            {
                leftClipper.transform.parent = null;
                leftClipperFixed = false;
                leftClipperIndicator.SetActive(true);
                leftClipper.transform.SetLocalPositionAndRotation(leftController.transform.position, leftController.transform.rotation);
                leftClipper.transform.parent = leftController.transform;
            }
        }
        else
        {
            if (leftClipperFixed)
            {
                leftClipper.transform.parent = brainModel.transform;
            }
            else
            {
                leftClipper.transform.parent = null;
            }
            leftClipperIndicator.SetActive(false);
        }

        // the "fixClip" botton, sets "clip fixed"; if clip fixed, set its parent to the brain object
        if (!isRestart & OVRInput.GetDown(OVRInput.RawButton.X))
        {
            // fix left controller
            leftClipperFixed = true;
            leftClipper.transform.parent = brainModel.transform;

        }
        else if (!isRestart & OVRInput.GetDown(OVRInput.RawButton.A))
        {
            // fix right controller
            rightClipperFixed = true;
            rightClipper.transform.parent = brainModel.transform;
        }

        // mode display show information
        if (leftClipperFixed & !rightClipperFixed)
        {
            ShowModeMsg("Slicer \n Left Clipper Locked");
        }
        else if (rightClipperFixed & !leftClipperFixed)
        {
            ShowModeMsg("Slicer \n Right Clipper Locked");
        }
        else if (rightClipperFixed & leftClipperFixed)
        {
            ShowModeMsg("Slicer \n Left Clipper Locked\n Right Clipper Locked");
        }
    }
    void ResetClippers()
    {
        if (!sliceMode)
        {
            if (!leftClipperFixed)
            {
                leftClipper.transform.parent = null;
                leftClipper.transform.SetPositionAndRotation(clipperPos, clipperRot);
            }
            if (!rightClipperFixed)
            {
                rightClipper.transform.parent = null;
                rightClipper.transform.SetPositionAndRotation(clipperPos, clipperRot);

            }
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
            leftToolMenu.SetActive(false); // this line is to temporarily deal with a bug
            rightToolMenu.SetActive(false);// same reason
            preDronePos = imDrone.transform.position;
            preDroneRot = imDrone.transform.rotation;
            dronePanel.SetActive(true);
            droneDirectionArrow.SetActive(true);

            miniMap.SetActive(true);
            miniMap.transform.SetPositionAndRotation(leftController.transform.position + new Vector3(0, 0.15f * imDrone.transform.localScale.magnitude, 0), leftController.transform.rotation); ;
            miniMap.transform.parent = leftController.transform;

            if (OVRManager.instance.isInsightPassthroughEnabled & !droneToggledPassthrough)
            {
                OVRManager.instance.isInsightPassthroughEnabled = false;
                droneToggledPassthrough = true;
            }
        }
        else
        {
            OperateDrone();
            ComfortToggle();
           
            if ((OVRInput.GetDown(OVRInput.RawButton.X)) & !isRestart)
            {
                miniMap.SetActive(false);
                imDrone.transform.localScale = Vector3.one;
                imDrone.transform.SetPositionAndRotation(preDronePos, preDroneRot);
                isDrone = false;
                droneMode = false;
                comfortPanel.SetActive(false);
                dronePanel.SetActive(false);
                droneDirectionArrow.SetActive(false);
                modePanel.SetActive(false);

                // toggle back passthrough if it was disabled earlier when entering drone mode
                if (droneToggledPassthrough)
                {
                    OVRManager.instance.isInsightPassthroughEnabled = true;
                    droneToggledPassthrough = false;
                }
            }   
        }
    }
    void ComfortToggle() {
        if (OVRInput.GetDown(OVRInput.RawButton.A)) isComfort = !isComfort;
        ShowModeMsg("Drone Mode\n Comfort Mode " + isComfort.ToString());
    }
    void OperateDrone()
    {
        float veloMod = 1;
        if (imDrone.transform.localScale.x <= 1f)
        {
            veloMod = imDrone.transform.localScale.x;
        }

        if (OVRInput.Get(OVRInput.RawButton.LThumbstickUp) |
            OVRInput.Get(OVRInput.RawButton.LThumbstickDown) |
            OVRInput.Get(OVRInput.RawButton.LThumbstickLeft) |
            OVRInput.Get(OVRInput.RawButton.LThumbstickRight) |
            OVRInput.Get(OVRInput.RawButton.RThumbstickUp) |
            OVRInput.Get(OVRInput.RawButton.RThumbstickDown) |
            OVRInput.Get(OVRInput.RawButton.RThumbstickLeft) |
            OVRInput.Get(OVRInput.RawButton.RThumbstickRight))
        {
            comfortPanel.SetActive(isComfort);
        }
        else comfortPanel.SetActive(false);

        //move up and down
        if (OVRInput.Get(OVRInput.RawButton.LThumbstickUp))
        {
            imDrone.transform.position += veloMod * Time.deltaTime * imDrone.transform.up;
        }
        else if (OVRInput.Get(OVRInput.RawButton.LThumbstickDown))
        {
            imDrone.transform.position -= veloMod * Time.deltaTime * imDrone.transform.up;
        }

        //pan left and right
        if (OVRInput.Get(OVRInput.RawButton.LThumbstickRight))
        {
            imDrone.transform.position += veloMod * Time.deltaTime * imDrone.transform.right;
        }
        else if (OVRInput.Get(OVRInput.RawButton.LThumbstickLeft))
        {
            imDrone.transform.position -= veloMod * Time.deltaTime * imDrone.transform.right;
        }

        //forward and back
        if (OVRInput.Get(OVRInput.RawButton.RThumbstickUp))
        {
            imDrone.transform.position += veloMod * Time.deltaTime * imDrone.transform.forward;
        }
        else if (OVRInput.Get(OVRInput.RawButton.RThumbstickDown))
        {
            imDrone.transform.position -= veloMod * Time.deltaTime * imDrone.transform.forward;
        }

        //look left and right
        if (OVRInput.Get(OVRInput.RawButton.RThumbstickRight))
        {
            imDrone.transform.RotateAround(imDrone.transform.position, imDrone.transform.up, 45 * Time.deltaTime);
        }
        else if (OVRInput.Get(OVRInput.RawButton.RThumbstickLeft))
        {
            imDrone.transform.RotateAround(imDrone.transform.position, imDrone.transform.up, -45 * Time.deltaTime);
        }

        // scale my size
        if (right_index & imDrone.transform.localScale.x <= 3f)
        {
            imDrone.transform.localScale *= (1 + Time.deltaTime);
        }
        else if (left_index & imDrone.transform.localScale.x >= 0.33f)
        {
            imDrone.transform.localScale *= (1 - Time.deltaTime);
        }

        droneMsg.text = "coord: " +
                        imDrone.transform.position.x.ToString("F3") + ", " +
                        imDrone.transform.position.y.ToString("F3") + ", " +
                        imDrone.transform.position.z.ToString("F3") + "\n" +
                    "drone scale: " +
                        imDrone.transform.localScale.x.ToString("F3");

        Vector3 babyDroneArrowPos = brainModel.transform.InverseTransformPoint(imDrone.transform.position);
        Quaternion babyDroneArrowRot = Quaternion.Inverse(brainModel.transform.rotation) * imDrone.transform.rotation;
        GameObject theMinimapArrow = Instantiate(posArrow, miniMap.transform.position, Quaternion.identity);
        theMinimapArrow.transform.parent = miniMap.transform;
        theMinimapArrow.transform.SetLocalPositionAndRotation(babyDroneArrowPos, babyDroneArrowRot);
        theMinimapArrow.transform.localScale = 666.7f * iniBrainScale.magnitude * new Vector3(imDrone.transform.localScale.x * 0.66f, imDrone.transform.localScale.y, imDrone.transform.localScale.z) / brainModel.transform.localScale.magnitude; // this is magic... cuz after setting parent the local scale becomes 666.7
        Destroy(theMinimapArrow, 0.02f);
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


    void StoreSnapshot()
    {
        if (droneMode & !isRestart)
        {
            if (OVRInput.GetDown(OVRInput.RawButton.Y))
            {
                Vector3 thePos = brainModel.transform.InverseTransformPoint(imDrone.transform.position);
                AddToMiniMap(thePos);
            }
        }
        else if (singleRayDepthCamMode & !isRestart & cameraIndicatorBall!=null)
        {
            if (OVRInput.GetDown(OVRInput.RawButton.Y))
            {
                Vector3 thePos = brainModel.transform.InverseTransformPoint(cameraIndicatorBall.transform.position);
                AddToMiniMap(thePos);
            }
        }
    }
    void AddToMiniMap(Vector3 pos) {
        GameObject snapshotIb = Instantiate(minimapIndicatorBall, miniMap.transform.position, Quaternion.identity);
        snapshotIb.transform.parent = miniMap.transform;
        snapshotIb.transform.localPosition = pos;
        snapshotIb.GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        // the screenshot size scales with the drone size and model size
        snapshotIb.transform.localScale = 10f * iniBrainScale.magnitude * imDrone.transform.localScale/brainModel.transform.localScale.magnitude;
        // add point to the list
        float[] snapshotEntry = new float[3];
        snapshotEntry[0] = pos.x;
        snapshotEntry[1] = pos.y;
        snapshotEntry[2] = pos.z;
        // snapshotEntry[3] = scale;
        snapshotList.Add(snapshotEntry);
    }
    //// two ray zoom move
    //// cast two rays onto object and scale the size of object based on distance of
    //// left and right ray cast hit points
    //void TwoRayZoom()
    //{
    //    if (right_index & right_hand & left_index & left_hand)
    //    {
    //        if (Physics.Raycast(leftController.transform.position, leftController.transform.forward, out RaycastHit hitL, 3) &
    //        Physics.Raycast(rightController.transform.position, rightController.transform.forward, out RaycastHit hitR, 3))
    //        {

    //            if (hitL.transform.parent.gameObject.CompareTag("Neuron"))
    //            {
    //                GameObject leftIndicatorBall = Instantiate(indicatorBall, hitL.point, Quaternion.identity);
    //                GameObject rightIndicatorBall = Instantiate(indicatorBall, hitR.point, Quaternion.identity);
    //                Destroy(leftIndicatorBall, 0.02f);
    //                Destroy(rightIndicatorBall, 0.02f);

    //                if (!selected)
    //                {
    //                    twoPointDistance = (hitL.point - hitR.point).magnitude;
    //                    selected = true;

    //                }
    //                currentObj = brainModel;

    //                if (selected & twoPointDistance > 0)
    //                {
    //                    currentObj.transform.localScale = Vector3.Lerp(currentObj.transform.localScale,
    //                ((hitL.point - hitR.point).magnitude) * currentObj.transform.localScale / twoPointDistance, 0.05f);
    //                }
    //            }
    //        }
    //    }
    //    else
    //    {
    //        selected = false;
    //        currentObj = null;
    //        twoPointDistance = 0;
    //    }
    //}

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

    //        if (Physics.Raycast(leftController.transform.position, leftController.transform.forward, out RaycastHit hit, 6))
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

}