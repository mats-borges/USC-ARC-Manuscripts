using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerTest : MonoBehaviour
{

    [SerializeField] private GameObject childObject;
    private bool isParented = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger, OVRInput.Controller.Touch) > 0.8f)
        {
            if (childObject)
            {
                childObject.transform.SetParent(this.transform);
                isParented = true;
            }
        }
        else
        {
            if (isParented)
            {
                childObject.transform.SetParent(null, true);
                isParented = false;
            }
        }
    }
}
