using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRopeWarp : PlayerAbility
{
    
    protected override void HandleInput()
    {
        Vector2 targetPosition = this.SetTargetPos();

        // when jump button pressed && state == rope state
        if (_jumpButtonClicked)
        {
            this.Warp(targetPosition);
        }
    }

    /// <summary>
    /// when warp-able object got detected, 
    /// </summary>
    /// <returns></returns>
    Vector2 SetTargetPos()
    {
        

        return new Vector2(-5, 5);
    }
    void Warp(Vector2 _targetPos)
    {
        this.transform.position = _targetPos;
    }
}

