using System;
using System.Collections;
using System.Collections.Generic;
using HandPhysicsToolkit.Helpers.Interfaces;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class InteractibleEvent : UnityEvent<BaseInteractor> {}

public class Interactible : MonoBehaviour
{
   public bool isLaserable = false;
   
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

   private void Start()
   {
      if (OnTouch == null)
         OnTouch = new InteractibleEvent();
      if (OffTouch == null)
         OffTouch = new InteractibleEvent();
   }
}
