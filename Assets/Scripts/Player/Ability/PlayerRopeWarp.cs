using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRopeWarp : PlayerAbility
{
    public GameObject afterImg;
    float warpTime = 0.1f;
    bool isCoolDown = false;
    RopeChain rope;

    public GameObject wing1;
    public GameObject wing2;

    SpriteRenderer r1;
    SpriteRenderer r2;

    Color cooldownColor;
    Color chargedColor;

    private void Start()
    {
        this.afterImg = (GameObject)Resources.Load("AfterImage");
        rope = this.gameObject.GetComponent<PlayerRopeSwing>().ropeChain;
        r1 = wing1.GetComponent<SpriteRenderer>();
        r2 = wing2.GetComponent<SpriteRenderer>();

        cooldownColor = new Color(1f, 0.25f, 0.3f, 0.5f);
        chargedColor = new Color(0.49f, 0.92f, 0.93f, 1f);

        r1.color = chargedColor;
        r2.color = chargedColor;
    }

    protected override void HandleInput()
    {
        Vector2 targetPosition = rope.TargetPosition;

        // when jump button pressed && state == rope state
        if (_jumpButtonClicked && _player.playerInfo.state == Player.State.ROPE)
        {
            if (!isCoolDown)
            {
                _player.ChangeState(Player.State.IDLE);
                StartCoroutine(CalculateLerpDistance(targetPosition, this.transform.position));
                this.Warp(targetPosition);
                isCoolDown = true;
            }
        }
    }

    /// <summary>
    /// when warp-able object got detected, 
    /// </summary>
    /// <returns></returns>
    

    void Warp(Vector2 _targetPos)
    {
        _player.ChangeState(Player.State.WALKING);
        rope.CancelRope();
        rope.CanCreate = false;
        _hookButtonClicked = false;

        Vector2 targetDir = _targetPos - (Vector2)this.transform.position;

        this.transform.position = _targetPos - 0.1f * targetDir;

        _input.Vibration(0.2f, 0.8f);
    }

    IEnumerator CalculateLerpDistance(Vector2 _targetPos, Vector2 _currentPos)
    {
        float currentTime = 0;
        int count = 0;
        r1.color = cooldownColor;
        r2.color = cooldownColor;

        while (warpTime > currentTime)
        {
            _currentPos = Vector2.Lerp(_currentPos, _targetPos, 0.3f);
            //Debug.Log(_currentPos.x + ", " + _currentPos.y);
            if(count < 5)
            {
                Instantiate(afterImg, _currentPos, Quaternion.identity);
                count++;
            }

            currentTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        isCoolDown = false;

        r1.color = chargedColor;
        r2.color = chargedColor;
    }
}

