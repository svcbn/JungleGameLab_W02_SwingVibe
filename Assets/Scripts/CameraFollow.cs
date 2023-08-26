using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 따라갈 대상 오브젝트 (Player)의 Transform
    public float smoothSpeed = 0.125f; // 카메라 이동 스무딩 정도
    public Vector3 offset = new Vector3(0, -1, 0); // 대상과 카메라 사이의 거리 벡터 (기본적으로 아래로 1만큼 이동)

    private void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset; // 대상의 위치에 거리 벡터를 더하여 카메라의 원하는 위치 계산
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed); // 현재 위치에서 원하는 위치까지 부드러운 보간

        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z); // 카메라 위치 업데이트
    }
}
