using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetObject : MonoBehaviour
{
    private Vector3 startPos;
    private Quaternion startRot;
    
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetPositionAndRotation()
    {
        transform.position = startPos;
        transform.rotation = startRot;
    }
}
