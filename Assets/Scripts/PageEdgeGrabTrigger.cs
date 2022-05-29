using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Drawing;
using Obi;
using UnityEngine;
using UnityEngine.Serialization;

public class PageEdgeGrabTrigger : MonoBehaviourGizmos
{
    [SerializeField] private bool showGizmos = false;
    [FormerlySerializedAs("_trigger")] [SerializeField] private BoxCollider trigger;
    [SerializeField] private ObiActor actor;
    [SerializeField] private string pageEdgeParticleGroupName;
    private ObiParticleGroup _pageEdgeParticleGroup;

    private int minEdge = -1;
    private int maxEdge = -1;

    private bool isSetup = false;

    private void OnValidate()
    {
        isSetup = false;
    }

    void SetupProps()
    {
        if (isSetup) return;
        
        if(!trigger)
            trigger = GetComponent<BoxCollider>();
        
        foreach (var @group in actor.blueprint.groups.Where(@group => @group.name == pageEdgeParticleGroupName))
        {
            _pageEdgeParticleGroup = @group;
        }

        CalculateTriggerBounds();

        isSetup = true;
    }
    
    void Awake()
    {
        SetupProps();
    }

    void CalculateTriggerBounds()
    {
        var minEdgeFound = -1;
        var maxEdgeFound = -1;
        var maxDistFound = float.MinValue;
        foreach (var pIdx1 in _pageEdgeParticleGroup.particleIndices)
        {
            var p1Pos = actor.GetParticlePosition(pIdx1);
            foreach (var pIdx2 in _pageEdgeParticleGroup.particleIndices)
            {
                var p2Pos = actor.GetParticlePosition(pIdx2);
                var distance = Vector3.Distance(p1Pos, p2Pos);
                if (distance < maxDistFound) continue;

                maxDistFound = distance;
                minEdgeFound = pIdx1;
                maxEdgeFound = pIdx2;
            }
        }

        minEdge = minEdgeFound;
        maxEdge = maxEdgeFound;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Application.isPlaying) return;

        if (minEdge == maxEdge) CalculateTriggerBounds();

        var minEdgePos = actor.GetParticlePosition(minEdge);
        var maxEdgePos = actor.GetParticlePosition(maxEdge);
        var pageCenter = (minEdgePos + maxEdgePos) / 2;
        transform.position = pageCenter;
        transform.rotation = Quaternion.LookRotation((maxEdgePos - minEdgePos).normalized, Vector3.up);
    }

    public override void DrawGizmos()
    {
        if (!trigger || !showGizmos) return;

        if (Application.isPlaying == false)
        {
            SetupProps();
            
            var minEdgePos = actor.GetParticlePosition(minEdge);
            var maxEdgePos = actor.GetParticlePosition(maxEdge);
            var pageCenter = (minEdgePos + maxEdgePos) / 2;
            transform.position = pageCenter;
            transform.rotation = Quaternion.LookRotation((maxEdgePos - minEdgePos).normalized, Vector3.up);
        }

        var tform = transform;
        var avgScale = (tform.localScale.x + tform.localScale.y + tform.localScale.z) / 3.0f;
        Draw.WireBox(tform.position + trigger.center, tform.rotation, trigger.size * avgScale, new Color(1,1,1, 0.25f));
        
        foreach (var pIdx in actor.solverIndices)
        {
            var particlePos = actor.GetParticlePosition(pIdx);
            Draw.WireSphere(particlePos, 0.05f, Color.red);
            Draw.Label2D(particlePos, pIdx.ToString(), 12F);
        }
    }
}
