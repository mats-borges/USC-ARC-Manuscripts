using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Platform.Samples.VrHoops;
using UnityEngine;
using OVR;

public class MovementManager : MonoBehaviour
{
    //controls player movement with oculus controllers
    
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject CenterEyeAnchor;
    private Rigidbody pr;
    private Quaternion lookDirection = Quaternion.identity;
    private Vector3 normalizedLookDirection = Vector3.forward;
    public float playerSpeed = 1;

    private void Start()
    {
        pr = player.GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        //secret speed button for debugging
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            playerSpeed *= 3;
        }
        else if (OVRInput.GetUp(OVRInput.Button.Two))
        {
            playerSpeed /= 3;
        }

        //controls movement relative to the direction the player is facing.
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
