using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraspingPoint : MonoBehaviour
{
    public float speed = 2f;
    private float startTime;
    private bool isAttached = false;
    private GameObject interactorObject;

    private void Start()
    {
        startTime = Time.time;
    }

    private void Update()
    {
        if (isAttached)
        {
            float fracComplete = (Time.time - startTime) / speed;
            transform.position = Vector3.Slerp(transform.position, interactorObject.transform.position, fracComplete);
        }
    }

    public void Attach(Interactor interactor)
    {
        isAttached = true;
        interactorObject = interactor.gameObject;
    }
    
    public void UnAttach()
    {
        isAttached = false;
        interactorObject = null;
    }
}
