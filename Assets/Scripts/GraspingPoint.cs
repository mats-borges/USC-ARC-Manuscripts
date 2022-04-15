using System;
using System.Collections;
using System.Globalization;
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

    public MeshCollider bookBoundaryCollider;
    
    // Object constrain properties
    private Transform unconstrainedPoint;
    
    private Vector3 constrainedPoint;
    
    private BoxCollider pageCollider;

    [SerializeField] private Transform SpineTop;
    [SerializeField] private Transform SpineBottom;
    
    [SerializeField] private Transform LeftPageEdgeTop;
    [SerializeField] private Transform LeftPageEdgeBottom;
    
    [SerializeField] private Transform RightPageEdgeTop;
    [SerializeField] private Transform RightPageEdgeBottom;

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
    [SerializeField] private ObiParticleAttachment _particleAttachment;

    private Vector3 handPos;

    private string handDebugLine = "";
    private bool attachmentInProgress;
    
    //page complaint delay
    private bool isDelaying = false;

    private bool isInsidePageColliders = false;

    private void Awake()
    {
        actor = page.GetComponent<ObiActor>();

        startTime = Time.fixedTime;

        foreach (var @group in actor.blueprint.groups.Where(@group => @group.name == pageEdgeParticleGroupName))
        {
            _pageEdgeParticleGroup = @group;
        }
        
        foreach (var @group in actor.blueprint.groups.Where(@group => @group.name == grabbedParticleGroupName))
        {
            _grabbedParticleGroup = @group;
        }
        
        // var particleAttachments = page.GetComponents<ObiParticleAttachment>();
        // foreach (var component in particleAttachments)
        // {
        //     if (component.particleGroup.GetInstanceID() == _grabbedParticleGroup.GetInstanceID())
        //     {
        //         _particleAttachment = component;
        //     }
        // }
        
        _grabbedParticleIdx = _pageEdgeParticleGroup.particleIndices[0];
        
        _velocityEstimator = GetComponent<VelocityEstimator>();

        _rb = GetComponent<Rigidbody>();
        
        // page constraint variables
        pageCollider = pageColliderRight;
    }

    private void FixedUpdate()
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

        isInsidePageColliders = IsPositionWithinBookBoundary(gameObject.transform.position, transform.localScale/2);
        
        // constrain the point within defined region
        DoConstraint();
        
        // match the position of this object to the constrained point
        // make object follow the interactor [e.g., the hand which is "holding" this object]
        float fracComplete = (Time.fixedTime - startTime) / followSpeed;
        _rb.MovePosition(Vector3.Slerp(transform.position, constrainedPoint, fracComplete));

        // var forceVector = constrainedPoint - transform.position;
        // var forceDirection = forceVector.normalized;
        // var forcePower = followSpeed * forceVector.magnitude;
        // _rb.AddRelativeForce(forceDirection * forcePower);
        
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

    private bool IsPositionWithinBookBoundary(Vector3 position, Vector3 boxCastSize)
    {
        // Use the OverlapBox to detect if there are any other colliders within this box area.
        // Use the GameObject's centre, half the size (as a radius) and rotation. This creates an invisible box around your GameObject.
        var hitColliders = Physics.OverlapBox(position, boxCastSize, Quaternion.identity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Collide);
        
        // Return true when there is a PageBoundary collider coming into contact with the box
        return hitColliders.Any(t => t.gameObject.name.Contains(bookBoundaryCollider.name));
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
    
    //bugnote : when the user grabs the page with pageMagic from the same particle twice, the grabber teleports far away
    public void Attach(BaseInteractor interactor)
    {
        Debug.Log("attach called at " + Time.time + ", frameDelta: " + Time.deltaTime);
        if (isAttached) return;
            
        handPos = interactor.GetGameObject().transform.position;
        attachmentInProgress = true;
        
        //call delaying coroutine here
        StartCoroutine(StartGraspDelay());
        
        _grabbedParticleGroup.particleIndices.Clear();
        
        var minDistance = float.MaxValue;
        var maxDistance = 2.5f;
        var pIdxOfMin = -1;
        var pPosOfMin = Vector3.zero;
        foreach (var pIdx in _pageEdgeParticleGroup.particleIndices)
        {
            var particlePos = actor.GetParticlePosition(pIdx);
            var distance = Vector3.Distance(particlePos, handPos);
            if (distance >= maxDistance) continue;
            
            pPosOfMin = particlePos;
            pIdxOfMin = pIdx;
            minDistance = distance;
            
            // modify list of particleGroup to include new particle we want to grab
            _grabbedParticleGroup.particleIndices.Add(pIdx);
            
            Debug.Log("Added grabbed particle");
        }
        
        centerPoint = BasicHelpers.NearestPointOnFiniteLine(bookCenterTop, bookCenterBottom,
            pPosOfMin);
        edgePoint = BasicHelpers.NearestPointOnFiniteLine(rightPageEdgeTop, rightPageEdgeBottom,
            pPosOfMin);

        
        // set obi particle's target transform to null, disconnecting its previous particle
        _particleAttachment.target = null;

        // teleport this grabber object to particle position
        transform.position = pPosOfMin;
        
        _grabbedParticleIdx = pIdxOfMin;
        
        // force rebind of particle attachment component
        _particleAttachment.target = transform;

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
        // # [BEGIN] Handle Right Collider

        bookCenterTop = SpineTop.position;
        bookCenterBottom = SpineBottom.position;

        rightPageEdgeTop = RightPageEdgeTop.position;
        rightPageEdgeBottom = RightPageEdgeBottom.position;
        // # [END] Handle Right Collider
        
        // # [BEGIN] Handle Left Collider

        leftPageEdgeTop = LeftPageEdgeTop.position;
        leftPageEdgeBottom = LeftPageEdgeBottom.position;
        // # [END] Handle Left Collider

        if (!showGizmos) return;
        using (Draw.InLocalSpace(bookBoundaryCollider.transform)) {
            // Draw a box at (0,0,0) relative to the current object
            // This means it will show up at the object's position
            Draw.ingame.WireMesh(bookBoundaryCollider.sharedMesh, Color.white);
        }
    }

    private void UpdateConstrainedPoint()
    {
        var handPosRelativeToPage = pageCollider.transform.InverseTransformPoint(handPos);
        
        if (isInsidePageColliders)
        {
            handDebugLine = "Interactor inside page boundary";
            
            var freePoint = centerPoint + (handPos - centerPoint);
            var radiusPoint = centerPoint + ((handPos - centerPoint).normalized * pageLength);

            constrainedPoint = freePoint.magnitude <= radiusPoint.magnitude ? freePoint : radiusPoint;
            
            if(showGizmos)
                Draw.ingame.Line(handPos, centerPoint, Color.cyan);
        }
        else
        {
            handDebugLine = "Interactor outside page boundary";
            
            var colliderPoint = pageCollider.ClosestPoint(handPos);
            var radiusPoint = centerPoint + ((colliderPoint - centerPoint).normalized * pageLength);

            if (radiusPoint.magnitude < colliderPoint.magnitude &&
                IsPositionWithinBookBoundary(radiusPoint, new Vector3(0.001f, 0.001f, 0.001f)))
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
        
        // pageBoundaryUp should represent the upward-facing normal of the open-book
        // ... essentially provides a dividing line (vertically) between the two pages of the book
        // (bookUpShift is a property that allows us to fine-tune this if necessary)
        var bookUpProjected = Vector3.ProjectOnPlane(pageBoundaryUp + bookUpShift, planeNormal);
        var constrainedProjected = Vector3.ProjectOnPlane(constrainedPoint, planeNormal);

        ppDot = Vector3.SignedAngle(bookUpProjected.normalized, constrainedProjected.normalized, planeNormal);

        pageCollider = ppDot >= 0 ? pageColliderLeft : pageColliderRight;
        
        if (showGizmos)
        {
            var up = transform.up;
            var startPos = book.transform.position + up * 5;
            Draw.ingame.Arrow(startPos, startPos + bookUpProjected.normalized * 20, Color.gray);
            Draw.ingame.Arrow(startPos, startPos + constrainedProjected.normalized * 20, new Color(0.25f, 0.3f, 1f));
            Draw.ingame.Label2D(startPos + up, ppDot.ToString(CultureInfo.InvariantCulture));
        }
    }
    #endregion
}
