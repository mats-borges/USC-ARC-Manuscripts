using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineConnector : MonoBehaviour
{
    private LineRenderer line;
    [SerializeField] private GameObject origin;
    [SerializeField] private GameObject target;
    private Vector3 start, end;
    //private SphereCollider m_collider;
    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        //m_collider = GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        start = (origin.transform.position);
        end = target.transform.position;
        
        var direction = (end - start).normalized;
        
        line.SetPosition(0,start );
        line.SetPosition(1,end);
    }
    
    //+ (direction * 2 * m_collider.radius)
}
