using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Platform.Samples.VrHoops;
using UnityEngine;
using OVR;

public class MovementManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private Rigidbody pr;
    private Transform pt;
    public float playerSpeed = 1;

    private void Start()
    {
        pt = player.transform;
        pr = player.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        var joystickAxisR = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick, OVRInput.Controller.LTouch);
        var joystickAxisU = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick, OVRInput.Controller.RTouch);

        pr.position += (transform.right * joystickAxisR.x + transform.forward * joystickAxisR.y) * Time.deltaTime * playerSpeed;
        pr.position += (transform.right * joystickAxisU.x + transform.up * joystickAxisU.y) * Time.deltaTime * playerSpeed;

    }
}
