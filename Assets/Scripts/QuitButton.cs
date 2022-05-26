using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuitButton : MonoBehaviour
{
    [SerializeField] private GameObject text;
    private bool readyToQuit = false;

    public void StartQuitSequence()
    {
        if (readyToQuit)
        {
            Application.Quit();
            Debug.Log("QUIT!");
        }

        text.GetComponent<TextMeshPro>().text = "PRESS AGAIN \nTO QUIT";
        text.GetComponent<TextMeshPro>().fontSize = 28;
        
        readyToQuit = true;
        StartCoroutine("StartQuitConfirmTimer");


    }
    
    IEnumerator StartQuitConfirmTimer() 
    {
        yield return new WaitForSeconds(5);
        text.GetComponent<TextMeshPro>().text = "QUIT APP";
        text.GetComponent<TextMeshPro>().fontSize = 41;
        
        readyToQuit = false;
    }
}
