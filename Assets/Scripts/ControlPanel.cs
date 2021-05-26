using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ControlPanel : MonoBehaviour
{
    private string languageName;
    private List<string> langList = new List<string>() {"LATIN", "ENGLISH", "SPANISH", "OFF"};
    [SerializeField] private GameObject LLatin, RLatin, LEnglish, REnglish, LSpanish, RSpanish;
    [SerializeField] private GameObject langNameTextObject;
    private TextMesh langNameText;
    private int i = 3;
    
    [SerializeField]  private UnityEvent onMusicPress;
    [SerializeField] private GameObject musicObject;
    [SerializeField] private GameObject musicOnOffText;
    private bool musicHasBeenTurnedOff;

    
    // Start is called before the first frame update
    void Start()
    {
        languageName = langList[3];
        langNameText = langNameTextObject.GetComponent<TextMesh>();
        langNameText.text = languageName;
        UpdateLanguageObjects();
        musicHasBeenTurnedOff = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            CycleLanguageName();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            onMusicPress.Invoke();   
            
        }
    }

    public void CycleLanguageName()
    {
        i++;
        if (i >= langList.Count)
        {
            i = 0;
        }
        langNameText.text = langList[i];
        UpdateLanguageObjects();
    }
    
    public void CycleLanguageNameBackwards()
    {
        i--;
        if (i <= langList.Count)
        {
            i = langList.Count;
        }
        langNameText.text = langList[i];
        UpdateLanguageObjects();
    }

    public void UpdateLanguageObjects()
    {
        switch (i)
        {
            case 0:
                LLatin.SetActive(true);
                RLatin.SetActive(true);
                LEnglish.SetActive(false);
                REnglish.SetActive(false);
                LSpanish.SetActive(false);
                LSpanish.SetActive(false);
                break;
            case 1:
                LLatin.SetActive(false);
                RLatin.SetActive(false);
                LEnglish.SetActive(true);
                REnglish.SetActive(true);
                LSpanish.SetActive(false);
                LSpanish.SetActive(false);
                break;
            case 2:
                LLatin.SetActive(false);
                RLatin.SetActive(false);
                LEnglish.SetActive(false);
                REnglish.SetActive(false);
                LSpanish.SetActive(true);
                RSpanish.SetActive(true);
                break;
            case 3:
                LLatin.SetActive(false);
                RLatin.SetActive(false);
                LEnglish.SetActive(false);
                REnglish.SetActive(false);
                LSpanish.SetActive(false);
                RSpanish.SetActive(false);
                break;
            default:
                break;
        }
    }

    public void UpdateMusicOnOff()
    {
        if (musicHasBeenTurnedOff == false)
        {
            musicOnOffText.GetComponent<TextMesh>().text = "OFF";
            musicObject.GetComponent<AudioSource>().Pause();
            musicHasBeenTurnedOff = true;
        }
        else
        {
            musicOnOffText.GetComponent<TextMesh>().text = "ON";
            musicObject.GetComponent<AudioSource>().Play();
            musicHasBeenTurnedOff = false;
        }
    }
}
