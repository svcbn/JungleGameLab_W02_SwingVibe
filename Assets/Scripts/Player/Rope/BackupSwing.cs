using System.Collections;
using System.Collections.Generic;
using UnityEditor.MPE;
using UnityEngine;
using UnityEngine.InputSystem;
using W02;


public class BackupSwing : PlayerAbility
{
    [Header("로프 최대 속력")] [SerializeField] float maxMoveSpeed = 6f;
    [Header("로프 최대 각도")] [SerializeField] float maxRopeAngle = 110f;

    [Header("로프 공중 가속")] [SerializeField] float accelerationOnAir = 0.2f;
    [Header("로프 지상 가속")] [SerializeField] float accelerationOnGround = 0.2f;

    [Header("로프 공중 감속")] [SerializeField] float decelerationOnAir = 0.2f;
    [Header("로프 지상 감속")] [SerializeField] float decelerationOnGround = 0.2f;

    [Header("로프 최대 속도 공중 감속 (합연산)")] [SerializeField] float decelerationMaxSpeedOnAir = 0.2f;
    [Header("로프 최대 속도 지상 감속 (합연산)")] [SerializeField] float decelerationMaxSpeedOnGround = 0.2f;

    [Header("로프 피격될 때 속도 감소치 (곱연산)")] [SerializeField] float penaltySpeedDizzy = 0.8f;
    [Header("로프 걸어다닐때 속도 감소치 (곱연산)")] [SerializeField] float penaltySpeedWalk = 0.6f;
    [Header("로프 걸때 속도 감소치 (곱연산)")] [SerializeField] float penaltySpeedRope = 1f;

    //public Chain rope;
    public RopeChain ropeChain;
    public float ropeSpeed = 10f;
    public float ropeGravityAbs = 20f;
    float targetVelocityX = 0;
    float currentVelocityY = 0;
    float direction;
    float theta; // 초기각
    float angularAcceleration;
    float centripetalAcceleration;
    float rotateSpeed = 0.01f;

    protected override void HandleInput()
    {
        //hook버튼 클릭시 state변경
        if (_hookButtonClicked
                && _player.playerInfo.state != Player.State.ROPE)
        {
            _player.playerInfo.ropeState = Player.RopeState.HOOKED;
            _player.ChangeState(Player.State.ROPE);
        }

        //HookedRope
        else if (_hookButtonClicked
                && _player.playerInfo.state == Player.State.ROPE
                && _player.playerInfo.ropeState == Player.RopeState.HOOKED)
        {
            //플레이어의 위치 조정 추가하기
            CreateRope();
            _player.playerInfo.ropeState = Player.RopeState.HOLDING;
        }

        //로프 타는중
        if (_hookButtonClicked && _player.playerInfo.ropeState == Player.RopeState.HOLDING)
        {
            if (ropeChain.RopeNodeCount != 0)
            {
                UpdateTheta(ropeChain.StartPoint, _player.transform.position);
                UpdatePendulumAcceleration();
            }
            HoldingRope();
        }

        //로프 실패
        if (_player.playerInfo.ropeState == Player.RopeState.HOLDING && !_hookButtonClicked)
        {
            ropeChain.CancelRope();
            _player.playerInfo.ropeState = Player.RopeState.FAILED;
            _player.ChangeState(Player.State.IDLE);
        }
    }

    private void CreateRope()
    {
        Vector2 playerPosition = (Vector2)_player.transform.position;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        _controller.SetVelocity(Vector2.zero);
        //_player.transform.position = ropeChain.CreateRope(mousePosition, playerPosition, _player);
    }

    private void HoldingRope()
    {
        CalculateVelocity();
        if (_controller.IsOnGround && Vector2.Distance((Vector2)_player.transform.position, ropeChain.StartPoint) <= ropeChain.RopeLength)
        {
            _controller.SetXVelocity(targetVelocityX);
            ropeChain.SetEndNodePos(_player.transform.position);
            return;
        }
        CalculateRopeSwinging();
    }

    public void CalculateRopeSwinging()
    {
        AddVelocity();
        Vector2 playerPosition = _player.transform.position;

        Vector2 ropeDirection = ropeChain.StartPoint - playerPosition;
        Vector2 v1 = TranslateForce(new Vector2(targetVelocityX, 0), ropeDirection, RoundNormalize(targetVelocityX));
        Vector2 v3 = (v1);

        Vector2 nextMovePoint = (Vector2)playerPosition + v3 * 0.01f;
        Vector2 nextMovePointToOriginChainNode = ropeChain.RopeLength * (ropeChain.StartPoint - (Vector2)nextMovePoint).normalized;
        Vector2 newMoveVector = -nextMovePointToOriginChainNode + ropeDirection;
        this._controller.AddVelocity(newMoveVector);
        // Vector2 velNorm = _controller.controllerPhysics.velocity.normalized;
        // int xInputDirection = RoundNormalize(InputManager.Instance.MoveHorizontal);
        // int currentXDirection = RoundNormalize(targetVelocityX);
        // float speedDelta;

        // if (xInputDirection == currentXDirection && xInputDirection != 0) {
        //     speedDelta = accelerationOnAir * Time.deltaTime;
        // } else {
        //     speedDelta = -decelerationOnAir * Time.deltaTime;
        // }

        // float curSpeed = _controller.controllerPhysics.velocity.magnitude;
        
        // float maxSpeed = Mathf.Sqrt(2 * ropeGravityAbs * ropeChain.RopeLength * (Mathf.Cos(theta) - Mathf.Cos(maxRopeAngle)));
        // Debug.Log(maxSpeed);
        // float targetSpeed = curSpeed + speedDelta;
        // if (targetSpeed > maxSpeed)
        // {
        //     targetSpeed = maxSpeed;
        // }

        // Vector2 playerPosition = _player.transform.position;
        // Vector2 ropeDirection = ropeChain.StartPoint - playerPosition;
        // Vector2 v1 = TranslateForce(velNorm * targetSpeed, ropeDirection, RoundNormalize(velNorm.x * targetSpeed));
        // Vector2 nextMovePoint = (Vector2)playerPosition + v1 * Time.deltaTime;
        // Vector2 nextMovePointToOriginChainNode = ropeChain.RopeLength * (ropeChain.StartPoint - (Vector2)nextMovePoint).normalized;
        // Vector2 newMoveVector = -nextMovePointToOriginChainNode + ropeDirection;
        
        //this._controller.AddVelocity(newMoveVector);

        return;
    }

    Vector2 TranslateForce(Vector2 rawForce, Vector2 ropeDirection, float dir)
    {
        Vector2 translatedVector;
        Vector3 playerMoveVector = Vector3.Cross(new Vector3(0, 0, -1), ropeDirection);

        if (dir < 0)
            playerMoveVector = -1 * playerMoveVector;

        float deg = Vector2.Angle(rawForce, playerMoveVector);
        translatedVector = ((Vector2)playerMoveVector).normalized * rawForce.magnitude * Mathf.Cos(Mathf.Deg2Rad * deg);

        return translatedVector;
    }

    int RoundNormalize(float _value)
    {
        if (_value == 0f)
        {
            return 0;
        }
        if (_value >= 0f)
        {
            return 1;
        }
        return -1;
    }


    void CalculateVelocity()
    {
        int xInputDirection = RoundNormalize(InputManager.Instance.MoveHorizontal);
        int currentXDirection = RoundNormalize(targetVelocityX);
        float targetMaxSpeed = maxMoveSpeed * penaltySpeedRope;
        bool isFasterThanMaxSpeed = Mathf.Abs(targetVelocityX) > targetMaxSpeed;
        float decelerationWhenMaxSpeed = _controller.IsOnGround ? decelerationMaxSpeedOnGround : decelerationMaxSpeedOnAir;
        float deceleration = _controller.IsOnGround ? decelerationOnGround : decelerationOnAir;
        float acceleration = _controller.IsOnGround ? accelerationOnGround : accelerationOnAir;
   
        if (currentXDirection == 1) // 현재진행방향 ->
        {
            if (xInputDirection == 1) // 입력방향 ->
            {
                if (!isFasterThanMaxSpeed)
                {
                    targetVelocityX += acceleration * Time.deltaTime;

                    if (targetVelocityX > targetMaxSpeed)
                    {
                        targetVelocityX = targetMaxSpeed;
                    }
                }
            }
            else if (xInputDirection == -1) // 입력방향 <-
            {
                targetVelocityX -= acceleration * Time.deltaTime;
            }
            else // 입력 0
            {
                targetVelocityX -= deceleration * Time.deltaTime;
                if (targetVelocityX < 0f)
                {
                    targetVelocityX = 0f;
                }
            }
            if (isFasterThanMaxSpeed)
            {
                targetVelocityX -= decelerationWhenMaxSpeed * Time.deltaTime;
            }
        }
        else if (currentXDirection == -1) // 현재진행방향 <-
        {
            if (xInputDirection == -1) // 입력방향 <-
            {
                if (!isFasterThanMaxSpeed)
                {
                    targetVelocityX -= acceleration * Time.deltaTime;
                    if (targetVelocityX < -targetMaxSpeed)
                    {
                        targetVelocityX = -targetMaxSpeed;
                    }
                }
            }
            else if (xInputDirection == 1) // 입력방향 ->
            {
                targetVelocityX += acceleration * Time.deltaTime;
            }
            else // 입력 0
            {
                targetVelocityX += deceleration * Time.deltaTime;
                if (targetVelocityX > 0f)
                {
                    targetVelocityX = 0f;
                }
            }
            if (isFasterThanMaxSpeed)
            {
                targetVelocityX += decelerationWhenMaxSpeed * Time.deltaTime;
            }
        }
        else if (xInputDirection == -1)
        {
            targetVelocityX -= acceleration * Time.deltaTime;
            if (targetVelocityX < -targetMaxSpeed)
            {
                targetVelocityX = -targetMaxSpeed;
            }
        }
        else if (xInputDirection == 1)
        {
            targetVelocityX += acceleration * Time.deltaTime;
            if (targetVelocityX > targetMaxSpeed)
            {
                targetVelocityX = targetMaxSpeed;
            }
        }
    }

    void UpdateTheta(Vector3 hookedPosition, Vector3 currentPosition)
    {
        float Xdistance = hookedPosition.x - currentPosition.x;
        float Ydistance = hookedPosition.y - currentPosition.y;

        theta = Mathf.Atan2(Xdistance, Ydistance);
    }

    void UpdatePendulumAcceleration()
    {
        angularAcceleration = -ropeGravityAbs * Mathf.Sin(theta);
        centripetalAcceleration = Mathf.Pow(_controller.controllerPhysics.velocity.magnitude, 2) / ropeChain.RopeLength;
    }

    private void AddVelocity()
    {
        Vector2 playerPosition = _player.transform.position;
        Vector2 toOriginChainNode = ropeChain.StartPoint - playerPosition;

        Vector2 t = toOriginChainNode.normalized * centripetalAcceleration;
        Vector2 g = ((Vector2)Vector3.Cross(new Vector3(0, 0, 1), toOriginChainNode)).normalized * angularAcceleration;

        //Debug.Log(t + " , " + g + " t + g : " + (t + g));
        //g = TranslateForce(g, toOriginChainNode, RoundNormalize(g.x));
        _controller.AddVelocity((t + g) * Time.deltaTime);
    }

}
