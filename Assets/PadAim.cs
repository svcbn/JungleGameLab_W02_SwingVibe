using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Users;
using W02;
using UnityEngine.InputSystem;


public class PadAim : MonoBehaviour
{
    InputUser user;
    bool isGamepad = false;
    Vector2 aimPosition;
    Vector2 aimVec;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateDeviceType(user);
        if (!isGamepad)
        {
            aimPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            aimVec = (aimPosition - (Vector2)this.transform.position).normalized;
            this.transform.rotation = Quaternion.FromToRotation(Vector2.right, aimVec);
        }
        else
        {
            aimVec = (new Vector2(InputManager.Instance.AimHorizontal, InputManager.Instance.AimVertical)).normalized;
            this.transform.rotation = Quaternion.FromToRotation(Vector2.right, aimVec);
        }

    }

    void UpdateDeviceType(InputUser user)
    {
        string device = user.controlScheme.Value.name;
        if (device == "Gamepad")
        {
            isGamepad = true;
        }
        else
        {
            isGamepad = false;
        }
    }

}
