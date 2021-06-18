using System.Collections;
using System.Threading.Tasks;
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
            transform.position = Vector3.Slerp(transform.position, interactorObject.transform.position, fracComplete);
            
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
        else
        {
            transform.SetPositionAndRotation(actor.GetParticlePosition(particleIdx), Quaternion.identity);
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
    }
    
    public void UnAttach()
    {
        isAttached = false;
        interactorObject = null;
        page.GetComponent<ObiParticleAttachment>().enabled = false;
    }
}
