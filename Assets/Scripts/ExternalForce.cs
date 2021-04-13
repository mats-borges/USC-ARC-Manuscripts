using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalForce : MonoBehaviour
{

    [SerializeField] GameObject forceMarker;


    void Update()
    {
        GetComponent<Cloth>().externalAcceleration = forceMarker.transform.position; ;
    }
}
