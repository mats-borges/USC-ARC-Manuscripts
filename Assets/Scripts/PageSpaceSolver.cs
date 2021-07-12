using System;
using System.Collections;
using System.Threading.Tasks;
using Drawing;
using HandPhysicsToolkit.Helpers;
using Obi;
using UnityEngine;

public class PageSpaceSolver : MonoBehaviour
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

    // Radius vector
    Vector3 centerPoint, edgePoint;
    float radius;

    private void Start()
    {
        actor = page.GetComponent<ObiActor>();
        startTime = Time.time;
        particleGroup = actor.blueprint.groups[particleGroupIndex];
        particleIdx = particleGroup.particleIndices[0];
        
        _velocityEstimator = GetComponent<VelocityEstimator>();
    }

    private void Update()
    {
        if (isAttached)
        {
            float fracComplete = (Time.time - startTime) / speed;
            
            // make object follow the interactor [e.g., the hand which is "holding" this object]
            transform.position = Vector3.Slerp(transform.position, interactorObject.transform.position, fracComplete);

            CheckVelocity();
            
            // constrain the point within defined region
            DoConstraint();
            
            // match the position of this object to the constrained point
            if(constrainedPoint != Vector3.positiveInfinity)
                this.transform.position = constrainedPoint;
            
            
            // DRAW GIZMOS -----------------------------------------------
            if (!pageCollider) return;
            
            // Draw page
            using (Draw.WithColor(Color.yellow))
            {
                Draw.ingame.Line(bookCenterTop, bookCenterBottom);
                Draw.ingame.Line(pageEdgeTop, pageEdgeBottom);
                Draw.ingame.Line(bookCenterTop, pageEdgeTop);
                Draw.ingame.Line(bookCenterBottom, pageEdgeBottom);
            }
        

            
            Draw.ingame.WireSphere(centerPoint, radius, Color.blue);

            // Draw constrained point
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = constrainedPoint;
            go.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        
            using (Draw.InLocalSpace(go.transform)) {
                Draw.SolidMesh(go.GetComponent<MeshFilter>().sharedMesh, Color.green);
            }

        }
        else
        {
            transform.SetPositionAndRotation(actor.GetParticlePosition(particleIdx), Quaternion.identity);
        }
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
        page.GetComponent<ObiParticleAttachment>().enabled = true;

        unconstrainedPoint = interactor.transform;
    }
    
    public void UnAttach()
    {
        isAttached = false;
        interactorObject = null;
        page.GetComponent<ObiParticleAttachment>().enabled = false;

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
        
        localBounds.center = pageCollider.center;
        localBounds.size = pageCollider.size;

        bookCenterBottom = pageCollider.transform.TransformPoint(localBounds.min.x, localBounds.min.y, localBounds.min.z);
        bookCenterTop = pageCollider.transform.TransformPoint(localBounds.min.x, localBounds.min.y, localBounds.max.z);
        pageEdgeTop = pageCollider.transform.TransformPoint(localBounds.max.x, localBounds.min.y, localBounds.max.z);
        pageEdgeBottom = pageCollider.transform.TransformPoint(localBounds.max.x, localBounds.min.y, localBounds.min.z);
    }

    private void UpdateConstrainedPoint()
    {
        if (pageCollider.bounds.Contains(unconstrainedPoint.position))
        {
            constrainedPoint = centerPoint + ((unconstrainedPoint.position - centerPoint).normalized * radius);
            Debug.DrawLine(unconstrainedPoint.position, centerPoint, Color.cyan);
            // constrainedPoint = Vector3.positiveInfinity;
        }
        else
        {
            Vector3 closestPoint = pageCollider.ClosestPoint(unconstrainedPoint.position);
            constrainedPoint = centerPoint + ((closestPoint - centerPoint).normalized * radius);
            
            Draw.ingame.Arrow(unconstrainedPoint.position, closestPoint, Color.red);
            Draw.ingame.Arrow(closestPoint, centerPoint, Color.red);
        }

        Draw.ingame.Arrow(centerPoint, constrainedPoint, Color.blue);
    }
    #endregion
}
