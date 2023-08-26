using UnityEngine;

public class VirtualRopeLine : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }


    public void Draw(Vector3 pos, Vector2 to)
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, new Vector3(0,0,0));
        lineRenderer.SetPosition(1, new Vector2(-pos.x + to.x, (-pos.y + to.y)/ 2));
    }

    public void Erase()
    {
        lineRenderer.enabled = false;
    }
}
