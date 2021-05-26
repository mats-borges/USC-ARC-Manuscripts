using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactible : MonoBehaviour
{
   public UnityEvent OnTouch;

   private void Start()
   {
      if (OnTouch == null)
         OnTouch = new UnityEvent();
   }
}
