using UnityEngine;

public class ColorChange : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float colorChangeInterval = 0.05f; // 색 변화 간격 (초 단위)
    private float timeSinceLastChange = 0.0f;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        timeSinceLastChange += Time.deltaTime;

        if (timeSinceLastChange >= colorChangeInterval)
        {
            timeSinceLastChange = 0.0f;

            // 랜덤한 색 생성
            Color newColor = new Color(Random.value, Random.value, Random.value);

            // SpriteRenderer 색 변경
            spriteRenderer.color = newColor;
        }
    }
}
