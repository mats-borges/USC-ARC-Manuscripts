using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class inspectorModeButton : MonoBehaviour
{
    [SerializeField] private GameObject previousButton;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject modeButtonText;
    private bool isPaired = true;

    public void ToggleInspectorMode()
    {
        isPaired = !isPaired; 
        if (isPaired)
        {
            modeButtonText.GetComponent<TextMeshPro>().text = "PAIRED";
            previousButton.SetActive(false);
            nextButton.SetActive(false);
        }
        else
        {
            modeButtonText.GetComponent<TextMeshPro>().text = "UNPAIRED";
            previousButton.SetActive(true);
            nextButton.SetActive(true);
        }
        
        
    }
}
