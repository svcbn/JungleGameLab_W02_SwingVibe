using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using W02;

public class PlayerJump : PlayerAbility
{
    public float jumpHeight = 15;

    protected override void HandleInput()
    {
        if (_controller.IsOnGround && InputManager.Instance.JumpButton) // 바닥에 있을 때만 점프 가능
        {
            _controller.SetYVelocity(jumpHeight);
        }
    }
}
