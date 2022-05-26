using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private Material smudgedMat3;
    [SerializeField] float xBoundary;
    [SerializeField] private string particleGroupName = "";
    private int pageStage;
    [SerializeField] private int particleIdx = -1;

    // Start is called before the first frame update
    void Awake()
    {
        pageStage = 0;
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
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }

    public void AdvanceSmudgeMaterial()
    {
        string name = page.GetComponent<MeshRenderer>().material.name;
        Debug.Log(name);
        if ((name == triggerMat.name + " (Instance)" || name == smudgedMat1.name + " (Instance)" || name == smudgedMat2.name + " (Instance)"|| name == smudgedMat3.name + " (Instance)") && transform.position.x > xBoundary)
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
                    page.GetComponent<MeshRenderer>().material = smudgedMat3;
                    pageStage++;
                    break;
                case 3:
                    page.GetComponent<MeshRenderer>().material = triggerMat;
                    pageStage = 0;
                    break;
            }
        }
    }
}
