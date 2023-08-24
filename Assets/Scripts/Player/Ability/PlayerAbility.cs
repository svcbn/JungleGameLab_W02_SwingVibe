using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using W02;

public abstract class PlayerAbility : MonoBehaviour
{
    protected Player _player;

    protected float _horizontalMove;
    protected float _verticalMove;
    protected float _horizontalAim;
    protected float _verticalAim;

    protected virtual void Awake()
    {
        Init();
    }

    public virtual void PreUpdate()
    {
        PreHandleInput();
    }

    public virtual void DoUpdate()
    {
        HandleInput();
    }

    public virtual void PostUpdate()
    {

    }

    protected virtual void Init()
    {
        _player = GetComponentInParent<Player>();
    }

    protected void PreHandleInput()
    {
        _horizontalMove = InputManager.Instance.MoveHorizontal;
        _verticalMove = InputManager.Instance.MoveVertical;

        _horizontalAim = InputManager.Instance.AimHorizontal;
        _verticalAim = InputManager.Instance.AimVertical;
    }

    protected abstract void HandleInput();
}
