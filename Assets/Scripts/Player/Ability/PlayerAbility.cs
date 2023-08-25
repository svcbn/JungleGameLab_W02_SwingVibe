using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using W02;

public abstract class PlayerAbility : MonoBehaviour
{
    protected Player _player;
    protected Controller2D _controller;

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
        HandleInput();
    }

    public virtual void DoUpdate()
    {
        
    }

    public virtual void PostUpdate()
    {

    }

    protected virtual void Init()
    {
        _player = GetComponentInParent<Player>();
        _controller = GetComponent<Controller2D>();
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
