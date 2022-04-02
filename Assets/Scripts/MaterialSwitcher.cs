using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSwitcher : MonoBehaviour
{
    private Material originalMat;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material clickedMaterial;
    private MeshRenderer _meshRenderer;
    
    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        originalMat = _meshRenderer.material;
    }

    public void TurnOnHighlight(Color highlightColor)
    {
        
        _meshRenderer.material = highlightMaterial;
        _meshRenderer.material.SetColor("_BaseColor", highlightColor);
    }
    
    public void TurnOnHighlight()
    {
        _meshRenderer.material = highlightMaterial;
    }

    public void TurnOffHighlight()
    {
        _meshRenderer.material = originalMat;
    }

    public void TurnOnClickedMaterial()
    {
        _meshRenderer.material = clickedMaterial;
    }
    
    public void TurnOffClickedMaterial()
    {
        _meshRenderer.material = highlightMaterial;
    }
}
