using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


public class PagesideTextManager : MonoBehaviour
{
    //controls the transcriptions and translations beside the book
    
    //take in language text files 
    //store them in a useful way (array of strings)
    //display stored texts
    //change what page we're on (including sim page position)
    //change what language we're on
    
    [SerializeField] public List<TextAsset> LanguageFileList = new List<TextAsset>();
    //0 will be empty
    [SerializeField] private GameObject leftTextField;
    [SerializeField] private GameObject rightTextField;
    [SerializeField] private GameObject bookManager;
    [SerializeField] private GameObject pageMarker;
    [SerializeField] private GameObject controlPanel;

    private List<string> LanguageTexts;
    private int pageNum;
    private int totalPages = 3;
    private int langNum = 0;

    enum pageRegion {LEFT, RIGHT};

    private pageRegion thisFrame, lastFrame;
    
    [SerializeField] public UnityEvent SimPageTurnLeft;
    [SerializeField] public UnityEvent SimPageTurnRight;

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
        
        if (pageNum >= totalPages)
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
            pageNum = (totalPages + pageNum) ;
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
            totalPages = splitArray.Length;

            for (int i = 0; i < splitArray.Length; i++)
            {
                Debug.Log("\"" + splitArray[i] + "\", ");
            }

            //display pages
            leftTextField.GetComponent<TextMeshPro>().text = splitArray[pageNum];
            rightTextField.GetComponent<TextMeshPro>().text = splitArray[pageNum + 1];
            
        }
        else
        {
            leftTextField.GetComponent<TextMeshPro>().text = " ";
            rightTextField.GetComponent<TextMeshPro>().text = " ";
        }
    }

    private void Update()
    {
        //two enum variables, one for this frame, other for last frame. The type of mismatch determines when the increment and decrement functions are called
        if (pageMarker.transform.position.x < bookManager.transform.position.x )
        {
            thisFrame = pageRegion.LEFT;
        }
        else
        {
            thisFrame = pageRegion.RIGHT;
        }
        
        if ( lastFrame == pageRegion.LEFT && thisFrame == pageRegion.RIGHT)
        {
            DecrementPageNum();
            //increment the page inspector also if it's paired
            if (bookManager.GetComponent<BookManager>().pairedMode)
            {
                Debug.Log("it's getting turned right");
                SimPageTurnRight.Invoke();
            }
        }
        if ( lastFrame == pageRegion.RIGHT && thisFrame == pageRegion.LEFT)
        {
            IncrementPageNum();
            //decrement the page inspector also if it's paired
            if (bookManager.GetComponent<BookManager>().pairedMode)
            {
                Debug.Log("it's getting turned left");
                SimPageTurnLeft.Invoke();
            }
        }

        lastFrame = thisFrame;
    }

    public void ResetExperiencePTM()
    {
        //safely move the pagemarker to the other side without triggering an increment
        pageMarker.transform.SetPositionAndRotation(new Vector3(bookManager.transform.position.x + 5, pageMarker.transform.position.y,pageMarker.transform.position.z), Quaternion.identity);
        lastFrame = pageRegion.RIGHT;
        
        //reset the language and page numbers
        langNum = controlPanel.GetComponent<ControlPanel>().CurLangNum;
        pageNum = bookManager.GetComponent<BookManager>().leftPageNum;
        displayTexts();
    }
}
