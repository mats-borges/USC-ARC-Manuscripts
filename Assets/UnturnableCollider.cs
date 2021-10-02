using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    
    public void ObjectCheck()
    {
        Vector3 dist = transform.position - objToCheck.transform.position;
        if (dist.magnitude >= minDist)
        {
            MessageDisplay();
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
