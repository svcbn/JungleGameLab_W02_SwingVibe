using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using W02;

public abstract class PlayerAbility : MonoBehaviour
{
    protected Player _player;
    protected Controller2D _controller;
    protected InputManager _input;

    protected float _horizontalMove;
    protected float _verticalMove;
    protected float _horizontalAim;
    protected float _verticalAim;

    protected bool _jumpButtonClicked;
    protected bool _hookButtonClicked;
    protected bool _dashButtonClicked;

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
        _input = InputManager.Instance;
    }

    protected void PreHandleInput()
    {
        _horizontalMove = InputManager.Instance.MoveHorizontal;
        _verticalMove = InputManager.Instance.MoveVertical;

        _horizontalAim = InputManager.Instance.AimHorizontal;
        _verticalAim = InputManager.Instance.AimVertical;

        _jumpButtonClicked = InputManager.Instance.JumpButton;
        _hookButtonClicked = InputManager.Instance.HookButton;
        _dashButtonClicked = InputManager.Instance.DashButton;
    }

    protected abstract void HandleInput();
}
