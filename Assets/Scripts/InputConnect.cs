using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputConnect : MonoBehaviour
{

    [SerializeField] private GameObject childObject;
    private bool isParented = false;
    
    /// <summary>
    /// Are we currently doing hand tracking?
    /// </summary>       
    private bool IsHandTracking = false;
    private bool wasHandTracking = false; // used to detect hand tracking toggle
    
    
    public OVRHand hand;
    /// <summary>
    /// The hand to listen to state changes on.
    /// </summary>
    protected OVRHand Hand { get => hand; set => hand = value; }

    private OVRSkeleton handSkeleton;
    private float _dampenedPinchValue = 0.0f;
    private float _pinchVelocity;
    [SerializeField] private float _smoothTime = 0.2f;
    private Transform targetToFollow;

    // Update is called once per frame
    void Update()
    {
        var pinchValue = GetIndexPinch();
        if (pinchValue > 0.8f)
        {
            var tipTransform  = handSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform;  // index tip position
            if (childObject)
            {
                childObject.transform.SetParent(tipTransform);
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
    
    // Returns values [0,1]
    private float GetIndexPinch()
    {
        if (!hand)
        {
            return 0.0f;
        }
        if (!handSkeleton) handSkeleton = hand.GetComponentInChildren<OVRSkeleton>();
            
        UpdateHandTracking();

        if (!IsHandTracking)
        {
            return 0.0f;
        }

        var handConfidence = hand.GetFingerConfidence(OVRHand.HandFinger.Index);
        var isHandBeingTracked = hand.IsTracked;
            
        if (handConfidence == OVRHand.TrackingConfidence.Low || isHandBeingTracked == false)
        {
            return 0.0f;
        }
        
        float indexFingerPinchStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);

        return indexFingerPinchStrength;
    }

    private float GetHandPinch()
    {
        if (!hand)
        {
            return 0.0f;
        }
        if (!handSkeleton) handSkeleton = hand.GetComponentInChildren<OVRSkeleton>();
            
        UpdateHandTracking();

        if (!IsHandTracking)
        {
            return 0.0f;
        }

        var handConfidence = hand.GetFingerConfidence(OVRHand.HandFinger.Index);
        var isHandBeingTracked = hand.IsTracked;
            
        if (handConfidence == OVRHand.TrackingConfidence.Low || isHandBeingTracked == false)
        {
            return 0.0f;
        }

        Transform Hand_Root = handSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Start].Transform;

        var tipTransforms = new Transform[4];

        var Hand_Thumb0  = handSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Thumb0].Transform;    // thumb trapezium bone
            
        tipTransforms[0]  = handSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip].Transform;  // index tip position
        tipTransforms[1] = handSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_MiddleTip].Transform; // middle tip position
        tipTransforms[2]   = handSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_RingTip].Transform;   // ring tip position
        tipTransforms[3]  = handSkeleton.Bones[(int)OVRSkeleton.BoneId.Hand_PinkyTip].Transform;  // pinky tip position

        // Calculate the sum of all squared distances between each finger tip and the wrist
        // TODO (Note): Do we need to consider the different sizes of hands when taking in this calculation?
        //              If so, how can we normalize this?
        var sumSquares = 0.0f;
        for (var i = 0; i < tipTransforms.Length; ++i)
        {
            var square = Vector3.Distance(tipTransforms[i].position, Hand_Root.position);
            square *= square;
                
            sumSquares += square;
        }

        // Remap the sumSquare values into a 0...1 range so that it can be used to trigger actions
        // TODO (Note): Values were observed based off my personal hand, again, do we need to consider different hand sizes?
        float normal = Mathf.InverseLerp(0.015f, 0.119f, sumSquares);
        float bValue = Mathf.Lerp(0, 1, normal);
        bValue = 1 - bValue;

        // // BELOW -- Getting pinch strength, was used but looking to remove
        // var pinchStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
            
        _dampenedPinchValue = Mathf.SmoothDamp(_dampenedPinchValue, bValue, ref _pinchVelocity, _smoothTime);

        return _dampenedPinchValue;
    }
    
    private void UpdateHandTracking()
    {
        IsHandTracking = OVRInput.GetActiveController() == OVRInput.Controller.Hands;

        if(IsHandTracking != wasHandTracking) {
            onHandTrackingChange(IsHandTracking);
        }

        wasHandTracking = IsHandTracking;
    }
    
    void onHandTrackingChange(bool isHandTracking)
    {
        // Do something if hand tracking status changes?
    }
}