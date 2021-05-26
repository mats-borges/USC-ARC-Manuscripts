using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class InteractibleEvent : UnityEvent<Interactor> {}

public class Interactible : MonoBehaviour
{
   public UnityEvent OnTouch;
   public UnityEvent OffTouch;
   public InteractibleEvent OnGrab;
   public InteractibleEvent OffGrab;

   private void Start()
   {
      if (OnTouch == null)
         OnTouch = new UnityEvent();
      if (OffTouch == null)
         OffTouch = new UnityEvent();
   }
}
