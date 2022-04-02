using System.Collections;
using System.Collections.Generic;
using HandPhysicsToolkit.Helpers.Interfaces;
using UnityEngine;
using UnityEngine.Events;

public class UnturnableCollider : MonoBehaviour
{
    //check if player is trying to grab
    //check if grabber is close
    //display message
    //disappear message

    [SerializeField] private GameObject messageText;
    [SerializeField] private GameObject objToCheck;
    [SerializeField] private GameObject otherMessage;
    
    public float minDist = 5f;

    //InteractibleEvent is defined in Interactible.cs
    [SerializeField] private InteractibleEvent pageMagic;
    
    public void PageMagicCheck(BaseInteractor interactor)
    {
        Vector3 dist = transform.position - objToCheck.transform.position;
        if (dist.magnitude >= minDist)
        {
            //MessageDisplay();
            
            pageMagic.Invoke(interactor);
        }
    }
    
    void MessageDisplay()
    {
        if (otherMessage.activeInHierarchy == false)
        {
            messageText.SetActive(true);
            StartCoroutine(HideText());
        }
    }
    
    private IEnumerator HideText()
    {
        if (otherMessage.activeInHierarchy)
        {
            messageText.SetActive(false);
        }
        yield return new WaitForSeconds(5f);
        messageText.SetActive(false);
    }
}
