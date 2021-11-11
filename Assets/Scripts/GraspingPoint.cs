using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Drawing;
using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Helpers.Interfaces;
using Obi;
using UnityEngine;
using UnityEngine.Serialization;

public class GraspingPoint : MonoBehaviour
{
    [FormerlySerializedAs("speed")] public float followSpeed = 2f;
    private float startTime;
    private bool isAttached = false;
    private GameObject interactorObject;
    [SerializeField] private GameObject page;
    private Vector3 targetParticlePos;
    private ObiActor actor;
    private ObiParticleGroup _pageEdgeParticleGroup;
    private ObiParticleGroup _grabbedParticleGroup;
    [SerializeField] private string pageEdgeParticleGroupName;
    [SerializeField] private string grabbedParticleGroupName;
    private int _grabbedParticleIdx;

    [SerializeField] private float maxVelocity = 50f;

    private VelocityEstimator _velocityEstimator;
    public GameObject tooFastText;

    [Header("Constraint Properties")]
    [SerializeField] private bool showGizmos = false;
    [SerializeField] private Transform book;
    public float pageLength = 10.0f;
    
    public BoxCollider pageColliderLeft;
    public BoxCollider pageColliderRight;
    
    // Object constrain properties
    private Transform unconstrainedPoint;
    
    private Vector3 constrainedPoint;
    
    private BoxCollider pageCollider;

    // Corners
    Bounds leftPageLocalBounds;
    Bounds rightPageLocalBounds;
    Vector3 bookCenterTop, bookCenterBottom;
    Vector3 leftPageEdgeTop, leftPageEdgeBottom;
    Vector3 rightPageEdgeTop, rightPageEdgeBottom;
    private Vector3 pageBoundaryUp;

    // Radius vector
    Vector3 centerPoint = Vector3.zero, edgePoint = Vector3.zero;

    // Dot product of radiusPoint and pageBoundaryUp
    private float ppDot = 0;
    public Vector3 bookUpShift = Vector3.zero;

    private Rigidbody _rb;
    private ObiParticleAttachment _particleAttachment;

    private Vector3 handPos;

    private string handDebugLine = "";
    private bool attachmentInProgress;
    
    //page complaint delay
    private bool isDelaying = false;

    private void Awake()
    {
        actor = page.GetComponent<ObiActor>();
        _particleAttachment = page.GetComponent<ObiParticleAttachment>();
        
        startTime = Time.time;

        foreach (var @group in actor.blueprint.groups.Where(@group => @group.name == pageEdgeParticleGroupName))
        {
            _pageEdgeParticleGroup = @group;
        }
        
        foreach (var @group in actor.blueprint.groups.Where(@group => @group.name == grabbedParticleGroupName))
        {
            _grabbedParticleGroup = @group;
        }
        
        _grabbedParticleIdx = _pageEdgeParticleGroup.particleIndices[0];
        
        _velocityEstimator = GetComponent<VelocityEstimator>();

        _rb = GetComponent<Rigidbody>();
        
        // page constraint variables
        pageCollider = pageColliderRight;
    }

    private void Update()
    {
        if (!unconstrainedPoint)
        {
            return;
        }
        handPos = unconstrainedPoint.position;
        var grabbedParticlePos = actor.GetParticlePosition(_grabbedParticleIdx);
        
        if (!isAttached || attachmentInProgress)
        {
            _rb.MovePosition(grabbedParticlePos);
            return;
        }
        
        //if page complaint delay is done, check the velocity
        if (isDelaying == false)
        {
            CheckVelocity();
        }
        
        // constrain the point within defined region
        DoConstraint();
        
        // match the position of this object to the constrained point
        // make object follow the interactor [e.g., the hand which is "holding" this object]
        float fracComplete = (Time.time - startTime) / followSpeed;
        _rb.MovePosition(Vector3.Slerp(transform.position, constrainedPoint, fracComplete));
        
        // DRAW GIZMOS -----------------------------------------------
        if (!showGizmos || !pageCollider) return;
        Draw.ingame.Line(bookCenterTop, bookCenterBottom, Color.red);
        
        Draw.ingame.Line(rightPageEdgeTop, rightPageEdgeBottom, Color.blue);
        Draw.ingame.Line(bookCenterTop, rightPageEdgeTop, Color.yellow);
        Draw.ingame.Line(bookCenterBottom, rightPageEdgeBottom, Color.yellow);
        
        Draw.ingame.Line(leftPageEdgeTop, leftPageEdgeBottom, Color.blue);
        Draw.ingame.Line(bookCenterTop, leftPageEdgeTop, Color.yellow);
        Draw.ingame.Line(bookCenterBottom, leftPageEdgeBottom, Color.yellow);
        
        
        // draw the center point
        Draw.ingame.WireSphere(centerPoint, 0.25f, Color.blue);
        Draw.ingame.WireSphere(edgePoint, 0.25f, Color.white);
        
        // // Draw constrained point
        Draw.ingame.WireSphere(constrainedPoint, 0.2f, Color.green);
        Draw.ingame.WireSphere(transform.position, 0.4f, Color.magenta);
        
        // Draw label showing ppDot value
        Draw.ingame.Label2D(interactorObject.transform.position, handDebugLine, 14F);
        
        // Draw grappedParticle sphere
        Draw.ingame.WireSphere(grabbedParticlePos, 0.05f, Color.red);
    }

    private void CheckVelocity()
    {
        var curVel = _velocityEstimator.GetVelocityEstimate().sqrMagnitude;
        if (curVel < maxVelocity) return;
        
        Debug.Log("Too fast! Current velocity: " + curVel);

        if (tooFastText.activeSelf) return;
        tooFastText.SetActive(true);
        StartCoroutine(HideTooFastText());
    }

    private IEnumerator HideTooFastText()
    {
        yield return new WaitForSeconds(5f);
        tooFastText.SetActive(false);
    }

    public void Attach(BaseInteractor interactor)
    {
        handPos = interactor.GetGameObject().transform.position;
        attachmentInProgress = true;
        
        //call delaying coroutine here
        StartCoroutine("StartGraspDelay");
        
        if (!isAttached)
        {
            var minDistance = float.MaxValue;
            var pIdxOfMin = -1;
            var pPosOfMin = Vector3.zero;
            foreach (var pIdx in _pageEdgeParticleGroup.particleIndices)
            {
                var particlePos = actor.GetParticlePosition(pIdx);
                var distance = Vector3.Distance(particlePos, handPos);
                if (distance >= minDistance) continue;
                
                pPosOfMin = particlePos;
                pIdxOfMin = pIdx;
                minDistance = distance;
            }
            
            centerPoint = BasicHelpers.NearestPointOnFiniteLine(bookCenterTop, bookCenterBottom,
                pPosOfMin);
            edgePoint = BasicHelpers.NearestPointOnFiniteLine(rightPageEdgeTop, rightPageEdgeBottom,
                pPosOfMin);

            if (pIdxOfMin != _grabbedParticleIdx)
            {
                // set obi particle's target transform to null, disconnecting its previous particle
                _particleAttachment.target = null;
                
                // modify list of particleGroup to include new particle we want to grab
                _particleAttachment.particleGroup.particleIndices[0] = pIdxOfMin;
                
                // teleport this grabber object to particle position
                transform.position = pPosOfMin;
                
                _grabbedParticleIdx = pIdxOfMin;
                
                // force rebind of particle attachment component
                _particleAttachment.target = transform;
            }
        }

        isAttached = true;
        interactorObject = interactor.GetGameObject();
        _particleAttachment.enabled = true;

        unconstrainedPoint = interactor.GetGameObject().transform;
        
        attachmentInProgress = false;
    }
    
    IEnumerator StartGraspDelay()
    {
        isDelaying = true;
        yield return new WaitForSeconds(1f);
        isDelaying = false;
    }
    
    public void UnAttach()
    {
        isAttached = false;
        interactorObject = null;
        _particleAttachment.enabled = false;

        unconstrainedPoint = null;
    }


    #region Constraint Functions
    private void DoConstraint()
    {
        if (!unconstrainedPoint) return;
        UpdateCorners();
        UpdateConstrainedPoint();
        CheckBoundaryTransition();
    }
    
    private void UpdateCorners()
    {
        if (!pageColliderLeft || !pageColliderRight) return;
        
        // cache for performance
        var pageColTransformL = pageColliderLeft.transform;
        var pageColTransformR = pageColliderRight.transform;
        
        // # [BEGIN] Handle Right Collider
        rightPageLocalBounds.center = pageColliderRight.center;
        rightPageLocalBounds.size = pageColliderRight.size;

        bookCenterBottom = pageColTransformR.TransformPoint(rightPageLocalBounds.min.x, rightPageLocalBounds.min.y, rightPageLocalBounds.min.z);
        bookCenterTop = pageColTransformR.TransformPoint(rightPageLocalBounds.min.x, rightPageLocalBounds.min.y, rightPageLocalBounds.max.z);
        
        rightPageEdgeTop = pageColTransformR.TransformPoint(rightPageLocalBounds.max.x, rightPageLocalBounds.min.y, rightPageLocalBounds.max.z);
        rightPageEdgeBottom = pageColTransformR.TransformPoint(rightPageLocalBounds.max.x, rightPageLocalBounds.min.y, rightPageLocalBounds.min.z);
        // # [END] Handle Right Collider
        
        // # [BEGIN] Handle Left Collider
        leftPageLocalBounds.center = pageColliderLeft.center;
        leftPageLocalBounds.size = pageColliderLeft.size;

        leftPageEdgeTop = pageColTransformL.TransformPoint(leftPageLocalBounds.min.x, leftPageLocalBounds.min.y, leftPageLocalBounds.max.z);
        leftPageEdgeBottom = pageColTransformL.TransformPoint(leftPageLocalBounds.min.x, leftPageLocalBounds.min.y, leftPageLocalBounds.min.z);
        // # [END] Handle Left Collider

        if (!showGizmos) return;
        Draw.ingame.WireBox(pageColTransformL.position, pageColTransformL.rotation, pageColliderLeft.size);
        Draw.ingame.WireBox(pageColTransformR.position, pageColTransformR.rotation, pageColliderRight.size);
    }

    private void UpdateConstrainedPoint()
    {
        var handPosRelativeToPage = pageCollider.transform.InverseTransformPoint(handPos);
        
        if (pageCollider.bounds.Contains(handPos))
        {
            handDebugLine = "Interactor inside collider, PageCollider: " + (pageCollider == pageColliderLeft ? "Left" : "Right");
            
            var freePoint = centerPoint + (handPos - centerPoint);
            var radiusPoint = centerPoint + ((handPos - centerPoint).normalized * pageLength);

            constrainedPoint = freePoint.magnitude <= radiusPoint.magnitude ? freePoint : radiusPoint;
            
            if(showGizmos)
                Draw.ingame.Line(handPos, centerPoint, Color.cyan);
        }
        else
        {
            handDebugLine = "Interactor outside collider, PageCollider: " + (pageCollider == pageColliderLeft ? "Left" : "Right");
            
            var colliderPoint = pageCollider.ClosestPoint(handPos);
            var radiusPoint = centerPoint + ((colliderPoint - centerPoint).normalized * pageLength);

            if (radiusPoint.magnitude < colliderPoint.magnitude && pageCollider.bounds.Contains(radiusPoint))
                constrainedPoint = radiusPoint;
            else
                constrainedPoint = colliderPoint;

            if (showGizmos)
            {
                Draw.ingame.Arrow(handPos, colliderPoint, Color.green);
                Draw.ingame.Arrow(colliderPoint, centerPoint, Color.red);
            }
        }
        
        if(showGizmos)
            Draw.ingame.Arrow(centerPoint, constrainedPoint, Color.blue);
    }

    private void CheckBoundaryTransition()
    {
        // cache for performance
        var pageColTransform = pageCollider.transform;
        
        // set the reference frame for the book side by extracting its normal vector
        pageBoundaryUp = book.up;
        
        var planeNormal = pageColTransform.forward;
        
        var bookUpProjected = Vector3.ProjectOnPlane(pageBoundaryUp + bookUpShift, planeNormal);
        var constrainedProjected = Vector3.ProjectOnPlane(constrainedPoint, planeNormal);
        
        ppDot = Vector3.SignedAngle(bookUpProjected.normalized, constrainedProjected.normalized, planeNormal);

        pageCollider = ppDot >= 0 ? pageColliderLeft : pageColliderRight;
    }
    #endregion
}
