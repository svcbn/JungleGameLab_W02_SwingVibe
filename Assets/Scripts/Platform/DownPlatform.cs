using UnityEngine;

public class DownPlatform : Platform
{
    public Transform desPos;
    public Transform startPos; // 시작지점
    public Transform endPos; // 끝지점    

    public float upSpeed;
    public float downSpeed;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = startPos.position;
        desPos = endPos;
    }

    
    private void FixedUpdate()
    {
       

        if (Vector2.Distance(transform.position, desPos.position) <= 0.05f)
        {
            if (desPos == endPos) desPos = startPos;
            else desPos = endPos;
        }
    }

    public override void OnPlayerHit(Player player)
    {
        base.OnPlayerHit(player);
        transform.position = Vector2.MoveTowards(transform.position, desPos.position, Time.deltaTime * downSpeed);        
    }

    public override void OnPlayerExit(Player player)
    {
        base.OnPlayerExit(player);
        transform.position = Vector2.MoveTowards(transform.position, desPos.position, Time.deltaTime * upSpeed);
    }
}
