using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class Select : MonoBehaviour
{
    
    void Start()
    {
        Debug.Log("Start");
    }
    
    void Update()
    {
        // if the mouse was clicked down during the last frame
        if (Input.GetMouseButtonDown(0))
        {
            bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // if the mouse pointer is not over a UI GameObject, and the raycast registered a hit
            if (!isOverUI & Physics.Raycast(ray, out hit))
            {
                if(hit.transform.gameObject.CompareTag("Neuropil"))
                {
                    Debug.Log(hit.transform.parent.name);
                }
            }
        }
    }
}