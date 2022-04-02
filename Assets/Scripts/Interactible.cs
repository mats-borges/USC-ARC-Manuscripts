using System;
using System.Collections;
using System.Collections.Generic;
using HandPhysicsToolkit.Helpers.Interfaces;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class InteractibleEvent : UnityEvent<BaseInteractor> {}

public enum HandGrabType
{
   Pinch = 0,
   Grip = 1,
   Controller = 2
}

public class Interactible : MonoBehaviour
{
   public bool isLaserable = false;
   public HandGrabType handGrabType = HandGrabType.Pinch;
   
   [Header("Hand-Based Events")]
   public InteractibleEvent OnTouch;
   public InteractibleEvent OffTouch;
   public InteractibleEvent OnGrab;
   public InteractibleEvent OffGrab;

   [Header("Pointer-Based Events")]
   public InteractibleEvent OnPointerOver;
   public InteractibleEvent OnPointerExit;
   public InteractibleEvent OnPointerClick;
   public InteractibleEvent OnPointerUnclick;

   public bool TryGrab(Interactor interactor, HandGrabType grabType)
   {
      if (grabType != handGrabType && grabType != HandGrabType.Controller) return false;
      
      OnGrab.Invoke(interactor);

      return true;
   }
   
   public bool TryUngrab(Interactor interactor, HandGrabType grabType)
   {
      if (grabType != handGrabType && grabType != HandGrabType.Controller) return false;
      
      OffGrab.Invoke(interactor);

      return true;
   }

   private void Start()
   {
      if (OnTouch == null)
         OnTouch = new InteractibleEvent();
      if (OffTouch == null)
         OffTouch = new InteractibleEvent();
   }
}
