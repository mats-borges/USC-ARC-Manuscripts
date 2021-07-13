using System;
using System.Collections;
using System.Threading.Tasks;
using Drawing;
using HandPhysicsToolkit.Helpers;
using Obi;
using UnityEngine;

public class GraspingPoint : MonoBehaviour
{
    public float speed = 2f;
    private float startTime;
    private bool isAttached = false;
    private GameObject interactorObject;
    [SerializeField] private GameObject page;
    private Vector3 targetParticlePos;
    private ObiActor actor;
    private ObiParticleGroup particleGroup;
    [SerializeField] private int particleGroupIndex;
    private int particleIdx;

    [SerializeField] private float maxVelocity = 50f;

    private VelocityEstimator _velocityEstimator;
    public GameObject tooFastText;
    
    public BoxCollider pageColliderLeft;
    public BoxCollider pageColliderRight;
    
    // Object constrain properties
    private Transform unconstrainedPoint;

    [Header("Output")]
    public Vector3 constrainedPoint;
    
    private BoxCollider pageCollider;

    [SerializeField] private Transform book;
    
    // Corners
    Bounds leftPageLocalBounds;
    Bounds rightPageLocalBounds;
    Vector3 bookCenterTop, bookCenterBottom;
    Vector3 leftPageEdgeTop, leftPageEdgeBottom;
    Vector3 rightPageEdgeTop, rightPageEdgeBottom;
    private Vector3 pageBoundaryUp;

    // Radius vector
    Vector3 centerPoint = Vector3.zero, edgePoint = Vector3.zero;
    public float radius = 10.0f;
    
    // Dot product of radiusPoint and pageBoundaryUp
    private float ppDot = 0;
    public Vector3 bookUpShift = Vector3.zero;

    private Rigidbody _rb;
    private ObiParticleAttachment _particleAttachment;

    private void Awake()
    {
        actor = page.GetComponent<ObiActor>();
        _particleAttachment = page.GetComponent<ObiParticleAttachment>();
        
        startTime = Time.time;
        particleGroup = actor.blueprint.groups[particleGroupIndex];
        particleIdx = particleGroup.particleIndices[0];
        
        _velocityEstimator = GetComponent<VelocityEstimator>();

        _rb = GetComponent<Rigidbody>();
        
        // page constraint variables
        pageCollider = pageColliderRight;
    }

    private void Update()
    {
        if(_particleAttachment.enabled == false)
            _rb.MovePosition(actor.GetParticlePosition(particleIdx));
        
        if (!isAttached) return;
        
        // make object follow the interactor [e.g., the hand which is "holding" this object]
        float fracComplete = (Time.time - startTime) / speed;
        _rb.MovePosition(Vector3.Slerp(transform.position, interactorObject.transform.position, fracComplete));
        
        CheckVelocity();
        
        // constrain the point within defined region
        DoConstraint();
        
        // match the position of this object to the constrained point
        _rb.MovePosition(constrainedPoint);
        
        // DRAW GIZMOS -----------------------------------------------
        if (!pageCollider) return;
        
        // Draw page colliders
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
        
        // Draw constrained point
        Draw.ingame.WireSphere(constrainedPoint, 0.2f, Color.green);
        
        Draw.ingame.Label2D(interactorObject.transform.position, ppDot.ToString(), 14F);
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

    public void Attach(Interactor interactor)
    {
        isAttached = true;
        interactorObject = interactor.gameObject;
        _particleAttachment.enabled = true;

        unconstrainedPoint = interactor.transform;
        
        centerPoint = BasicHelpers.NearestPointOnFiniteLine(bookCenterTop, bookCenterBottom, actor.GetParticlePosition(particleIdx));
        edgePoint = BasicHelpers.NearestPointOnFiniteLine(rightPageEdgeTop, rightPageEdgeBottom, actor.GetParticlePosition(particleIdx));
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
        rightPageLocalBounds = pageColliderRight.bounds;
        rightPageLocalBounds.center = pageColliderRight.center;
        rightPageLocalBounds.size = pageColliderRight.size;

        bookCenterBottom = pageColTransformR.TransformPoint(rightPageLocalBounds.min.x, rightPageLocalBounds.min.y, rightPageLocalBounds.min.z);
        bookCenterTop = pageColTransformR.TransformPoint(rightPageLocalBounds.min.x, rightPageLocalBounds.min.y, rightPageLocalBounds.max.z);
        
        rightPageEdgeTop = pageColTransformR.TransformPoint(rightPageLocalBounds.max.x, rightPageLocalBounds.min.y, rightPageLocalBounds.max.z);
        rightPageEdgeBottom = pageColTransformR.TransformPoint(rightPageLocalBounds.max.x, rightPageLocalBounds.min.y, rightPageLocalBounds.min.z);
        // # [END] Handle Right Collider
        
        // # [BEGIN] Handle Left Collider
        leftPageLocalBounds = pageColliderLeft.bounds;
        leftPageLocalBounds.center = pageColliderLeft.center;
        leftPageLocalBounds.size = pageColliderLeft.size;

        leftPageEdgeTop = pageColTransformL.TransformPoint(leftPageLocalBounds.min.x, leftPageLocalBounds.min.y, leftPageLocalBounds.max.z);
        leftPageEdgeBottom = pageColTransformL.TransformPoint(leftPageLocalBounds.min.x, leftPageLocalBounds.min.y, leftPageLocalBounds.min.z);
        // # [END] Handle Left Collider
        
        Draw.ingame.WireBox(pageColTransformL.position, pageColTransformL.rotation, pageColliderLeft.size);
        Draw.ingame.WireBox(pageColTransformR.position, pageColTransformR.rotation, pageColliderRight.size);
    }

    private void UpdateConstrainedPoint()
    {
        var handPos = unconstrainedPoint.position;
        
        if (pageCollider.bounds.Contains(unconstrainedPoint.position))
        {
            var freePoint = centerPoint + (handPos - centerPoint);
            var radiusPoint = centerPoint + ((handPos - centerPoint).normalized * radius);

            constrainedPoint = freePoint.magnitude <= radiusPoint.magnitude ? freePoint : radiusPoint;
            
            Draw.ingame.Line(handPos, centerPoint, Color.cyan);
        }
        else
        {
            var colliderPoint = pageCollider.ClosestPoint(handPos);

            var radiusPoint = centerPoint + ((colliderPoint - centerPoint).normalized * radius);
            constrainedPoint = colliderPoint.magnitude <= radiusPoint.magnitude ? colliderPoint : radiusPoint;

            Draw.ingame.Arrow(handPos, colliderPoint, Color.green);
            Draw.ingame.Arrow(colliderPoint, centerPoint, Color.red);
        }

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
