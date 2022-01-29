using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using HandPhysicsToolkit.Helpers.Interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityFx.Outline;


public class Interactor : MonoBehaviour, BaseInteractor
{
    
    public bool isIntersecting = false;
    private List<Interactible> intersectedObjects = new List<Interactible>();
    private List<Interactible> _grabbedObjects = new List<Interactible>();
    public bool isLeftHand = true;
    
    public Color _highlightOnTouch = Color.cyan;
    public Color _highlightOnGrab = Color.green;

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

    [SerializeField] public UnityEvent onHandTrackingActive;
    [SerializeField] public UnityEvent onHandTrackingInactive;
    private bool isGrabbing = false;

    private Vector3 _initialScale;
    private Vector3 _initialPosition = Vector3.zero;
    private OutlineBehaviour _outlineBehaviour;
    private MaterialSwitcher _materialSwitcher;

    private void Awake()
    {
        _initialScale = this.transform.localScale;
        _initialPosition = transform.localPosition;
        _outlineBehaviour = GetComponent<OutlineBehaviour>();
        _materialSwitcher = GetComponent<MaterialSwitcher>();

        onHandTrackingActive = new UnityEvent();
        onHandTrackingInactive = new UnityEvent();
    }

    private void Start()
    {
        StartCoroutine(DelayedHandTrackingEventInvoke());
    }

    IEnumerator DelayedHandTrackingEventInvoke()
    {
        yield return new WaitForSecondsRealtime(3);
        
        IsHandTracking = OVRInput.GetActiveController() == OVRInput.Controller.Hands;
        HandTrackingChange(IsHandTracking);
        wasHandTracking = IsHandTracking;
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

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    private void SetInteractorTransform()
    {
        transform.position = GetBonePosition(OVRSkeleton.BoneId.Hand_IndexTip);
    }

    public Vector3 GetBonePosition(OVRSkeleton.BoneId boneId)
    {
        if (!handSkeleton) handSkeleton = ovrHand.GetComponentInChildren<OVRSkeleton>();
        if (!handSkeleton) return Vector3.zero;
        if (handSkeleton.Bones == null) return Vector3.zero;
        
        return handSkeleton.Bones.Count == 0 ? Vector3.zero : handSkeleton.Bones[(int) boneId].Transform.position;
    }

    public Transform GetPointerPose()
    {
        if (!ovrHand) return null;
        return ovrHand.IsPointerPoseValid ? ovrHand.PointerPose : null;
    }

    private void DoControllerUpdate()
    {
        var triggerType = isLeftHand ? OVRInput.Axis1D.PrimaryHandTrigger : OVRInput.Axis1D.SecondaryHandTrigger;

        var controllerHandedness = isLeftHand ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        
        if (OVRInput.Get(triggerType, OVRInput.Controller.Touch) > 0.7f || OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controllerHandedness) > 0.7f )
        {
            foreach (var obj in intersectedObjects)
            {
                DoGrab(obj);
            }
        }
        else
        {
            var cachedList = new List<Interactible>(_grabbedObjects);
            foreach (var obj in cachedList)
            {
                DoUngrab(obj);
            }
        }
    }

    private void DoGrab(Interactible objtoGrab)
    {
        _grabbedObjects.Add(objtoGrab);
        objtoGrab.OnGrab.Invoke(this);
        isGrabbing = true;
        
        GetComponent<MaterialSwitcher>().TurnOnHighlight(_highlightOnGrab);
    }

    private void DoUngrab(Interactible objtoGrab)
    {
        objtoGrab.OffGrab.Invoke(this);
        _grabbedObjects.Remove(objtoGrab);
        isGrabbing = false;
        
        GetComponent<MaterialSwitcher>().TurnOffHighlight();
    }

    private void DoHandTrackingUpdate()
    {
        var pinchValue = GetIndexPinch();
        if (pinchValue > 0.8f)
        {
            Debug.Log("Pinch");
            foreach (var obj in intersectedObjects)
            {
                DoGrab(obj);
            }
        }
        else
        {
            Debug.Log("Unpinch");
            
            var cachedList = new List<Interactible>(_grabbedObjects);
            foreach (var obj in cachedList)
            {
                DoUngrab(obj);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactible = other.gameObject.GetComponent<Interactible>();
        if (!interactible) return;
        
        isIntersecting = true;
        intersectedObjects.Add(interactible);
            
        interactible.OnTouch.Invoke(this);

        //change interactor sphere's material
        if(!isGrabbing)
            GetComponent<MaterialSwitcher>().TurnOnHighlight(_highlightOnTouch);
    }

    private void OnTriggerExit(Collider other)
    {
        var interactible = other.gameObject.GetComponent<Interactible>();
        if (!interactible) return;
        
        isIntersecting = false;
        intersectedObjects.Remove(interactible);
            
        interactible.OffTouch.Invoke(this);
        
        //change interactor sphere's material
        if(!isGrabbing)
            GetComponent<MaterialSwitcher>().TurnOffHighlight();
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
            
            // turn outline off
            gameObject.layer = LayerMask.NameToLayer("Default");
            
            onHandTrackingActive.Invoke();
        }
        else
        {
            _materialSwitcher.enabled = true;
            
            // turn outline on
            gameObject.layer = LayerMask.NameToLayer("OutlineLayer");
            
            transform.localScale = _initialScale;
            transform.localPosition = _initialPosition;
            onHandTrackingInactive.Invoke();
        }
    }
    
    // Returns values [0,1]
    public float GetIndexPinch()
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
