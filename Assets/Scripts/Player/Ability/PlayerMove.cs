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
        Vector3 velocity;
        float targetVelocityX;
        float _moveSpeed = 6f;
        float velocityXSmoothing;
        public float jumpHeight = 4;
        public float timeToJumpApex = 0.4f;
        float accelerationTimeAirborne = 0.2f;
        float accelerationTimeGrounded = 0.1f;

        float gravity;
        float verticalVelocity;
        bool _jumpPressed = false; // 점프 입력을 나타내는 변수

        Controller2D controller;

        protected override void Awake()
        {
            controller = GetComponent<Controller2D>();
            gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        }

        protected override void HandleInput()
        {
            targetVelocityX = _horizontalMove * _moveSpeed;
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

            // 중력 적용 및 이동
            verticalVelocity += gravity * Time.deltaTime;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);


            // "Ground" 레이어와 충돌을 감지할 때 중력의 영향을 받지 않도록 처리
            if (!controller.collisions.below)
            {
                verticalVelocity += gravity * Time.deltaTime;
                velocity.y = verticalVelocity;
            }
            else
            {
                verticalVelocity = 0f;
                velocity.y = 0f;
            }


            if (controller.collisions.below && _jumpPressed) // 바닥에 있을 때만 점프 가능
            {
                verticalVelocity = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(gravity));
                //Debug.Log(verticalVelocity);
                velocity.y = verticalVelocity;
            }

            if (InputManager.Instance.JumpButton)
            {
                _jumpPressed = true;
            }
            if (!InputManager.Instance.JumpButton)
            {
                _jumpPressed = false;
            }
            controller.Move(velocity * Time.deltaTime);
            Debug.Log(controller.collisions.below);
        }
    }
}
