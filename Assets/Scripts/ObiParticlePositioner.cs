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
    [SerializeField] private Material smudgedMat1;
    [SerializeField] private Material smudgedMat2;
    [SerializeField] private Material smudgedMat3;

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
        string name = page.GetComponent<MeshRenderer>().material.name;
        if ((name == triggerMat.name + " (Instance)" || name == smudgedMat1.name + " (Instance)" || name == smudgedMat2.name + " (Instance)"|| name == smudgedMat3.name + " (Instance)") && transform.position.x > xBoundary)
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


