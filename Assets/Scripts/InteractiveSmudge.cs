using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
using UnityFx.Outline;

[RequireComponent(typeof(ObiActor))]
public class InteractiveSmudge : MonoBehaviour
{
    [SerializeField] private GameObject page;
    private ObiParticleGroup particleGroup;
    private ObiActor actor;
    [SerializeField] Material triggerMat;
    [SerializeField] private Material smudgedMat1;
    [SerializeField] private Material smudgedMat2;
    [SerializeField] float xBoundary;
    [SerializeField] private int particleGroupIndex;
    private int pageStage;
    private int particleIdx;

    // Start is called before the first frame update
    void Awake()
    {
        pageStage = 0;
        actor = page.GetComponent<ObiActor>();
        particleGroup = actor.blueprint.groups[particleGroupIndex];
        particleIdx = particleGroup.particleIndices[0];
    }

    // Update is called once per frame
    void Update()
    {
        transform.SetPositionAndRotation(actor.GetParticlePosition(particleIdx), Quaternion.identity);
        if ((page.GetComponent<MeshRenderer>().material.name == "8 (Instance)" || page.GetComponent<MeshRenderer>().material.name == "8_revised.png for smudging 5-05 (Instance)") && transform.position.x > xBoundary)
        {
            gameObject.GetComponent<OutlineBehaviour>().enabled = true;
        }
        else
        {
            gameObject.GetComponent<OutlineBehaviour>().enabled = false;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            switch (pageStage)
            {
                case 0 : 
                    page.GetComponent<MeshRenderer>().material = smudgedMat1;
                    pageStage++;
                    break;
                case 1:
                    page.GetComponent<MeshRenderer>().material = smudgedMat2;
                    pageStage++;
                    break;
                case 2:
                    page.GetComponent<MeshRenderer>().material = triggerMat;
                    pageStage = 0;
                    break;
            }
        }
    }

    public void AdvanceSmudgeMaterial()
    {
        switch (pageStage)
        {
            case 0 : 
                page.GetComponent<MeshRenderer>().material = smudgedMat1;
                pageStage++;
                break;
            case 1:
                page.GetComponent<MeshRenderer>().material = smudgedMat2;
                pageStage++;
                break;
            case 2:
                page.GetComponent<MeshRenderer>().material = triggerMat;
                pageStage = 0;
                break;
        }
    }
}
