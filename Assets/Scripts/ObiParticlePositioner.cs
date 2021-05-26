using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
using UnityFx.Outline;

[RequireComponent(typeof(ObiActor))]
public class ObiParticlePositioner : MonoBehaviour
{
    [SerializeField] private GameObject page;
    [SerializeField] private GameObject line;
    private ObiParticleGroup particleGroup;
    private ObiActor actor;
    [SerializeField] Material triggerMat;
    [SerializeField] float xBoundary;
    [SerializeField] private int particleGroupIndex;
    [SerializeField] private GameObject highlightText;

    private int particleIdx;
    // Start is called before the first frame update
    void Awake()
    {
        actor = page.GetComponent<ObiActor>();
        particleGroup = actor.blueprint.groups[particleGroupIndex];
        particleIdx = particleGroup.particleIndices[0];
    }

    // Update is called once per frame
    void Update()
    {
        transform.SetPositionAndRotation(actor.GetParticlePosition(particleIdx), Quaternion.identity);
        if ((page.GetComponent<MeshRenderer>().material.name == "8 (Instance)"|| page.GetComponent<MeshRenderer>().material.name == "8_revised.png for smudging 5-05 (Instance)") && transform.position.x > xBoundary)
        {
            gameObject.GetComponent<OutlineBehaviour>().enabled = true;
            line.SetActive(true);
            highlightText.SetActive(true);
        }
        else
        {
            gameObject.GetComponent<OutlineBehaviour>().enabled = false;
            line.SetActive(false);
            highlightText.SetActive(false);
        }
        
        
    }
}


