using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    
    public bool isIntersecting = false;
    private List<Interactible> intersectedObjects = new List<Interactible>();
    public bool isLeftHand = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isIntersecting)
            return;

        var triggerType = isLeftHand ? OVRInput.Axis1D.PrimaryHandTrigger : OVRInput.Axis1D.SecondaryHandTrigger;
        
        if (OVRInput.Get(triggerType, OVRInput.Controller.Touch) > 0.8f)
        {
            foreach (var obj in intersectedObjects)
            {
                obj.OnGrab.Invoke(this);
            }
        }
        else
        {
            foreach (var obj in intersectedObjects)
            {
                obj.OffGrab.Invoke(this);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactible = other.gameObject.GetComponent<Interactible>();

        if (interactible)
        {
            intersectedObjects.Add(interactible);
            
            interactible.OnTouch.Invoke();
            isIntersecting = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var interactible = other.gameObject.GetComponent<Interactible>();

        if (interactible)
        {
            intersectedObjects.Remove(interactible);
            
            interactible.OffTouch.Invoke();
            isIntersecting = false;
        }
    }
}
