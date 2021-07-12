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


    // Object constrain properties
    private Transform unconstrainedPoint;
    public BoxCollider pageCollider;

    [Header("Output")]
    public Vector3 constrainedPoint;

    // Corners
    Bounds localBounds;
    Vector3 bookCenterTop, bookCenterBottom;
    Vector3 pageEdgeTop, pageEdgeBottom;
    private Vector3 pageBoundaryUp;

    // Radius vector
    Vector3 centerPoint, edgePoint;
    float radius;
    
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
            
        // Draw page
        Draw.ingame.Line(bookCenterTop, bookCenterBottom, Color.red);
        Draw.ingame.Line(pageEdgeTop, pageEdgeBottom, Color.blue);
        Draw.ingame.Line(bookCenterTop, pageEdgeTop, Color.yellow);
        Draw.ingame.Line(bookCenterBottom, pageEdgeBottom, Color.yellow);
        
        // draw the center point
        Draw.ingame.WireSphere(centerPoint, 0.25f, Color.blue);
        Draw.ingame.WireSphere(edgePoint, 0.25f, Color.white);
        
        // Draw constrained point
        Draw.ingame.WireSphere(constrainedPoint, 0.2f, Color.green);
        
        Draw.ingame.Label2D(interactorObject.transform.position, ppDot.ToString(), 14F);
    }

    private void CheckVelocity()
    {
        float curVel = _velocityEstimator.GetVelocityEstimate().sqrMagnitude;
        if ( curVel >= maxVelocity )
        {
            Debug.Log("Too fast! Current velocity: " + curVel);

            if (!tooFastText.activeSelf)
            {
                tooFastText.SetActive(true);
                StartCoroutine(HideTooFastText());
            }
        }
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
    }
    
    public void UnAttach()
    {
        isAttached = false;
        interactorObject = null;
        _particleAttachment.enabled = false;

        unconstrainedPoint = null;
    }


    #region Constraint Functions
    private void CalculatePagePosWeight()
    {
        throw new NotImplementedException();
    }
    
    private void DoConstraint()
    {
        if (!unconstrainedPoint) return;
        UpdateCorners();
        centerPoint = BasicHelpers.NearestPointOnFiniteLine(bookCenterTop, bookCenterBottom, unconstrainedPoint.position);
        edgePoint = BasicHelpers.NearestPointOnFiniteLine(pageEdgeTop, pageEdgeBottom, unconstrainedPoint.position);
        radius = Vector3.Distance(centerPoint, edgePoint);
        UpdateConstrainedPoint();
    }
    
    private void UpdateCorners()
    {
        if (!pageCollider) return;

        localBounds = pageCollider.bounds;
        localBounds.center = pageCollider.center;
        localBounds.size = pageCollider.size;

        // cache for performance
        var pageColTransform = pageCollider.transform;

        bookCenterBottom = pageColTransform.TransformPoint(localBounds.min.x, localBounds.min.y, localBounds.min.z);
        bookCenterTop = pageColTransform.TransformPoint(localBounds.min.x, localBounds.min.y, localBounds.max.z);
        
        pageEdgeTop = pageColTransform.TransformPoint(localBounds.max.x, localBounds.min.y, localBounds.max.z);
        pageEdgeBottom = pageColTransform.TransformPoint(localBounds.max.x, localBounds.min.y, localBounds.min.z);
        
        Draw.ingame.WireBox(pageColTransform.position, pageColTransform.rotation, pageCollider.size);

        pageBoundaryUp = pageCollider.transform.up;
    }

    private void UpdateConstrainedPoint()
    {
        if (pageCollider.bounds.Contains(unconstrainedPoint.position))
        {
            var freePoint = centerPoint + (unconstrainedPoint.position - centerPoint);
            var radiusPoint = centerPoint + ((unconstrainedPoint.position - centerPoint).normalized * radius);

            constrainedPoint = freePoint.magnitude <= radiusPoint.magnitude ? freePoint : radiusPoint;
            
            Debug.DrawLine(unconstrainedPoint.position, centerPoint, Color.cyan);
        }
        else
        {
            var handPos = unconstrainedPoint.position;
            var colliderPoint = Vector3.zero;
            
            // # cast a ray from center point to unconstrained point (location of hand)
            var colLeftCenter = ((bookCenterBottom + bookCenterTop) / 2) + new Vector3(0.5f,0.5f,0);
            Draw.ingame.WireSphere(colLeftCenter, 0.2f, Color.red);
            
            colliderPoint = pageCollider.ClosestPoint(handPos);

            var radiusPoint = centerPoint + ((colliderPoint - centerPoint).normalized * radius);
            constrainedPoint = colliderPoint.magnitude <= radiusPoint.magnitude ? colliderPoint : radiusPoint;

            Draw.ingame.Arrow(handPos, colliderPoint, Color.green);
            Draw.ingame.Arrow(colliderPoint, centerPoint, Color.red);
        }

        Draw.ingame.Arrow(centerPoint, constrainedPoint, Color.blue);
        
        var planeNormal = pageCollider.transform.forward;
        
        var bookUpProjected = Vector3.ProjectOnPlane(pageBoundaryUp + bookUpShift, planeNormal);
        var constrainedProjected = Vector3.ProjectOnPlane(constrainedPoint, planeNormal);
        
        ppDot = Vector3.Dot(bookUpProjected.normalized, constrainedProjected.normalized);
    }
    #endregion
}
