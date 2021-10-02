using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class IntroWall : MonoBehaviour
{
    [SerializeField] private GameObject textWall;
    [SerializeField] private GameObject introWallParent;
    [SerializeField] private GameObject spriteWall;

    [SerializeField, TextArea] List<string> textList = new List<string>();
    [SerializeField] List<Sprite> spriteList = new List<Sprite>();

    private int currentIndex = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateTextWall();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) || OVRInput.GetDown(OVRInput.Button.One))
        {
            if (currentIndex < textList.Count - 1)
            {
                currentIndex++;
                UpdateTextWall();
            }
            else
            {
                introWallParent.SetActive(false);
            }
            
        }
    }

    void UpdateTextWall()
    {
        textWall.GetComponent<TextMeshPro>().text = textList[currentIndex].Replace("@n", Environment.NewLine);
        spriteWall.GetComponent<SpriteRenderer>().sprite = spriteList[currentIndex];
    }

    public void Restart()
    {
        currentIndex = 0;
        introWallParent.SetActive(true);
        UpdateTextWall();
    }
}
