using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
    float alpha = 0.5f;
    public SpriteRenderer sprite;

    // Update is called once per frame
    void Update()
    {
        alpha = Mathf.Lerp(alpha, 0f, Time.deltaTime * 10);
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alpha);
        if(alpha < 0.01f)
        {
            Destroy(this.gameObject);
        }
    }
}
