using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitButton : MonoBehaviour
{
    [SerializeField] private GameObject text;
    private bool readyToQuit = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartQuitSequence()
    {
        if (readyToQuit)
        {
            Application.Quit();
            Debug.Log("QUIT!");
        }

        text.GetComponent<TextMesh>().text = "PRESS AGAIN \nTO QUIT";
        text.GetComponent<TextMesh>().fontSize = 48;
        
        readyToQuit = true;
        StartCoroutine("StartQuitConfirmTimer");


    }
    
    IEnumerator StartQuitConfirmTimer() 
    {
        yield return new WaitForSeconds(5);
        text.GetComponent<TextMesh>().text = "QUIT APP";
        text.GetComponent<TextMesh>().fontSize = 71;
        
        readyToQuit = false;
    }
}
