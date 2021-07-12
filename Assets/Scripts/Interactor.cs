using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityFx.Outline;


public class Interactor : MonoBehaviour
{
    
    public bool isIntersecting = false;
    private List<Interactible> intersectedObjects = new List<Interactible>();
    private List<Interactible> _grabbedObjects = new List<Interactible>();
    public bool isLeftHand = true;
    
    public OVRHand ovrHand;
    private OVRSkeleton handSkeleton;
    private float _dampenedPinchValue = 0.0f;
    private float _pinchVelocity;
    [SerializeField] private float _smoothTime = 0.2f;
    
    /// <summary>
    /// Are we currently doing hand tracking?
    /// </summary>       
    private bool IsHandTracking = false;
    private bool wasHandTracking = false; // used to detect hand tracking toggle

    [SerializeField] private UnityEvent onHandTrackingActive;
    [SerializeField] private UnityEvent onHandTrackingInactive;

    private Vector3 _initialScale;
    private Vector3 _initialPosition = Vector3.zero;
    private OutlineBehaviour _outlineBehaviour;
    private MaterialSwitcher _materialSwitcher;

    private void Start()
    {
        _initialScale = this.transform.localScale;
        _initialPosition = transform.localPosition;
        _outlineBehaviour = GetComponent<OutlineBehaviour>();
        _materialSwitcher = GetComponent<MaterialSwitcher>();

        onHandTrackingActive = new UnityEvent();
        onHandTrackingInactive = new UnityEvent();
    }

    // Update is called once per frame
    void Update()
    {
        
        UpdateHandTrackingStatus();

        if (IsHandTracking)
        {
            SetInteractorTransform();
            DoHandTrackingUpdate();
        }
        else
            DoControllerUpdate();
    }

    private void SetInteractorTransform()
    {
        if (!handSkeleton) handSkeleton = ovrHand.GetComponentInChildren<OVRSkeleton>();
        if (!handSkeleton) return;
        if (handSkeleton.Bones == null) return;
        if (handSkeleton.Bones.Count == 0) return;
        
        var tipTransform  = handSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform;  // index tip position
        
        transform.position = tipTransform.position;
    }

    private void DoControllerUpdate()
    {
        var triggerType = isLeftHand ? OVRInput.Axis1D.PrimaryHandTrigger : OVRInput.Axis1D.SecondaryHandTrigger;
        
        if (OVRInput.Get(triggerType, OVRInput.Controller.Touch) > 0.7f)
        {
            foreach (var obj in intersectedObjects)
            {
                _grabbedObjects.Add(obj);
                obj.OnGrab.Invoke(this);
            }
        }
        else
        {
            var cachedList = new List<Interactible>(_grabbedObjects);
            foreach (var obj in cachedList)
            {
                obj.OffGrab.Invoke(this);
                _grabbedObjects.Remove(obj);
            }
        }
    }

    private void DoHandTrackingUpdate()
    {
        var pinchValue = GetIndexPinch();
        if (pinchValue > 0.8f)
        {
            Debug.Log("Pinch");
            foreach (var obj in intersectedObjects)
            {
                _grabbedObjects.Add(obj);
                obj.OnGrab.Invoke(this);
            }
        }
        else
        {
            Debug.Log("Unpinch");
            
            var cachedList = new List<Interactible>(_grabbedObjects);
            foreach (var obj in cachedList)
            {
                obj.OffGrab.Invoke(this);
                _grabbedObjects.Remove(obj);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactible = other.gameObject.GetComponent<Interactible>();
        if (!interactible) return;
        
        intersectedObjects.Add(interactible);
            
        interactible.OnTouch.Invoke(this);
        //change interactor sphere's material
        GetComponent<MaterialSwitcher>().TurnOnHighlight();
        isIntersecting = true;
    }

    private void OnTriggerExit(Collider other)
    {
        var interactible = other.gameObject.GetComponent<Interactible>();
        if (!interactible) return;
        
        intersectedObjects.Remove(interactible);
            
        interactible.OffTouch.Invoke(this);
        //change interactor sphere's material
        GetComponent<MaterialSwitcher>().TurnOffHighlight();
        isIntersecting = false;
    }


    #region Hand Tracking Code
    
    private void UpdateHandTrackingStatus()
    {
        IsHandTracking = OVRInput.GetActiveController() == OVRInput.Controller.Hands;

        if(IsHandTracking != wasHandTracking) {
            HandTrackingChange(IsHandTracking);
        }

        wasHandTracking = IsHandTracking;
    }
    
    void HandTrackingChange(bool isHandTracking)
    {
        if (isHandTracking)
        {
            transform.localScale = _initialScale * 2;
            _materialSwitcher.enabled = false;
            _outlineBehaviour.enabled = false;
            
            onHandTrackingActive.Invoke();
        }
        else
        {
            _materialSwitcher.enabled = true;
            _outlineBehaviour.enabled = true;
            
            transform.localScale = _initialScale;
            transform.localPosition = _initialPosition;
            
            onHandTrackingInactive.Invoke();
        }
    }
    
    // Returns values [0,1]
    private float GetIndexPinch()
    {
        if (!ovrHand)
        {
            return 0.0f;
        }
        if (!handSkeleton) handSkeleton = ovrHand.GetComponentInChildren<OVRSkeleton>();
            
        UpdateHandTrackingStatus();

        if (!IsHandTracking)
        {
            return 0.0f;
        }

        var handConfidence = ovrHand.GetFingerConfidence(OVRHand.HandFinger.Index);
        var isHandBeingTracked = ovrHand.IsTracked;
            
        if (handConfidence == OVRHand.TrackingConfidence.Low || isHandBeingTracked == false)
        {
            return 0.0f;
        }
        
        float indexFingerPinchStrength = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);

        return indexFingerPinchStrength;
    }
    
    #endregion
}
