using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using JetBrains.Annotations;

namespace W02
{
    [RequireComponent(typeof(Controller2D))]
    public class PlayerMove : PlayerAbility
    {


        float currentVelocityX;
        float velocityXSmoothing;

        [Header("Player 속성")]
        [SerializeField][Tooltip("최대 속력")] float maxMoveSpeed = 6f;
        [SerializeField][Tooltip("공중 가속")] float accelerationOnAir = 0.2f;
        [SerializeField][Tooltip("지상 가속")] float accelerationOnGround = 0.2f;
        [SerializeField][Tooltip("공중 감속")] float decelerationOnAir = 0.2f;
        [SerializeField][Tooltip("지상 감속")] float decelerationOnGround = 0.2f;
        [SerializeField][Tooltip("최대 속도 공중 감속 / 합연산")] float decelerationMaxSpeedOnAir = 0.2f;
        [SerializeField][Tooltip("최대 속도 지상 감속 / 합연산")] float decelerationMaxSpeedOnGround = 0.2f;
        [SerializeField][Tooltip("피격될 때 속도 감소치 / 곱연산")] float penaltySpeedDizzy = 0.8f;
        [SerializeField][Tooltip("걸어다닐때 속도 감소치 / 곱연산")] float penaltySpeedWalk = 0.6f;
        [SerializeField][Tooltip("로프 걸때 속도 감소치 / 곱연산")] float penaltySpeedRope = 0.6f;

        float moveSpeed = 6f;

        protected override void HandleInput()
        {
            //currentVelocityX = _horizontalMove * moveSpeed;
            //_controller.SetXVelocity(
            //    Mathf.SmoothDamp(
            //            _controller.controllerPhysics.velocity.x,
            //            currentVelocityX,
            //            ref velocityXSmoothing,
            //            (_controller.controllerPhysics.collisions.below) ? accelerationTimeOnGround : accelerationTimeOnAir
            //        )
            //    );
            
            
            this.CalculateVelocity();
            _controller.SetXVelocity(currentVelocityX);
            Debug.Log("velocityX :" + currentVelocityX);
            //this.MoveMent();
        }

        /// <summary>
        /// Calculate Velocity when Player Moves Left or Right in Idle/Walking State
        /// </summary>
        void CalculateVelocity()
        {
            //if (_player.playerInfo.state == Player.State.WALL_GRAB) return;
            //if (_player.playerInfo.state == Player.State.JUMPING) return;
            // add exception state Up here

            int xInputDirection = RoundNormalize(InputManager.Instance.MoveHorizontal);
            int currentXDirection = RoundNormalize(currentVelocityX);
            bool isFasterThanMaxSpeed = Mathf.Abs(currentVelocityX) > maxMoveSpeed;
            float targetMaxSpeed = MaxSpeedSetting(_player.playerInfo.state);
            float decelerationWhenMaxSpeed = _player.playerInfo.isGrounded ? decelerationMaxSpeedOnGround : decelerationMaxSpeedOnAir;
            float deceleration = _player.playerInfo.isGrounded ? decelerationOnGround : decelerationOnAir;
            float acceleration = _player.playerInfo.isGrounded ? accelerationOnGround : accelerationOnAir;

            if (currentXDirection == 1)
            {
                if (xInputDirection == 1)
                {
                    if (!isFasterThanMaxSpeed)
                    {
                        currentVelocityX += acceleration * Time.deltaTime;

                        if (currentVelocityX > maxMoveSpeed)
                        {
                            currentVelocityX = maxMoveSpeed;
                        }
                    }
                }
                else if (xInputDirection == -1)
                {
                    currentVelocityX -= acceleration * Time.deltaTime;
                }
                else
                {
                    currentVelocityX -= deceleration * Time.deltaTime;
                    if (currentVelocityX < 0f)
                    {
                        currentVelocityX = 0f;
                    }
                }
                if (isFasterThanMaxSpeed)
                {
                    currentVelocityX -= decelerationWhenMaxSpeed * Time.deltaTime;
                }
            }
            else if (currentXDirection == -1)
            {
                if (xInputDirection == -1)
                {
                    if (!isFasterThanMaxSpeed)
                    {
                        currentVelocityX -= acceleration * Time.deltaTime;
                        if (currentVelocityX < -maxMoveSpeed)
                        {
                            currentVelocityX = maxMoveSpeed;
                        }
                    }
                }
                else if (xInputDirection == 1)
                {
                    currentVelocityX += acceleration * Time.deltaTime;
                }
                else
                {
                    currentVelocityX += deceleration * Time.deltaTime;
                    if (currentVelocityX > 0f)
                    {
                        currentVelocityX = 0f;
                    }
                }
                if (isFasterThanMaxSpeed)
                {
                    currentVelocityX += decelerationWhenMaxSpeed * Time.deltaTime;
                }
            }
            else if (currentXDirection == -1)
            {
                if (xInputDirection == -1)
                {
                    if (!isFasterThanMaxSpeed)
                    {
                        currentVelocityX -= acceleration * Time.deltaTime;
                        if (currentVelocityX < -targetMaxSpeed)
                        {
                            currentVelocityX = -targetMaxSpeed;
                        }
                    }
                }
                else if (xInputDirection == 1)
                {
                    currentVelocityX += acceleration * Time.deltaTime;
                }
                else
                {
                    currentVelocityX += deceleration * Time.deltaTime;
                    if (currentVelocityX > 0f)
                    {
                        currentVelocityX = 0f;
                    }
                }
                if (isFasterThanMaxSpeed)
                {
                    currentVelocityX += decelerationWhenMaxSpeed * Time.deltaTime;
                }
            }
            else if (xInputDirection == -1)
            {
                currentVelocityX -= acceleration * Time.deltaTime;
                if (currentVelocityX < -targetMaxSpeed)
                {
                    currentVelocityX = -targetMaxSpeed;
                }
            }
            else if (xInputDirection == 1)
            {
                currentVelocityX += acceleration * Time.deltaTime;
                if (currentVelocityX > targetMaxSpeed)
                {
                    currentVelocityX = targetMaxSpeed;
                }
            }
        }


        void MoveMent()
        {
            Vector2 vector = Vector2.right * currentVelocityX * Time.deltaTime;
            this.Move(vector, 0, 0);
        }

        void Move(Vector2 moveAmount, float _minHeight = 0, float _maxHeight = 0)
        {
            this._controller.Move(moveAmount, _minHeight, _maxHeight);
        }

        /// <summary>
        /// Returns targetMaxSpeed based on player's state
        /// </summary>
        /// <param name="_state"></param>
        /// <returns></returns>
        float MaxSpeedSetting(Player.State _state)
        {
            switch ( _state )
            {
                case Player.State.WALKING:
                    return maxMoveSpeed * penaltySpeedWalk;
                case Player.State.DIZZY:
                    return maxMoveSpeed * penaltySpeedDizzy;
                case Player.State.ROPE:
                    return maxMoveSpeed * penaltySpeedRope;
                default:
                    return maxMoveSpeed;
            }
        }

        /// <summary>
        /// To make normalized round int Value
        /// </summary>
        /// <param name="_value"></param>
        int RoundNormalize(float _value)
        {
            if (_value == 0f)
            {
                return 0;
            }
            if(_value >= 0f)
            {
                return 1;
            }
            return -1;
        }
    }
}
