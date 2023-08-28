using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using W02;

public class AimLineRenderer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float maxLineLength;
    public RopeChain ropeChain;
    public float rayAngle;
    float[] angles;
    RaycastHit2D mainHit;
    Vector2 aimPosition;
    LayerMask mask;
    InputUser user;
    Vector2 aimVec;
    Vector2 targetPos;
    bool isGamepad = false;
    public GameObject aimCircle;
    public GameObject padAim;


    private void Awake()
    {
        PlayerInput input = GameObject.Find("GameManagers").GetComponent<PlayerInput>();
        mask = LayerMask.GetMask("Ground", "NotPass", "TargetableObject");
        aimCircle.SetActive(false);
    }


    private void Update()
    {
        
        if (!isGamepad)
        {
            aimPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            aimVec = (aimPosition - (Vector2)transform.position).normalized;
            padAim.transform.rotation = Quaternion.FromToRotation(Vector2.right, aimVec);
            // 마우스 위치 디버그 로그
            //Debug.Log("Mouse Position: " + aimPosition);
        }
        else
        {
            // 게임패드 조준 위치 계산            
            aimVec = (new Vector2(InputManager.Instance.AimHorizontal, InputManager.Instance.AimVertical)).normalized;
            padAim.transform.rotation = Quaternion.FromToRotation(Vector2.right, aimVec);

        }
        // 충돌 검사

        checkRayCollision();
        Debug.DrawRay(transform.position, (mainHit.point - (Vector2)transform.position).normalized * maxLineLength, Color.red);
        aimVec = (targetPos - (Vector2)transform.position).normalized;
        if (mainHit.collider != null && !mainHit.collider.CompareTag("UnableGrap"))
        {
            float length = Vector2.Distance(transform.position, targetPos);
            // 조준선 그리기 및 업데이트
            if (length > maxLineLength)
            {
                length = maxLineLength;
                DrawAimLine(aimVec, 0);
                aimCircle.SetActive(false);
                ropeChain.CanCreate = false;
            }
            else
            {
                ropeChain.TargetPosition = targetPos;
                DrawAimLine(aimVec, length);
                aimCircle.SetActive(true);
                ropeChain.CanCreate = true;
            }
        }
        else
        {
            DrawAimLine(aimVec, 0);
            aimCircle.SetActive(false);
            ropeChain.CanCreate = false;
        }
    }
    
    public bool checkGamePad()
    {
        return isGamepad;
    }

    private void OnEnable()
    {
        InputUser.onChange += OnInputDeviceChange;
    }

    private void OnDisable()
    {
        InputUser.onChange -= OnInputDeviceChange;

    }

    void OnInputDeviceChange(InputUser user, InputUserChange change, InputDevice device)
    {
        if (change == InputUserChange.ControlSchemeChanged)
        {
           UpdateDeviceType(user);
        }
    }

    void UpdateDeviceType(InputUser user)
    {
        string device = user.controlScheme.Value.name;
        if(device == "Gamepad")
        {
            isGamepad = true;
        }
        else
        {
            isGamepad = false;
        }
    }

    private void DrawAimLine(Vector2 _aimPos, float _length)
    {
        // 조준선 끝 위치 계산
        Vector3 endPos = this.transform.position + (Vector3)aimVec * _length;
        aimCircle.transform.position = endPos;

        // 조준선 그리기
        DrawLine(this.transform.position, endPos);
    }

    private void DrawLine(Vector3 startPos, Vector3 endPos)
    {
        startPos.z = -1;
        endPos.z = -1;

        lineRenderer.startWidth = 0.3f;
        lineRenderer.endWidth = 0.3f;
        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }

    private bool checkRayCollision()
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, aimVec, maxLineLength, mask);
        RaycastHit2D hitLeft = Physics2D.Raycast(this.transform.position, Quaternion.Euler(0f, 0f, rayAngle) * aimVec, maxLineLength, mask);
        RaycastHit2D hitRight = Physics2D.Raycast(this.transform.position, Quaternion.Euler(0f, 0f, -rayAngle) * aimVec, maxLineLength, mask);
        RaycastHit2D[] hits = { hit, hitLeft, hitRight };
        angles = new float[3] { 0, rayAngle, -rayAngle };
        Debug.DrawRay(transform.position, aimVec.normalized * maxLineLength);
        Debug.DrawRay(transform.position, Quaternion.Euler(0f, 0f, rayAngle) * aimVec.normalized * maxLineLength);
        Debug.DrawRay(transform.position, Quaternion.Euler(0f, 0f, -rayAngle) * aimVec.normalized * maxLineLength);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider != null && hits[i].collider.gameObject.CompareTag("Target"))
            {
                mainHit = hits[i];
                targetPos = mainHit.collider.gameObject.transform.position;
                return true;
            }
        }
        
        if (hits[0].collider == null)
        {
            if (Vector2.Distance(mainHit.point, transform.position) < maxLineLength
                 && Vector2.Angle(mainHit.point - (Vector2)transform.position, aimVec) < rayAngle)
            {
                targetPos = mainHit.point;
                return true;
            }
        }

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider != null)
            {
                aimVec = Quaternion.Euler(0f, 0f, angles[i]) * aimVec.normalized;
                mainHit = hits[i];
                targetPos = mainHit.point;
                return true;
            }
        }
        mainHit = hits[0];
        targetPos = mainHit.point;
        return false;
    }

}
