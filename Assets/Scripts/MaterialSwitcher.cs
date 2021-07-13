using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSwitcher : MonoBehaviour
{
    [SerializeField] private Material highlightMaterial;
    private MeshRenderer _meshRenderer;
    private Material originalMat;
    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        originalMat = _meshRenderer.material;
    }

    public void TurnOnHighlight(Color highlightColor)
    {
        
        _meshRenderer.material = highlightMaterial;
        _meshRenderer.material.SetColor("_Color", highlightColor);
    }
    
    public void TurnOnHighlight()
    {
        _meshRenderer.material = highlightMaterial;
    }

    public void TurnOffHighlight()
    {
        _meshRenderer.material = originalMat;
    }
}
