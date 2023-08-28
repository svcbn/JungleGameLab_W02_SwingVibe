using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRopeWind : PlayerAbility
{
    public GameObject afterImg;
    float windSpeed = 10f;
    

    private void Start()
    {
        this.afterImg = (GameObject)Resources.Load("AfterImage");
    }

    protected override void HandleInput()
    {
        
        // when jump button pressed && state == rope state
        if (_jumpButtonClicked)
        {
            StartCoroutine(Wind());
        }
    }

    /// <summary>
    /// when warp-able object got detected, 
    /// </summary>
    /// <returns></returns>
    Vector2 SetTargetPos()
    {
        return Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    IEnumerator Wind(Vector2 _targetPos)
    {
        _player.ChangeState(Player.State.ROPE);

        while (true)
        {
            Vector2 dir = _targetPos - (Vector2)this.transform.position;
            this.transform.Translate(dir * windSpeed * Time.deltaTime);
            
            if(_targetPos.x == this.transform.position.x && _targetPos.y == this.transform.position.y)
            {
                break;
            }
            yield return null;
        }

        _player.ChangeState(Player.State.WALKING);
    }

   
}

