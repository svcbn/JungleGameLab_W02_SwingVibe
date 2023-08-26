using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace W02
{
    public class InputManager : Utils.Singleton<InputManager>
    {
        // Vector2 Input
        public float MoveHorizontal { get; protected set; }
        public float MoveVertical { get; protected set; }
        public bool MoveButton { get; protected set; }
        public float AimHorizontal { get; protected set; }
        public float AimVertical { get; protected set; }
        public bool AimButton { get; protected set; }

        // Button Input
        public bool JumpButton { get; protected set; }
        public bool HookButton { get; protected set; }
        public bool DashButton { get; protected set; }
        
        // User callback functions
        public Action OnJumpPressed;
        public Action OnJumpReleased;
        public Action OnHookPressed;
        public Action OnHookReleased;
        public Action OnDashPressed;
        public Action OnDashReleased;

        private BaseInputAction _action;
        private InputAction _moveAction;
        private InputAction _aimAction;
        private InputAction _jumpAction;
        private InputAction _hookAction;
        private InputAction _dashAction;

        protected override void Init()
        {
            MoveHorizontal = 0;
            MoveVertical = 0;
            AimHorizontal = 0;
            AimVertical = 0;

            JumpButton = false;
            HookButton = false;

            _action = new BaseInputAction();
            _moveAction = _action.Player.Move;
            _aimAction = _action.Player.Aim;
            _jumpAction = _action.Player.Jump;
            _hookAction = _action.Player.Hook;
            _dashAction = _action.Player.Dash;

            EnableActions();
        }

        protected void EnableActions()
        {
            LinkEnableNonInteractionAction(_moveAction, OnMove, OnCancelMove);
            LinkEnableNonInteractionAction(_aimAction, OnAim, OnCancelAim);
            LinkEnableNonInteractionAction(_jumpAction, OnJump, OnCancelJump);
            LinkEnableNonInteractionAction(_hookAction, OnHook, OnCancelHook);
            LinkEnableNonInteractionAction(_dashAction, OnDash, OnCancelDash);
        }

        protected void DisableActions()
        {
            UnlinkDisableNonInteractionAciton(_moveAction, OnMove, OnCancelMove);
            UnlinkDisableNonInteractionAciton(_aimAction, OnAim, OnCancelAim);
            UnlinkDisableNonInteractionAciton(_jumpAction, OnJump, OnCancelJump);
            UnlinkDisableNonInteractionAciton(_hookAction, OnHook, OnCancelHook);
            UnlinkDisableNonInteractionAciton(_dashAction, OnDash, OnCancelDash);
        }

        // Util functions for InputManager
        protected void LinkEnableNonInteractionAction(
            InputAction action, 
            Action<InputAction.CallbackContext> callback, 
            Action<InputAction.CallbackContext> cancelCallback)
        {
            action.Enable();
            LinkNonInteractionAction(action, callback, cancelCallback);
        }

        protected void LinkNonInteractionAction(
            InputAction action, 
            Action<InputAction.CallbackContext> callback, 
            Action<InputAction.CallbackContext> cancelCallback)
        {
            action.performed -= callback;
            action.performed += callback;
            action.canceled -= cancelCallback;
            action.canceled += cancelCallback;
        }

        protected void UnlinkDisableNonInteractionAciton(
            InputAction action, 
            Action<InputAction.CallbackContext> callback, 
            Action<InputAction.CallbackContext> cancelCallback)
        {
            action.Disable();
            UnlinkNonInteractionAction(action, callback, cancelCallback);
        }

        protected void UnlinkNonInteractionAction(
            InputAction action, 
            Action<InputAction.CallbackContext> callback, 
            Action<InputAction.CallbackContext> cancelCallback)
        {
            action.performed -= callback;
            action.canceled -= cancelCallback;
        }

        // Callback functions for axis input (Vector2)
        protected void OnMove(InputAction.CallbackContext context)
        {
            MoveHorizontal = context.ReadValue<Vector2>().x;
            MoveVertical = context.ReadValue<Vector2>().y;
            if (!AimButton)
            {
                AimHorizontal = context.ReadValue<Vector2>().x;
                AimVertical = context.ReadValue<Vector2>().y;
            }

            MoveButton = true;
        }

        protected void OnCancelMove(InputAction.CallbackContext context)
        {
            MoveHorizontal = 0;
            MoveVertical = 0;
            if (!AimButton)
            {
                AimHorizontal = 0;
                AimVertical = 0;
            }
            MoveButton = false;
        }

        protected void OnAim(InputAction.CallbackContext context)
        {
            AimHorizontal = context.ReadValue<Vector2>().x;
            AimVertical = context.ReadValue<Vector2>().y;
            AimButton = true;
        }

        protected void OnCancelAim(InputAction.CallbackContext context)
        {
            AimHorizontal = 0; 
            AimVertical = 0;
            AimButton = false;
        }

        protected void OnJump(InputAction.CallbackContext context)
        {
            JumpButton = true;
            OnJumpPressed?.Invoke();
        }

        protected void OnCancelJump(InputAction.CallbackContext context)
        {
            JumpButton = false;
            OnJumpReleased?.Invoke();
        }

        protected void OnHook(InputAction.CallbackContext context)
        {
            HookButton = true;
            OnHookPressed?.Invoke();
        }

        protected void OnCancelHook(InputAction.CallbackContext context)
        {
            HookButton = false;
            OnHookReleased?.Invoke();
        }

        protected void OnDash(InputAction.CallbackContext context)
        {
            DashButton = true;
            OnDashPressed?.Invoke();
        }

        protected void OnCancelDash(InputAction.CallbackContext context)
        {
            DashButton = false;
            OnDashReleased?.Invoke();
        }
    }
}