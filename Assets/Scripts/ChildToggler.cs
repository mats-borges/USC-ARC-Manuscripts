using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFx.Outline;

public class ChildToggler : MonoBehaviour
{
    private OutlineBehaviour[] children;
    private int curIndex = -1;
    
    // Start is called before the first frame update
    void Start()
    {
        children = GetComponentsInChildren<OutlineBehaviour>();
        foreach (var child in children)
        {
            child.enabled = false;
            child.OutlineRenderMode = OutlineRenderFlags.EnableDepthTesting;
        }

        if (children.Length > 0)
        {
            curIndex = 0;
            children[curIndex].enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Two) && curIndex != -1)
        {
            Debug.Log("toggled child");
            children[curIndex].enabled = false;

            if (curIndex == children.Length - 1)
            {
                curIndex = 0;
            }
            else
            {
                curIndex++;
            }

            children[curIndex].enabled = true;
        }
    }
}
