using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float smoothing = 0.2f;
    [SerializeField] Vector2 minCameraBoundary;
    [SerializeField] Vector2 maxCameraBoundary;
    public Vector3 offset; // 대상과 카메라 사이의 거리 벡터

    Vector3 velocity;

    private void LateUpdate()
    {
        //Vector3 targetPos = new Vector3(player.position.x, player.position.y, this.transform.position.z);

        /*targetPos.x = Mathf.Clamp(targetPos.x, minCameraBoundary.x, maxCameraBoundary.x);
        targetPos.y = Mathf.Clamp(targetPos.y, minCameraBoundary.y, maxCameraBoundary.y);*//*

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothing);*/

        Vector3 desiredPosition = player.position + offset; // 대상의 위치에 거리 벡터를 더하여 카메라의 원하는 위치 계산
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothing); // 현재 위치에서 원하는 위치까지 부드러운 보간

        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z); // 카메라 위치 업데이트
    }
}
