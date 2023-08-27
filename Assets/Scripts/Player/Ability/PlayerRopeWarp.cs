using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRopeWarp : PlayerAbility
{
    public GameObject afterImg;
    float warpTime = 0.1f;
    bool isStart = false;

    private void Start()
    {
        this.afterImg = (GameObject)Resources.Load("AfterImage");
    }

    protected override void HandleInput()
    {
        Vector2 targetPosition = this.SetTargetPos();

        // when jump button pressed && state == rope state
        if (_jumpButtonClicked)
        {
            if (!isStart)
            {
                StartCoroutine(CalculateLerpDistance(targetPosition, this.transform.position));
                this.Warp(targetPosition);

            }
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

    void Warp(Vector2 _targetPos)
    {
        this.transform.position = _targetPos;
    }

    IEnumerator CalculateLerpDistance(Vector2 _targetPos, Vector2 _currentPos)
    {
        isStart = true;
        float currentTime = 0;
        int count = 0;

        while (warpTime > currentTime)
        {
            _currentPos = Vector2.Lerp(_currentPos, _targetPos, 0.3f);
            Debug.Log(_currentPos.x + ", " + _currentPos.y);
            if(count < 5)
            {
                Instantiate(afterImg, _currentPos, Quaternion.identity);
                count++;
            }

            currentTime += Time.deltaTime;
            yield return null;
        }
        isStart = false;
    }
}

