using System.Collections;
using System.Collections.Generic;
using HandPhysicsToolkit.Helpers.Interfaces;
using UnityEngine;
using UnityEngine.Events;

public class UnturnableCollider : MonoBehaviour
{
    //controls the page turning "magic trick" 
    
    //check if player is trying to grab
    //check if grabber is on the side closer to the hand
    //invoke the magic trick unity events in the inspector
    
    public GraspingPoint.SimPageSide colliderSide = GraspingPoint.SimPageSide.RightSide;

    //InteractibleEvent is defined in Interactible.cs
    [SerializeField] private InteractibleEvent pageMagic;
    [SerializeField] private GraspingPoint _graspingPoint;
    
    public void PageMagicCheck(BaseInteractor interactor)
    {
        if (!_graspingPoint) return;

        var currentPageSide = _graspingPoint.GetSimPageSide();

        if (currentPageSide == colliderSide) return;
        
        pageMagic.Invoke(interactor);
    }
}
