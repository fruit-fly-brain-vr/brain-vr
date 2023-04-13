using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UIElements;

public class UserCtrl : MonoBehaviour
{
    public GameObject leftController;
    public GameObject rightController;

    // Prefab objects
    //public GameObject club;
    public Material highlightMat;

    // Panel objects
    public GameObject instructionPanel;//handles quit game or win game
    public TextMeshProUGUI instructionMsg;
    public GameObject restartPanel;

    // miscellaneous
    private static string filePath;
    private bool isRestart = false;
     
    // Start is called before the first frame update
    void Start()
    {
        //filePath = Application.persistentDataPath + "/golf_course.csv";
        ShowInstructionMsg("Interaction Demo open Left/Right Tool Menu by\n pressing Left/Right Thunbstick");
        restartPanel.SetActive(false);
        //DebugFunction();
    }

    void Update()
    {
        LoadObject();
        Restart();
    }

    void LoadObject()
    {
        
    }

    // this method updates the displayed message and instruction
    void ShowInstructionMsg(string theMsg) {
        instructionPanel.SetActive(true);
        instructionMsg.text=theMsg;
    }


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


    void LoadFile()
    {
        //to be added
    }

}

