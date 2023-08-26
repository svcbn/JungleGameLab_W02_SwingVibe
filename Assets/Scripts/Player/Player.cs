using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public enum State
    {
        IDLE,
        WALKING,
        WALL_GRAB,
        JUMPING,
        FALLING,
        DIZZY,
        ROPE,
    }

    public enum RopeState
    {
        HOOKED,
        HOLDING,
        SWINGING,
        SWINGING_AIMING,
        FAILED,
    }

    public class Info
    {
        public State state;
        public RopeState ropeState;
        public bool aimingEnabled;
        public bool isGrounded;
        public bool isFalling;
        public bool isJumping;
    }

    public Info playerInfo { get; private set; }

    private List<PlayerAbility> _abilities;
    private Controller2D _controller;

    private void Awake()
    {
        _abilities = new List<PlayerAbility>();
        _abilities.AddRange(gameObject.GetComponents<PlayerAbility>());

        playerInfo = new Info();
        ChangeState(State.IDLE);
    }

    void Update()
    {
        PreUpdatePlayer();

        DoUpdatePlayer();

        PostUpdatePlayer();
    }

    private void PreUpdatePlayer()
    {
        // Pre Update
        foreach (var ability in _abilities)
        {
            ability.PreUpdate();
        }
    } 

    private void DoUpdatePlayer()
    {
        foreach (var ability in _abilities)
        {
            ability.DoUpdate();
        }
    }

    private void PostUpdatePlayer()
    {
        foreach (var ability in _abilities)
        {
            ability.PostUpdate();
        }
    }

    private void CalculateAimVector()
    {
        if (playerInfo.aimingEnabled)
        {

        }
    }

    /// <summary>
    /// Change Player's State
    /// </summary>
    /// <param name="_state"></param>
    public void ChangeState(State _state)
    {
        playerInfo.state = _state;
        switch (_state)
        {
            case State.IDLE:
                // put bool here

                break;
            case State.JUMPING:
                // put bool here

                break;
            case State.WALKING:
                // put bool here

                break;
            case State.WALL_GRAB:
                // put bool here

                break;
            case State.DIZZY:
                // put bool here

                break;
        }
    }
}