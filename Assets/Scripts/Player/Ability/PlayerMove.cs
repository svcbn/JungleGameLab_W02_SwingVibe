using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace W02
{
    [RequireComponent(typeof(Controller2D))]
    public class PlayerMove : PlayerAbility
    {
        Vector3 velocity;
        float targetVelocityX;
        float _moveSpeed = 6f;
        float velocityXSmoothing;
        public float jumpHeight = 4;
        public float timeToJumpApex = 0.4f;
        float accelerationTimeAirborne = 0.2f;
        float accelerationTimeGrounded = 0.1f;

        Controller2D controller;

        protected override void Awake()
        {
            controller = GetComponent<Controller2D>();
        }

        

        protected override void HandleInput()
        {
            targetVelocityX = _horizontalMove * _moveSpeed;
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

            controller.Move(velocity * Time.deltaTime);
        }
    }
}


