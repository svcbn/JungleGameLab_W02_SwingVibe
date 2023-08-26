using UnityEngine;

public class VirtualRopeLine : MonoBehaviour
{
    Vector2 targetPos;
    public float rayAngle = 30f;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }


    public void Draw(Vector3 pos, Vector2 to, float length)
    {
        lineRenderer.enabled = true;
        DetectObject(pos, to - (Vector2)pos, length);
        lineRenderer.SetPosition(0, new Vector3(0,0,0));
        Debug.Log(targetPos);
        lineRenderer.SetPosition(1, new Vector2(-pos.x + targetPos.x, (-pos.y + targetPos.y)/ 2));
    }

    public void Erase()
    {
        lineRenderer.enabled = false;
    }

    public void DetectObject(Vector2 pos, Vector2 dir, float length)
    {
        float[] angles = { 0f, -rayAngle, rayAngle };
        Color[] colors = { Color.black, Color.red, Color.cyan };
        for(int i = 0; i < angles.Length; i++)
        {
            if (ShootRay(pos, dir.normalized, length, angles[i], colors[i]))
                Debug.Log("hit: " + i);
        }
    }

    public bool ShootRay(Vector2 pos, Vector2 dir, float length, float angle, Color color)
    {
        Vector2 raydir = Quaternion.Euler(0f, 0f, angle) * dir;
        RaycastHit2D hit = Physics2D.Raycast(pos, raydir * length);
        Debug.DrawRay(pos, raydir * length, color);
        if (hit)
        {
            ////만약 특정오브젝트라면
            //if (hit.collider.CompareTag("Enemy"))
            //{
            //    targetPos = hit.collider.transform.position;
            //}
            ////그냥 벽면이면
            //else
            //{
            //    targetPos = hit.transform.position;
            //}
            targetPos = hit.transform.position;
            return true;
        }
        return false;
    }

}
