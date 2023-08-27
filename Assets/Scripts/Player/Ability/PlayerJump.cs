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
        if (_controller.IsOnGround && InputManager.Instance.JumpButton && _player.playerInfo.state != Player.State.ROPE) // 바닥에 있을 때만 점프 가능
        {
            _player.playerInfo.isJumping = true;
            _controller.SetYVelocity(jumpHeight);
        }

        if (_player.playerInfo.isJumping && !_controller.WasOnGound && !_controller.IsOnGround)
        {
            _player.playerInfo.isJumping = false;
        }

        if (_player.playerInfo.state == Player.State.ROPE)
        {
            _player.playerInfo.isJumping = false;
        }
    }
}
