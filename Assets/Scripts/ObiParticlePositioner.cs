using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private int particleGroupIndex;
    [SerializeField] private string particleGroupName;
    [SerializeField] private GameObject highlightText;
    [SerializeField] private Material smudgedMat1;
    [SerializeField] private Material smudgedMat2;
    [SerializeField] private Material smudgedMat3;

    [SerializeField] private int particleIdx = -1;
    
    // Start is called before the first frame update
    void Awake()
    {
        actor = page.GetComponent<ObiActor>();
        
        if (particleIdx != -1 && particleGroupName != "")
        {
            foreach (var @group in actor.blueprint.groups.Where(@group => @group.name == particleGroupName))
            {
                particleGroup = @group;
                particleIdx = particleGroup.particleIndices[0];
            }
        } else if (particleIdx == -1 && particleGroupName == "")
        {
            particleIdx = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.SetPositionAndRotation(actor.GetParticlePosition(particleIdx), Quaternion.identity);
        string name = page.GetComponent<MeshRenderer>().material.name;
        if ((name == triggerMat.name + " (Instance)" || name == smudgedMat1.name + " (Instance)" || name == smudgedMat2.name + " (Instance)"|| name == smudgedMat3.name + " (Instance)") && transform.position.x > xBoundary)
        {
            gameObject.layer = LayerMask.NameToLayer("OutlineLayer");
            line.SetActive(true);
            highlightText.SetActive(true);
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            line.SetActive(false);
            highlightText.SetActive(false);
        }
        
        
    }
}


