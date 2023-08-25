using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace W02
{
    [RequireComponent(typeof(Controller2D))]
    public class PlayerMove : PlayerAbility
    {
        float targetVelocityX;
        float _moveSpeed = 6f;
        float velocityXSmoothing;
        float accelerationTimeAirborne = 0.2f;
        float accelerationTimeGrounded = 0.1f;

        protected override void HandleInput()
        {
            targetVelocityX = _horizontalMove * _moveSpeed;
            _controller.SetXVelocity(
                Mathf.SmoothDamp(
                        _controller.velocity.x, 
                        targetVelocityX, 
                        ref velocityXSmoothing, 
                        (_controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne
                    )
                );
        }
    }
}
