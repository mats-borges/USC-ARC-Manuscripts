using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSwitcher : MonoBehaviour
{
    [SerializeField] private Material highlightMaterial;
    private Material originalMat;
    private void Start()
    {
        originalMat = GetComponent<MeshRenderer>().material;
    }

    public void TurnOnHighlight()
    {
        GetComponent<MeshRenderer>().material = highlightMaterial;
    }

    public void TurnOffHighlight()
    {
        GetComponent<MeshRenderer>().material = originalMat;
    }
}
