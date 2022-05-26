using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ControlPanel : MonoBehaviour
{
    private string languageName;
    private List<string> langList = new List<string>();

    [SerializeField] private GameObject langNameTextObject;
    [SerializeField] private GameObject pagesideTextManager;
    private TextMeshPro langNameText;
    public int CurLangNum = 0;
    
    [SerializeField]  private UnityEvent onMusicPress;
    [SerializeField] private GameObject musicObject;
    [SerializeField] private GameObject musicOnOffText;
    private bool musicHasBeenTurnedOff;
    
    [SerializeField] private GameObject AnnotationOnOffText;
    [SerializeField] private GameObject highlightFolder;
    private bool AnnotationHasBeenTurnedOff;
    
    [SerializeField] private GameObject InspectorOnOffText;
    private bool inspectorIsOn = true;
    [SerializeField] private GameObject inspectorPeripheries;

    
    // Start is called before the first frame update
    void Start()
    {
        //fill a list with the names of all language files from the pageside text manager
        List<TextAsset> langAssetList = pagesideTextManager.GetComponent<PagesideTextManager>().LanguageFileList;
        for (int i = 0; i < langAssetList.Count; i++)
        {
            langList.Add(langAssetList[i].name);
        }
        
        langNameText = langNameTextObject.GetComponent<TextMeshPro>();
        langNameText.text = langList[CurLangNum];

        musicHasBeenTurnedOff = false;
        AnnotationHasBeenTurnedOff = false;
    }
    

    public void CycleLanguageName()
    {
        CurLangNum++;
        if (CurLangNum >= langList.Count)
        {
            CurLangNum = 0;
        }
        langNameText.text = langList[CurLangNum];
    }
    
    public void CycleLanguageNameBackwards()
    {
        CurLangNum--;
        if (CurLangNum <= langList.Count)
        {
            CurLangNum = langList.Count;
        }
        langNameText.text = langList[CurLangNum];
    }
    

    public void UpdateMusicOnOff()
    {
        if (musicHasBeenTurnedOff == false)
        {
            musicOnOffText.GetComponent<TextMeshPro>().text = "OFF";
            musicObject.GetComponent<AudioSource>().Pause();
            musicHasBeenTurnedOff = true;
        }
        else
        {
            musicOnOffText.GetComponent<TextMeshPro>().text = "ON";
            musicObject.GetComponent<AudioSource>().Play();
            musicHasBeenTurnedOff = false;
        }
    }
    
    public void UpdateAnnotationsOnOff()
    {
        if (AnnotationHasBeenTurnedOff == false)
        {
            AnnotationOnOffText.GetComponent<TextMeshPro>().text = "OFF";
            highlightFolder.SetActive(false);
            AnnotationHasBeenTurnedOff = true;
        }
        else
        {
            AnnotationOnOffText.GetComponent<TextMeshPro>().text = "ON";
            highlightFolder.SetActive(true);
            AnnotationHasBeenTurnedOff = false;
        }
    }
    
    public void UpdateInspectorOnOff()
    {
        inspectorIsOn = !inspectorIsOn;
        if (inspectorIsOn == false)
        {
            InspectorOnOffText.GetComponent<TextMeshPro>().text = "OFF";
            inspectorPeripheries.SetActive(false);
        }
        else
        {
            InspectorOnOffText.GetComponent<TextMeshPro>().text = "ON";
            inspectorPeripheries.SetActive(true);

        }
        
    }

    public void ResetExperienceCP()
    {
        //translation
        CurLangNum = 0;
        langNameText.text = langList[CurLangNum];
        
        //music
        musicOnOffText.GetComponent<TextMeshPro>().text = "ON";
        musicObject.GetComponent<AudioSource>().Play();
        musicHasBeenTurnedOff = false;
        
        //page
        //done in book manager
        
        //annotation
        AnnotationOnOffText.GetComponent<TextMeshPro>().text = "ON";
        highlightFolder.SetActive(true);
        AnnotationHasBeenTurnedOff = false;
    }
    
}
