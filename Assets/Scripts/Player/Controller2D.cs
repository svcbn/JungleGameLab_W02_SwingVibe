using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem.XR;
using System;
using Unity.VisualScripting;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
    [Serializable]
    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
        public float height;
        public float width;
    }

    [Serializable]
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public Transform aboveTransform, belowTransform, leftTransform, rightTransform;

        public void Reset()
        {
            above = below = false;
            left = right = false;
        }

        public Vector2 moveAmountOld;

    }

    [Serializable]
    public class ControllerPhysics
    {
        [Header("Variables")]
        [Header("중력 관련 설정")]
        public float gravity = -30f;
        [Range(0f, 2f)]
        public float jumpGravityScale = 1f;
        [Range(0f, 2f)]
        public float fallGravityScale = 1f;

        [Header("Physics value")]
        public Vector3 velocity = Vector3.zero;

        [Header("Detections")]
        public CollisionInfo collisions;
        [HideInInspector]
        public CollisionInfo prevCollisions;
    }

    [Serializable]
    public class ControllerSetting
    {
        public float skinWidth = .015f;
        public int horizontalRayCount = 4;
        public int verticalRayCount = 4;
        [Range(0f, 0.1f)]
        public float raycastHorizontalError = 0.05f;
        [Range(0f, 0.1f)]
        public float raycastVerticalError = 0.05f;
        [Range(0f, 0.1f)]
        public float raycastHorizontalOffset = 0.05f;
        [Range(0f, 0.1f)]
        public float raycastVerticalOffset = 0.05f;
    }

    [Header("Controller Settings")]
    [SerializeField]
    public ControllerSetting controllerSetting;

    [Header("Controller Physics")]
    [SerializeField]
    public ControllerPhysics controllerPhysics;

    public bool IsOnGround
    {
        get {
            return controllerPhysics.collisions.below;
        }
    }


    private LayerMask collisionMask;
    private float _horizontalRaySpacing;
    private float _verticalRaySpacing;
    private BoxCollider2D _collider;
    private RaycastOrigins _raycastOrigins;
    private Vector3 _originalDeltaPos;

    void Awake()
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
        collisionMask = LayerMask.GetMask("Ground");

    }

    private void Update()
    {
        UpdateController();
    }

    void UpdateController()
    {
        ApplyGravity();

        ReadyForRaycast();

        UpdateRaycastOrigins();
        controllerPhysics.collisions.Reset();

        controllerPhysics.prevCollisions = controllerPhysics.collisions;
        HorizontalCollisions(1);
        HorizontalCollisions(-1);
        VerticalCollisions(1);
        VerticalCollisions(-1);

        transform.Translate(_originalDeltaPos);

        controllerPhysics.velocity = _originalDeltaPos / Time.deltaTime;
    }

    public void Move(Vector3 velocity)
    {
        this.controllerPhysics.velocity = velocity;
    }

    public void SetXVelocity(float xVelocity)
    {
        this.controllerPhysics.velocity.x = xVelocity;
    }

    public void SetYVelocity(float yVelocity)
    {
        this.controllerPhysics.velocity.y = yVelocity;
    }

    void ApplyGravity()
    {
        bool isFalling = controllerPhysics.velocity.y < 0;
        float gravity = controllerPhysics.gravity * (isFalling ? controllerPhysics.fallGravityScale : controllerPhysics.jumpGravityScale);
        controllerPhysics.velocity.y += gravity * Time.deltaTime;
    }

    public void Move(Vector2 moveAmount, float minHeight = 0f, float maxHeight = 0f)
    {
        this.UpdateRaycastOrigins();
        this.controllerPhysics.collisions.Reset();
        this.controllerPhysics.collisions.moveAmountOld = moveAmount;

        bool below = this.controllerPhysics.collisions.below;
        transform.Translate(moveAmount);
        Debug.Log("x: " + moveAmount.x + "y: " + moveAmount.y);
    }

    void ReadyForRaycast()
    {
        _originalDeltaPos = controllerPhysics.velocity * Time.deltaTime;
    }

    void HorizontalCollisions(float directionX)
    {
        float rayLength = Mathf.Abs(controllerPhysics.velocity.x * Time.deltaTime) + _raycastOrigins.width / 2 + controllerSetting.raycastHorizontalOffset * 2;
        Vector2 rayOriginBottom = (_raycastOrigins.bottomLeft + _raycastOrigins.bottomRight) / 2 + ((Vector2)transform.up * controllerSetting.raycastHorizontalError);
        Vector2 rayOriginTop = (_raycastOrigins.topLeft + _raycastOrigins.topRight) / 2 - ((Vector2)transform.up * controllerSetting.raycastHorizontalError);

        for (int i = 0; i < controllerSetting.horizontalRayCount; i++)
        {
            Vector2 rayOrigin = Vector2.Lerp(rayOriginBottom, rayOriginTop, (float)i / (float)(controllerSetting.horizontalRayCount - 1));
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {
                if (Mathf.Sign(directionX) == Mathf.Sign(controllerPhysics.velocity.x))
                {
                    _originalDeltaPos.x = directionX * (hit.distance - _raycastOrigins.width / 2 - controllerSetting.raycastHorizontalOffset * 2);
                }


                if (directionX == -1)
                {
                    controllerPhysics.collisions.left = true;
                    controllerPhysics.collisions.leftTransform = hit.transform;
                } else
                {
                    controllerPhysics.collisions.right = true;
                    controllerPhysics.collisions.rightTransform = hit.transform;
                }
            } else
            {
                if (directionX == -1)
                {
                    controllerPhysics.collisions.left = false;
                    controllerPhysics.collisions.leftTransform = null;
                }
                else
                {
                    controllerPhysics.collisions.right = false;
                    controllerPhysics.collisions.rightTransform = null;
                }
            }

            Platform targetPlatform = controllerPhysics.collisions.leftTransform ? controllerPhysics.collisions.leftTransform.GetComponent<Platform>() : null;
            CallPlatformCollisionCallback(controllerPhysics.prevCollisions.left, controllerPhysics.collisions.left, targetPlatform);
            targetPlatform = controllerPhysics.collisions.rightTransform ? controllerPhysics.collisions.rightTransform.GetComponent<Platform>() : null;
            CallPlatformCollisionCallback(controllerPhysics.prevCollisions.right, controllerPhysics.collisions.right, targetPlatform);
        }
    }

    void VerticalCollisions(float directionY)
    {
        float rayLength = Mathf.Abs(_originalDeltaPos.y) + _raycastOrigins.height / 2 + controllerSetting.raycastVerticalError;
        Vector2 rayOriginLeft = (_raycastOrigins.bottomLeft + _raycastOrigins.topLeft) / 2 + (Vector2)transform.up * controllerSetting.raycastVerticalError + (Vector2)transform.right * _originalDeltaPos.x;
        Vector2 rayOriginRight = (_raycastOrigins.bottomRight + _raycastOrigins.topRight) / 2 + (Vector2)transform.up * controllerSetting.raycastVerticalError + (Vector2)transform.right * _originalDeltaPos.x;

        for (int i = 0; i < controllerSetting.verticalRayCount; i++)
        {
            Vector2 rayOrigin = Vector2.Lerp(rayOriginLeft, rayOriginRight, (float)i / (float)(controllerSetting.verticalRayCount - 1));
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
                if (!(controllerPhysics.velocity.y > 0 && directionY == -1))
                {
                    _originalDeltaPos.y = directionY * (hit.distance - _raycastOrigins.height / 2 - controllerSetting.raycastVerticalError);
                    Debug.Log(_originalDeltaPos.y);
                }
                
                rayLength = hit.distance;

                if (directionY == -1)
                {
                    controllerPhysics.collisions.below = true;
                    controllerPhysics.collisions.belowTransform = hit.transform;
                } else
                {
                    controllerPhysics.collisions.above = true;
                    controllerPhysics.collisions.aboveTransform = hit.transform;
                }
            } else
            {
                if (directionY == -1)
                {
                    controllerPhysics.collisions.below = false;
                    controllerPhysics.collisions.belowTransform = null;
                }
                else
                {
                    controllerPhysics.collisions.above = false;
                    controllerPhysics.collisions.aboveTransform = null;
                }
            }

            Platform targetPlatform = controllerPhysics.collisions.aboveTransform ? controllerPhysics.collisions.aboveTransform.GetComponent<Platform>() : null;
            CallPlatformCollisionCallback(controllerPhysics.prevCollisions.above, controllerPhysics.collisions.above, targetPlatform);
            targetPlatform = controllerPhysics.collisions.belowTransform ? controllerPhysics.collisions.belowTransform.GetComponent<Platform>() : null;
            CallPlatformCollisionCallback(controllerPhysics.prevCollisions.below, controllerPhysics.collisions.below, targetPlatform);
        }
    }

    void CallPlatformCollisionCallback(bool prevCollision, bool collision, Platform targetPlatform)
    {
        if (prevCollision != collision)
        {
            if (collision)
            {
                if (targetPlatform != null)
                {
                    targetPlatform.OnPlayerHit(GetComponent<Player>());
                }
            }
            else
            {
                if (targetPlatform != null)
                {
                    targetPlatform.OnPlayerExit(GetComponent<Player>());
                }
            }
        }
    }

    void UpdateRaycastOrigins()
    {
        Bounds bounds = _collider.bounds;
        bounds.Expand(controllerSetting.skinWidth * -2);

        _raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        _raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        _raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        _raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        _raycastOrigins.height = _raycastOrigins.topRight.y - _raycastOrigins.bottomRight.y;
        _raycastOrigins.width = _raycastOrigins.topRight.x - _raycastOrigins.topLeft.x;
    }

    void CalculateRaySpacing()
    {
        Bounds bounds = _collider.bounds;
        bounds.Expand(controllerSetting.skinWidth * -2);

        controllerSetting.horizontalRayCount = Mathf.Clamp(controllerSetting.horizontalRayCount, 2, int.MaxValue);
        controllerSetting.verticalRayCount = Mathf.Clamp(controllerSetting.verticalRayCount, 2, int.MaxValue);

        _horizontalRaySpacing = bounds.size.y - 2 * controllerSetting.raycastHorizontalError / (controllerSetting.horizontalRayCount - 1);
        _verticalRaySpacing = bounds.size.x / (controllerSetting.verticalRayCount - 1);
    }
}