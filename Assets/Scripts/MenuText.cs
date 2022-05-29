using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuText : MonoBehaviour
{
    
    //controls the name of the folio displayed in the control panel
    
    //takes in list of names from inspector
    [SerializeField] private List<string> stateList = new List<string>();
    private int stateNum = 0;

    private void Start()
    {
        if (stateList.Count > 0)
        {
            GetComponent<TextMesh>().text = stateList[0];
        }
    }

    public void IncrementState()
    {
        stateNum++;
        if (stateNum >= stateList.Count)
        {
            stateNum = 0;
        }
        GetComponent<TextMesh>().text = stateList[stateNum];
    }
    
    public void DecrementState()
    {
        stateNum--;
        if (stateNum < 0)
        {
            stateNum = stateList.Count-1;
        }
        GetComponent<TextMesh>().text = stateList[stateNum];
    }

    public void ResetExperienceMT()
    {
        stateNum = 0;
        GetComponent<TextMesh>().text = stateList[stateNum];
    }
}
