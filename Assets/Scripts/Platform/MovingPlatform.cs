using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform desPos;
    public Transform startPos; // 시작지점
    public Transform endPos; // 끝지점    

    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = startPos.position;
        desPos = endPos;
    }

   

    private void FixedUpdate()
    {
        transform.position = Vector2.MoveTowards(transform.position, desPos.position, Time.deltaTime * speed);

        if(Vector2.Distance(transform.position, desPos.position) <= 0.05f)
        {
            if (desPos == endPos) desPos = startPos;
            else desPos = endPos;
        }
    }
}
