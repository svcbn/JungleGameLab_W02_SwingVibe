using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
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
    }

    public enum RopeState
    {
        HOOKED,
        HOLDING,
        SWINGING,
        SWINGING_AIMING,
        FAILED,
    }

    public struct Info
    {
        public State state;
        public RopeState ropeState;
        public bool aimingEnabled;
    }

    public Info playerInfo { get; private set; }

    private List<PlayerAbility> _abilities;
    private Controller2D _controller;

    private void Awake()
    {
        _abilities = new List<PlayerAbility>();
        _abilities.AddRange(gameObject.GetComponents<PlayerAbility>());
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
}
