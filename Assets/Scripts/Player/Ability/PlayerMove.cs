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

        [Header("최대 속력")][SerializeField] float maxMoveSpeed = 6f;

        [Header("공중 가속")][SerializeField] float accelerationOnAir = 0.2f;
        [Header("지상 가속")][SerializeField] float accelerationOnGround = 0.2f;

        [Header("공중 감속")][SerializeField] float decelerationOnAir = 0.2f;
        [Header("지상 감속")][SerializeField] float decelerationOnGround = 0.2f;

        [Header("최대 속도 공중 감속 (합연산)")][SerializeField] float decelerationMaxSpeedOnAir = 0.2f;
        [Header("최대 속도 지상 감속 (합연산)")][SerializeField] float decelerationMaxSpeedOnGround = 0.2f;

        [Header("피격될 때 속도 감소치 (곱연산)")][SerializeField] float penaltySpeedDizzy = 0.8f;
        [Header("걸어다닐때 속도 감소치 (곱연산)")][SerializeField] float penaltySpeedWalk = 0.6f;
        [Header("로프 걸때 속도 감소치 (곱연산)")][SerializeField] float penaltySpeedRope = 0.6f;



        float moveSpeed = 6f;

        protected override void HandleInput()
        {         
            this.CalculateVelocity();
            _controller.SetXVelocity(currentVelocityX);
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
            float decelerationWhenMaxSpeed = _controller.IsOnGround ? decelerationMaxSpeedOnGround : decelerationMaxSpeedOnAir;
            float deceleration = _controller.IsOnGround ? decelerationOnGround : decelerationOnAir;
            float acceleration = _controller.IsOnGround ? accelerationOnGround : accelerationOnAir;

            if (currentXDirection == 1)
            {
                if (xInputDirection == 1)
                {
                    if (!isFasterThanMaxSpeed)
                    {
                        currentVelocityX += accelerationTime * Time.deltaTime;

                        if (currentVelocityX > maxMoveSpeed)
                        {
                            currentVelocityX = maxMoveSpeed;
                        }
                    }
                }
                else if (xInputDirection == -1)
                {
                    currentVelocityX -= accelerationTime * Time.deltaTime;
                }
                else
                {
                    currentVelocityX -= decelerationTime * Time.deltaTime;
                    if (currentVelocityX < 0f)
                    {
                        currentVelocityX = 0f;
                    }
                }
                if (isFasterThanMaxSpeed)
                {
                    currentVelocityX -= decelerationSpeed * Time.deltaTime;
                }
            }
            else if (currentXDirection == -1)
            {
                if (xInputDirection == -1)
                {
                    if (!isFasterThanMaxSpeed)
                    {
                        currentVelocityX -= accelerationTime * Time.deltaTime;
                        if (currentVelocityX < -maxMoveSpeed)
                        {
                            currentVelocityX = maxMoveSpeed;
                        }
                    }
                }
                else if (xInputDirection == 1)
                {
                    currentVelocityX += accelerationTime * Time.deltaTime;
                }
                else
                {
                    currentVelocityX += decelerationTime * Time.deltaTime;
                    if (currentVelocityX > 0f)
                    {
                        currentVelocityX = 0f;
                    }
                }
                if (isFasterThanMaxSpeed)
                {
                    currentVelocityX += decelerationSpeed * Time.deltaTime;
                }
            }
            else if (currentXDirection == -1)
            {
                if (xInputDirection == -1)
                {
                    if (!isFasterThanMaxSpeed)
                    {
                        currentVelocityX -= accelerationTime * Time.deltaTime;
                        if (currentVelocityX < -targetMaxSpeed)
                        {
                            currentVelocityX = -targetMaxSpeed;
                        }
                    }
                }
                else if (xInputDirection == 1)
                {
                    currentVelocityX += accelerationTime * Time.deltaTime;
                }
                else
                {
                    currentVelocityX += decelerationTime * Time.deltaTime;
                    if (currentVelocityX > 0f)
                    {
                        currentVelocityX = 0f;
                    }
                }
                if (isFasterThanMaxSpeed)
                {
                    currentVelocityX += decelerationSpeed * Time.deltaTime;
                }
            }
            else if (xInputDirection == -1)
            {
                currentVelocityX -= accelerationTime * Time.deltaTime;
                if (currentVelocityX < -targetMaxSpeed)
                {
                    currentVelocityX = -targetMaxSpeed;
                }
            }
            else if (xInputDirection == 1)
            {
                currentVelocityX += accelerationTime * Time.deltaTime;
                if (currentVelocityX > targetMaxSpeed)
                {
                    currentVelocityX = targetMaxSpeed;
                }
            }
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
