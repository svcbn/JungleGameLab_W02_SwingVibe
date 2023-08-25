using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using W02;

public class PlayerJump : PlayerAbility
{
    float jumpHeight = 4;

    protected override void HandleInput()
    {
        if (_controller.collisions.below && InputManager.Instance.JumpButton) // 바닥에 있을 때만 점프 가능
        {
            Debug.Log(Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(_controller.gravity)));

            _controller.SetYVelocity(Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(_controller.gravity)));
        }
    }
}
