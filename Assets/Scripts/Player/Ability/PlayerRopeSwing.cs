using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using W02;


public class PlayerRopeSwing : PlayerAbility
{
    public Chain rope;
    public float ropeSpeed = 10f;
    public float ropeGravity = -5f;
    float currentVelocityX = 0;
    float currentVelocityY = 0;
    float direction;

    [Header("로프 최대 속력")] [SerializeField] float maxMoveSpeed = 6f;

    [Header("로프 공중 가속")] [SerializeField] float accelerationOnAir = 0.2f;
    [Header("로프 지상 가속")] [SerializeField] float accelerationOnGround = 0.2f;

    [Header("로프 공중 감속")] [SerializeField] float decelerationOnAir = 0.2f;
    [Header("로프 지상 감속")] [SerializeField] float decelerationOnGround = 0.2f;

    [Header("로프 최대 속도 공중 감속 (합연산)")] [SerializeField] float decelerationMaxSpeedOnAir = 0.2f;
    [Header("로프 최대 속도 지상 감속 (합연산)")] [SerializeField] float decelerationMaxSpeedOnGround = 0.2f;

    [Header("로프 피격될 때 속도 감소치 (곱연산)")] [SerializeField] float penaltySpeedDizzy = 0.8f;
    [Header("로프 걸어다닐때 속도 감소치 (곱연산)")] [SerializeField] float penaltySpeedWalk = 0.6f;
    [Header("로프 걸때 속도 감소치 (곱연산)")] [SerializeField] float penaltySpeedRope = 1f;

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
            if (rope.nodes.Count != 0)
            {

                UpdateTheta(rope.nodes[rope.chainMaxCount - 1].position, _player.transform.position);
                TestVelocity();
            }
            HoldingRope();

        }

        //로프 실패
        if (_player.playerInfo.ropeState == Player.RopeState.HOLDING && !_hookButtonClicked)
        {
            rope.ChainReset();
            _player.playerInfo.ropeState = Player.RopeState.FAILED;
            _player.ChangeState(Player.State.IDLE);

            _controller.SetXVelocity(0);
        }


    }

    private void CreateRope()
    {
        Vector2 playerPosition = (Vector2)_player.transform.position;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        //rope.chainMaxLength = Vector2.Distance(playerPosition, mousePosition);
        float ropeLength = Vector2.Distance(playerPosition, mousePosition);
        rope.CreateChain(ropeLength);
        rope.ChainConnect(playerPosition, mousePosition, ropeLength, 0.5f);
    }

    private void HoldingRope()
    {
        //    Chain.ChainNode lastRopeNode = rope.nodes[rope.chainMaxCount - 1];
        //    Vector2 playerPosition = (Vector2)_player.transform.position;
        //    rope.nodes[0].position = this.transform.position;
        //    ////////velocity.y = -1 * speed;
        //    if (Vector2.Distance((Vector2)playerPosition + velocity, lastRopeNode.position) > rope.chainMaxLength)
        //    {
        //        Vector3 toOriginChainNode = ((Vector3)lastRopeNode.position - (Vector3)playerPosition);
        //        Debug.DrawRay(playerPosition, toOriginChainNode, Color.red);
        //        Vector3 playerMoveVector = Vector3.Cross(new Vector3(0, 0, -1), toOriginChainNode);
        //        if (velocity.x < 0)
        //            playerMoveVector = -playerMoveVector;
        //        else if (velocity.x == 0)
        //        {
        //            if (playerPosition.x < lastRopeNode.position.x)
        //            {//오른쪽 낙하
        //                playerMoveVector = playerMoveVector;

        //            }//왼쪽 낙하
        //            else
        //                playerMoveVector = -playerMoveVector;

        //        }
        //        Vector3 nextMovePoint = (Vector3)playerPosition + playerMoveVector * ropeSpeed * 0.1f;
        //        Vector2 nextMovePointToOriginChainNode = rope.chainMaxLength * (lastRopeNode.position - (Vector2)nextMovePoint).normalized;
        //        playerMoveVector = -nextMovePointToOriginChainNode + (Vector2)toOriginChainNode;
        //        //움직임 벡터
        //        Debug.DrawRay(playerPosition, playerMoveVector, Color.black);
        //        velocity = playerMoveVector;
        //    }
        CalculateVelocity();
        CalculateRopeSwinging();
    }
    public void CalculateRopeSwinging()
    {
        currentVelocityY += ropeGravity * Time.deltaTime;
        Vector2 velocity = new Vector2(currentVelocityX, currentVelocityY);
        Chain.ChainNode lastRopeNode = rope.nodes[rope.chainMaxCount - 1];
        Vector2 playerPosition = _player.transform.position;
        rope.nodes[0].position = this.transform.position;

        Vector3 toOriginChainNode = ((Vector3)lastRopeNode.position - (Vector3)playerPosition);
        Debug.DrawRay(playerPosition, toOriginChainNode, Color.red);
        Vector3 playerMoveVector = Vector3.Cross(new Vector3(0, 0, -1), toOriginChainNode);

        direction = RoundNormalize(currentVelocityX);
        Vector3 nextMovePoint = (Vector3)playerPosition + playerMoveVector * direction * 0.01f;
        Vector2 nextMovePointToOriginChainNode = rope.chainMaxLength * (lastRopeNode.position - (Vector2)nextMovePoint).normalized;
        playerMoveVector = -nextMovePointToOriginChainNode + (Vector2)toOriginChainNode;

        Debug.DrawRay(playerPosition, velocity, Color.yellow);
        if (Vector2.Distance((Vector2)playerPosition + velocity * Time.deltaTime, lastRopeNode.position) > rope.chainMaxLength)
        {
            float deg =  Vector2.Angle(velocity, playerMoveVector);
            if (deg <= 0.1f)
                currentVelocityY = 0;
            if (deg > 90.0f)
            {
                deg = 89.99f;
            }
            velocity = ((Vector2)playerMoveVector).normalized * velocity.magnitude * Mathf.Cos(Mathf.Deg2Rad * deg / 2);
            Debug.DrawRay(playerPosition, velocity, Color.green);
        }
        TestMove();
        this._controller.AddXVelocity(velocity.x * Time.deltaTime);
        this._controller.AddYVelocity(velocity.y * Time.deltaTime);
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
        //if (_player.playerInfo.state == Player.State.WALL_GRAB) return;
        //if (_player.playerInfo.state == Player.State.JUMPING) return;
        // add exception state Up here

        int xInputDirection = RoundNormalize(InputManager.Instance.MoveHorizontal);
        int currentXDirection = RoundNormalize(currentVelocityX);
        float targetMaxSpeed = maxMoveSpeed * penaltySpeedRope;
        bool isFasterThanMaxSpeed = Mathf.Abs(currentVelocityX) > targetMaxSpeed;
        float decelerationWhenMaxSpeed = _controller.IsOnGround ? decelerationMaxSpeedOnGround : decelerationMaxSpeedOnAir;
        float deceleration = _controller.IsOnGround ? decelerationOnGround : decelerationOnAir;
        float acceleration = _controller.IsOnGround ? accelerationOnGround : accelerationOnAir;
   
        if (currentXDirection == 1) // 현재진행방향 ->
        {
            if (xInputDirection == 1) // 입력방향 ->
            {
                if (!isFasterThanMaxSpeed)
                {
                    currentVelocityX += acceleration * Time.deltaTime;

                    if (currentVelocityX > targetMaxSpeed)
                    {
                        currentVelocityX = targetMaxSpeed;
                    }
                }
            }
            else if (xInputDirection == -1) // 입력방향 <-
            {
                currentVelocityX -= acceleration * Time.deltaTime;
            }
            else // 입력 0
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
        else if (currentXDirection == -1) // 현재진행방향 <-
        {
            if (xInputDirection == -1) // 입력방향 <-
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
            else if (xInputDirection == 1) // 입력방향 ->
            {
                currentVelocityX += acceleration * Time.deltaTime;
            }
            else // 입력 0
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


    float theta; // 초기각
    float omega = 0f; // 각속도

    void UpdateTheta(Vector3 hookedPosition, Vector3 currentPosition)
    {
        float Xdistance = hookedPosition.x - currentPosition.x;
        float Ydistance = hookedPosition.y - currentPosition.y;

        theta = Mathf.Atan2(Ydistance, Xdistance) * Mathf.Rad2Deg - 90f;
        //Debug.Log("theta" + theta);
    }

    float angularAcceleration;

    void TestVelocity()
    {
        angularAcceleration = -ropeGravity / rope.chainMaxLength * Mathf.Sin(theta);
        //Debug.Log("angularAcceleration" + angularAcceleration);
    }

    private void TestMove()
    {
        omega += angularAcceleration * Time.deltaTime;
        theta += omega * Time.deltaTime;

        _controller.AddXVelocity(angularAcceleration * Time.deltaTime);
        //Debug.Log("velocity" + _controller.controllerPhysics.velocity);
    }

}
