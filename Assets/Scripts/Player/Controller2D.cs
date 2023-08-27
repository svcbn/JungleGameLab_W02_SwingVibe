using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UIElements;
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
        public bool wasOnGround;
        public bool wasAtCeiling;
        public float faceDir;

        public Transform aboveTransform, belowTransform, leftTransform, rightTransform;
        public GameObject[] belowObjects;
        public GameObject belowObj;
        public GameObject[] aboveObjects;
        public GameObject aboveObj;

        public void Reset()
        {
            above = below = false;
            left = right = false;
        }
    }

    [Serializable]
    public class ControllerPhysics
    {
        [Header("Variables")]
        [Range(0f, 0.001f)]
        public float epsilon = 0.0001f;
        public Vector2 maxVelocity = new Vector2(100f, 100f);

        [Header("중력 관련 설정")]
        public float gravity = -30f;
        [Range(0f, 2f)]
        public float jumpGravityScale = 1f;
        [Range(0f, 2f)]
        public float fallGravityScale = 1f;

        [Header("Physics value")]
        public Vector3 velocity = Vector3.zero;
        public Vector3 externalForce = Vector3.zero;

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
        [Range(0f, 1f)]
        public float raycastStickOffset = 0.2f;
        [Range(0f, 2f)]
        public float raycastStickLength = 1;
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

    public bool WasOnGound
    {
        get
        {
            return controllerPhysics.collisions.wasOnGround;
        }
    }

    private LayerMask collisionMask;
    private float _horizontalRaySpacing;
    private float _verticalRaySpacing;
    private BoxCollider2D _collider;
    private RaycastOrigins _raycastOrigins;
    private Vector3 _deltaPos;
    private Player _player;

    void Awake()
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
        collisionMask = LayerMask.GetMask("Ground");
        _player = GetComponent<Player>();
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
        BelowCollision();
        AboveCollison();

        transform.Translate(_deltaPos, Space.Self);
        //Debug.Log("deltaPos: " + _deltaPos);
        //controllerPhysics.velocity = _deltaPos / Time.deltaTime;

        controllerPhysics.externalForce.x = 0;
        controllerPhysics.externalForce.y = 0;
    }

    public void SetVelocity(Vector3 velocity)
    {
        this.controllerPhysics.velocity = velocity;
        this.controllerPhysics.externalForce = velocity;
    }

    public void AddVelocity(Vector3 velocity)
    {
        this.controllerPhysics.velocity += velocity;
        this.controllerPhysics.externalForce += velocity;
    }


    public void SetXVelocity(float xVelocity)
    {
        this.controllerPhysics.velocity.x = xVelocity;
        this.controllerPhysics.externalForce.x = xVelocity;
    }

    public void AddXVelocity(float xVelocity)
    {
        this.controllerPhysics.velocity.x += xVelocity;
        this.controllerPhysics.externalForce.x += xVelocity;
    }

    public void SetYVelocity(float yVelocity)
    {
        this.controllerPhysics.velocity.y = yVelocity;
        this.controllerPhysics.externalForce.y = yVelocity;
    }

    public void AddYVelocity(float yVelocity)
    {
        this.controllerPhysics.velocity.y += yVelocity;
        this.controllerPhysics.externalForce.y += yVelocity;
    }

    void ApplyGravity()
    {
        bool isFalling = controllerPhysics.velocity.y < 0;
        float gravity = controllerPhysics.gravity * (isFalling ? controllerPhysics.fallGravityScale : controllerPhysics.jumpGravityScale);
        if (_player.playerInfo.state == Player.State.ROPE)
        {
            return;
        }
        controllerPhysics.velocity.y += gravity * Time.deltaTime;
    }

    void ReadyForRaycast()
    {
        _deltaPos = controllerPhysics.velocity * Time.deltaTime;
        controllerPhysics.collisions.wasOnGround = controllerPhysics.collisions.below;
        controllerPhysics.collisions.wasAtCeiling = controllerPhysics.collisions.above;
    }

    void HorizontalCollisions(float directionX)
    {
        float rayLength = Mathf.Abs(controllerPhysics.velocity.x * Time.deltaTime) + _raycastOrigins.width / 2 + controllerSetting.raycastHorizontalOffset * 2;
        Vector2 rayOriginBottom = (_raycastOrigins.bottomLeft + _raycastOrigins.bottomRight) / 2 + ((Vector2)transform.up * controllerSetting.raycastHorizontalError);
        Vector2 rayOriginTop = (_raycastOrigins.topLeft + _raycastOrigins.topRight) / 2 - ((Vector2)transform.up * controllerSetting.raycastHorizontalError);

        bool hasCollision = false;
        Transform targetTransform = null;
        for (int i = 0; i < controllerSetting.horizontalRayCount; i++)
        {
            Vector2 rayOrigin = Vector2.Lerp(rayOriginBottom, rayOriginTop, (float)i / (float)(controllerSetting.horizontalRayCount - 1));
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {
                if (Mathf.Sign(directionX) == Mathf.Sign(controllerPhysics.velocity.x))
                {
                    _deltaPos.x = directionX * (hit.distance - _raycastOrigins.width / 2 - controllerSetting.raycastHorizontalOffset * 2);
                } else
                {
                    _deltaPos.x = controllerPhysics.velocity.x * Time.deltaTime;
                }

                hasCollision = true;
                targetTransform = hit.transform;

                break;
            }
        }

        if (hasCollision)
        {
            if (directionX == -1)
            {
                controllerPhysics.collisions.left = true;
                controllerPhysics.collisions.leftTransform = targetTransform;
            }
            else
            {
                controllerPhysics.collisions.right = true;
                controllerPhysics.collisions.rightTransform = targetTransform;
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

    void BelowCollision()
    {
        RaycastHit2D[] rayHits = new RaycastHit2D[controllerSetting.verticalRayCount];
        controllerPhysics.collisions.belowObjects = new GameObject[controllerSetting.verticalRayCount];

        if (_deltaPos.y < -controllerPhysics.epsilon)
        {
            _player.playerInfo.isFalling = true;
        } else
        {
            _player.playerInfo.isFalling = false;
        }

        float rayLength = _raycastOrigins.height / 2 + controllerSetting.raycastVerticalOffset;

        if (_deltaPos.y < 0)
        {
            rayLength += Mathf.Abs(_deltaPos.y);
        }

        Vector2 rayOriginLeft = (_raycastOrigins.bottomLeft + _raycastOrigins.topLeft) / 2;
        rayOriginLeft += (Vector2)transform.up * controllerSetting.raycastVerticalOffset;
        rayOriginLeft += (Vector2)transform.right * _deltaPos.x;

        Vector2 rayOriginRight = (_raycastOrigins.bottomRight + _raycastOrigins.topRight) / 2;
        rayOriginRight += (Vector2)transform.up * controllerSetting.raycastVerticalOffset;
        rayOriginRight += (Vector2)transform.right * _deltaPos.x;

        float minDistance = float.MaxValue;
        int minDistanceIdx = 0;
        bool hasCollision = false;
        for (int i = 0; i < controllerSetting.verticalRayCount; i++)
        {
            Vector2 rayOrigin = Vector2.Lerp(rayOriginLeft, rayOriginRight, (float)i / (float)(controllerSetting.verticalRayCount - 1));

            rayHits[i] = RaycastWithDebug(rayOrigin, -transform.up, rayLength, collisionMask, Color.red);

            if (rayHits[i])
            {
                hasCollision = true;

                controllerPhysics.collisions.belowObjects[i] = rayHits[i].collider.gameObject;
                if (rayHits[i].distance < minDistance)
                {
                    minDistance = rayHits[i].distance;
                    minDistanceIdx = i;
                }
            }

            if (minDistance < controllerPhysics.epsilon)
            {
                break;
            }
        }

        if (hasCollision)
        {
            controllerPhysics.collisions.belowObj = controllerPhysics.collisions.belowObjects[minDistanceIdx];
            _player.playerInfo.isFalling = false;
            controllerPhysics.collisions.below = true;

            if (controllerPhysics.velocity.y > 0 && controllerPhysics.externalForce.y > 0)
            {
                _deltaPos.y = controllerPhysics.velocity.y * Time.deltaTime;
                controllerPhysics.collisions.below = false;
            } else
            {
                _deltaPos.y = -minDistance + _raycastOrigins.height / 2 + controllerSetting.raycastVerticalOffset;
            }

            if (!controllerPhysics.collisions.wasOnGround && controllerPhysics.velocity.y > 0)
            {
                _deltaPos.y += controllerPhysics.velocity.y * Time.deltaTime;
            }

            if (Mathf.Abs(controllerPhysics.velocity.y) < controllerPhysics.epsilon)
            {
                _deltaPos.y = 0;
            }
        } else
        {
            controllerPhysics.collisions.below = false;
        }

        StickToGround();
    }

    void StickToGround()
    {
        if (_deltaPos.y >= controllerSetting.raycastStickOffset || 
            _deltaPos.y <= -controllerSetting.raycastStickOffset || 
            _player.playerInfo.isJumping ||
            !WasOnGound || 
            controllerPhysics.externalForce.y > 0)
        {
            return;
        }

        Vector2 rayOriginLeft = (_raycastOrigins.bottomLeft + _raycastOrigins.topLeft) / 2;
        Vector2 rayOriginRight = (_raycastOrigins.bottomRight + _raycastOrigins.topRight) / 2;
        Vector2 center = (rayOriginLeft + rayOriginRight) / 2;
        float rayOriginY = rayOriginLeft.y;
        rayOriginLeft.x += _deltaPos.x;
        rayOriginRight.x += _deltaPos.x;

        // TODO: 각도 있는 땅에서 걷기 시 각도 계산 필요 - 구현 추가 필요
        /*
        RaycastHit2D leftHit, rightHit, targetHit;

        leftHit = RaycastWithDebug(rayOriginLeft, -transform.up, controllerSetting.raycastStickLength, collisionMask, Color.red);
        rightHit = RaycastWithDebug(rayOriginRight, -transform.up, controllerSetting.raycastStickLength, collisionMask, Color.red);
        
        if(leftHit)
        {
            targetHit = leftHit;
            targetVector = rayOriginLeft;
        } else if (rightHit)
        {
            targetHit = rightHit;
            targetVector = rayOriginRight;
        } else
        {
            return;
        }*/

        RaycastHit2D stickHit = BoxCastWithDebug(
            center,
            new Vector2(_raycastOrigins.width, _raycastOrigins.height),
            0,
            -transform.up,
            controllerSetting.raycastStickLength,
            collisionMask,
            Color.red);

        if (stickHit)
        {
            _deltaPos.y = -Mathf.Abs(stickHit.point.y - rayOriginY) + _raycastOrigins.height / 2;
            controllerPhysics.collisions.below = true;
        }
    }

    void AboveCollison()
    {
        RaycastHit2D[] rayHits = new RaycastHit2D[controllerSetting.verticalRayCount];
        controllerPhysics.collisions.aboveObjects = new GameObject[controllerSetting.verticalRayCount];

        float rayLength = _raycastOrigins.height / 2 + (IsOnGround ? controllerSetting.raycastVerticalOffset : _deltaPos.y);

        Vector2 rayOriginLeft = (_raycastOrigins.bottomLeft + _raycastOrigins.topLeft) / 2;
        rayOriginLeft += (Vector2)transform.right * _deltaPos.x;

        Vector2 rayOriginRight = (_raycastOrigins.bottomRight + _raycastOrigins.topRight) / 2;
        rayOriginRight += (Vector2)transform.right * _deltaPos.x;

        float minDistance = float.MaxValue;
        int minDistanceIdx = 0;
        bool hasCollision = false;
        for (int i = 0; i < controllerSetting.verticalRayCount; i++)
        {
            Vector2 rayOrigin = Vector2.Lerp(rayOriginLeft, rayOriginRight, (float)i / (float)(controllerSetting.verticalRayCount - 1));

            rayHits[i] = RaycastWithDebug(rayOrigin, transform.up, rayLength, collisionMask, new Color(0.6f, 0.3f, 0.3f, 1));

            if (rayHits[i])
            {
                hasCollision = true;

                controllerPhysics.collisions.aboveObjects[i] = rayHits[i].collider.gameObject;
                if (rayHits[i].distance < minDistance)
                {
                    minDistance = rayHits[i].distance;
                    minDistanceIdx = i;
                }
            }
        }

        if (hasCollision)
        {
            Debug.Log(controllerPhysics.collisions.aboveObjects[minDistanceIdx]);
            _deltaPos.y = minDistance - _raycastOrigins.height / 2;
            controllerPhysics.collisions.aboveObj = controllerPhysics.collisions.aboveObjects[minDistanceIdx];
            controllerPhysics.collisions.above = true;

            if (IsOnGround && _deltaPos.y < 0)
            {
                _deltaPos.y = 0;
            }

            controllerPhysics.velocity.y = 0;
            controllerPhysics.externalForce.y = 0;
        }
        else
        {
            controllerPhysics.collisions.above = false;
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

    RaycastHit2D RaycastWithDebug(Vector2 origin, Vector2 direction, float distance, int layerMask, Color debugColor, bool drawDebugLine=true)
    {
        if (drawDebugLine)
        {
            Debug.DrawRay(origin, direction * distance, debugColor);
        }
        return Physics2D.Raycast(origin, direction, distance, layerMask);
    }

    RaycastHit2D BoxCastWithDebug(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, int layerMask, Color debugColor, bool drawDebugLine = true)
    {
        RaycastHit2D boxHit = Physics2D.BoxCast(origin, size, angle, direction, distance, layerMask);

        // TODO: 박스 그리기 (일단 넘김)

        return boxHit;
    }

    void UpdateRaycastOrigins()
    {
        Bounds bounds = _collider.bounds;
        //bounds.Expand(controllerSetting.skinWidth * -2);

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