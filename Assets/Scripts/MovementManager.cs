using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Platform.Samples.VrHoops;
using UnityEngine;
using OVR;

public class MovementManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject LController, RController;

    [SerializeField] private GameObject TrackingSpace;
    [SerializeField] private GameObject CenterEyeAnchor;
    
    private Rigidbody pr;
    private Transform pt;
    
    private Quaternion lookDirection = Quaternion.identity;
    private Vector3 normalizedLookDirection = Vector3.forward;
    public float playerSpeed = 1;

    private void Start()
    {
        pt = player.transform;
        pr = player.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        var centerEyeTransform = CenterEyeAnchor.transform;

        lookDirection = centerEyeTransform.transform.rotation;
        lookDirection.eulerAngles = new Vector3(0, lookDirection.eulerAngles.y, 0);
        
        normalizedLookDirection = lookDirection * Vector3.forward;
        var lookDirectionRight = Vector3.Cross(Vector3.up, normalizedLookDirection);

        var joystickAxisR = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick, OVRInput.Controller.LTouch);
        var joystickAxisU = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick, OVRInput.Controller.RTouch);
        
        var fwdMove = joystickAxisR.y * Time.deltaTime * playerSpeed * (normalizedLookDirection);
        var strafeMove = joystickAxisR.x * Time.deltaTime * playerSpeed * (lookDirectionRight);
        pr.position += fwdMove + strafeMove;

        pr.position += joystickAxisU.y * Time.deltaTime * playerSpeed * transform.up;

    }
}
