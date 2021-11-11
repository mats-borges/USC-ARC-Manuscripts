using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PagesideTextManager : MonoBehaviour
{
    
    //take in language text files 
    //store them in a useful way (array of strings)
    //display stored texts
    //change what page we're on (including sim page position)
    //change what language we're on
    
    //0 will be empty
    [SerializeField] public List<TextAsset> LanguageFileList = new List<TextAsset>();
    
    [SerializeField] private GameObject leftTextField;
    [SerializeField] private GameObject rightTextField;
    [SerializeField] private GameObject bookManager;
    [SerializeField] private GameObject pageMarker;
    [SerializeField] private GameObject controlPanel;

    private List<string> LanguageTexts;
    private int pageNum;
    private int langNum = 0;
    private bool simPageIsTurned = false;
    private bool simPageWasTurnedLastFrame = false;
    
    enum pageRegion {LEFT, RIGHT};

    private pageRegion thisFrame, lastFrame;

    private void Start()
    {
        langNum = controlPanel.GetComponent<ControlPanel>().CurLangNum;
        pageNum = bookManager.GetComponent<BookManager>().leftPageNum;
        displayTexts();

        thisFrame = pageRegion.RIGHT;
        lastFrame = pageRegion.RIGHT;
    }

    public void IncrementLangNum()
    {
        langNum++;
        if (langNum > LanguageFileList.Count-1)
        {
            langNum = 0;
        }
        displayTexts();
    }

    public void DecrementLangNum()
    {
        langNum--;
        if (langNum < 0)
        {
            langNum = LanguageFileList.Count;
        } 
        displayTexts();
    }

    public void IncrementPageNum()
    {
        pageNum++;
        pageNum++;
        
        if (pageNum > LanguageFileList.Count)
        {
            pageNum = 0;
        }
        displayTexts();
        
    }

    public void DecrementPageNum()
    {
        pageNum--;
        pageNum--;
        
        if (pageNum < 0)
        {
            pageNum = LanguageFileList.Count;
        }
        displayTexts();
    }

    void displayTexts()
    {
        if (langNum > 0)
        {
            //take in language text file
            //make an array of individual pages
            string[] splitArray = null;
            string fullText = LanguageFileList[langNum].ToString();
            string[] separator = new string[]{"/next/"};
            splitArray = fullText.Split(separator,9000,StringSplitOptions.None);

            for (int i = 0; i < splitArray.Length; i++)
            {
                Debug.Log("\"" + splitArray[i] + "\", ");
            }
            
        
            //display pages
            leftTextField.GetComponent<TextMeshPro>().text = splitArray[pageNum];
            rightTextField.GetComponent<TextMeshPro>().text = splitArray[pageNum+1];
        }
        else
        {
            leftTextField.GetComponent<TextMeshPro>().text = " ";
            rightTextField.GetComponent<TextMeshPro>().text = " ";

        }
    }

    private void Update()
    {
        //two enum variables, one for this frame, other for last frame, the type of mismatch determines when the increment and decrement functions are called
        if (pageMarker.transform.position.x < bookManager.transform.position.x )
        {
            thisFrame = pageRegion.LEFT;
        }
        else
        {
            thisFrame = pageRegion.RIGHT;
        }

        if (thisFrame == pageRegion.LEFT && lastFrame == pageRegion.RIGHT)
        {
            IncrementPageNum();
        }
        if (thisFrame == pageRegion.RIGHT && lastFrame == pageRegion.LEFT)
        {
            DecrementPageNum();        
        }
        
        lastFrame = thisFrame;
    }
}
