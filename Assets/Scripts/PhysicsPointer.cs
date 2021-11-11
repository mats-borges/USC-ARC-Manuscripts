using System;
using System.Collections;
using System.Collections.Generic;
using HandPhysicsToolkit.Helpers.Interfaces;
using UnityEngine;

public class PhysicsPointer : MonoBehaviour, BaseInteractor
{
    private int layerMask = 1 << 3; // only allow layer 3 (LaserUI) for raycasts
    
    public float defaultLength = 3.0f;

    private LineRenderer lineRenderer = null;
    [SerializeField] private GameObject startPointObj;

    private Interactible hitObject = null;
    [SerializeField] private Interactor sphereInteractor; 

    public bool isLeftHand = true;

    private bool wasClickPressedLastFrame = false;

    private bool isLROn = false;
    private bool MasterisLROn = true;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        if (sphereInteractor != null)
        {
            sphereInteractor.onHandTrackingActive.AddListener(OnHandTrackingOn);
            sphereInteractor.onHandTrackingInactive.AddListener(OnHandTrackingOff);
        }
    }

    private void Update()
    {
        UpdateLength();
        CheckPointerClick();
        if (isLROn && MasterisLROn)
        {
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    private void CheckPointerClick()
    {
        var triggerType = isLeftHand ? OVRInput.Axis1D.PrimaryHandTrigger : OVRInput.Axis1D.SecondaryHandTrigger;
        var controllerHandedness = isLeftHand ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        
        if (OVRInput.Get(triggerType, OVRInput.Controller.Touch) > 0.7f || OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controllerHandedness) > 0.7f )
        {
            if (wasClickPressedLastFrame == false)
            {
                if (hitObject)
                {
                    DoClick(hitObject);
                }
            }

            wasClickPressedLastFrame = true;
        }
        else
        {
            if (hitObject)
            {
                DoUnclick(hitObject);
            }
            wasClickPressedLastFrame = false;
        }
    }
    
    private void DoClick(Interactible objtoGrab)
    {
        objtoGrab.OnPointerClick.Invoke(this);
    }
    
    private void DoUnclick(Interactible objtoGrab)
    {
        objtoGrab.OnPointerUnclick.Invoke(this);
    }

    void UpdateLength()
    {
         lineRenderer.SetPosition(0,startPointObj.transform.position);
         lineRenderer.SetPosition(1,CalculateEnd());
    }

    Vector3 CalculateEnd()
    {
        RaycastHit hit = CreateForwardRaycast();
        Vector3 endPosition = DefaultEnd(defaultLength);

        if (hit.collider)
        {
            isLROn = true;
            
            endPosition = hit.point;
            
            var interactible = hit.collider.gameObject.GetComponent<Interactible>();
            
            AddInteractible(interactible);
        }
        else
        {
            isLROn = false;
            
            RemoveInteractible();
        }
        return endPosition;
    }

    private void AddInteractible(Interactible interactible)
    {
        if (!interactible)
        {
            RemoveInteractible();
        }
        else
        {
            // if there was previously an object that the pointer was over, remove it
            if (hitObject != null)
            {
                RemoveInteractible();
            }
            hitObject = interactible;
            interactible.OnPointerOver.Invoke(this);
        }
    }

    private void RemoveInteractible()
    {
        if (!hitObject) return;
        
        hitObject.OnPointerExit.Invoke(this);
        hitObject = null;
    }

    RaycastHit CreateForwardRaycast()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);

        Physics.Raycast(ray, out hit, defaultLength, layerMask);
        
        return hit;

    }

    Vector3 DefaultEnd(float length)
    {
        return transform.position + (transform.forward * length);
    }
    //CHECK this when implementing handtracking laser - it only turns off the line renderer, not the actual raycast
    void OnHandTrackingOn()
    {
        MasterisLROn = false;
    }
    void OnHandTrackingOff()
    {
        MasterisLROn = true;
    }
    
}
