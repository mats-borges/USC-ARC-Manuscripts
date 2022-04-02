using System.Collections;
using System.Collections.Generic;
using System.Data;
using HandPhysicsToolkit.Helpers.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

public class FollowObject : MonoBehaviour
{
    private Transform targetObject;
    private bool _isHandTracking = false;
    private bool _isLeftHand = false;
    private BaseInteractor currentInteractor = null;
    
    [Header("Controller Offset")]
    [FormerlySerializedAs("pinchPosOffset")] [SerializeField] private Vector3 controllerPosOffset;
    [FormerlySerializedAs("pinchRotOffset")] [SerializeField] private Vector3 controllerRotOffset;
    
    
    [Header("Handtracking Offset")]
    [FormerlySerializedAs("gripPosOffset")] [SerializeField] private Vector3 handtrackingPosOffset;
    [FormerlySerializedAs("gripRotOffset")] [SerializeField] private Vector3 handtrackingRotOffset;
    

    // Update is called once per frame
    void Update()
    {
        if (targetObject)
        {
            Vector3 newPos = targetObject.transform.position;
            _isHandTracking = currentInteractor.GetIsHandTracking();
            
            var posOffset = (!_isHandTracking) ? controllerPosOffset : handtrackingPosOffset;
            var rotOffset = (!_isHandTracking) ? controllerRotOffset : handtrackingRotOffset;
            
            transform.position = Vector3.MoveTowards(transform.position, newPos, 100000f);
            transform.position += posOffset;

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetObject.rotation,360);

            if (!_isLeftHand && _isHandTracking)
            {
                rotOffset.x -= 180f;
                rotOffset.z -= 180f;
            }
            transform.Rotate(rotOffset);
        }
    }

    public void RegisterTarget(BaseInteractor interactor)
    {
        if (targetObject != null) return;

        targetObject = interactor.GetGameObject().transform;
        currentInteractor = interactor;

        _isLeftHand = interactor.GetIsLeftHand();
    }
    
    public void UnregisterTarget()
    {
        if (targetObject == null) return;
        
        targetObject = null;
        currentInteractor = null;
    }
}
