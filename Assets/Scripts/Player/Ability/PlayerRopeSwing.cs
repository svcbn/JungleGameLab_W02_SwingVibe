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
        this._controller.CalculateRopeSwinging(ref rope, _player.transform.position);
        Debug.Log("rope velocity" + this._controller.controllerPhysics.velocity);
    }





   
}
