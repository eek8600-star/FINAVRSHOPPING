using UnityEngine;

public class SmoothBlink : MonoBehaviour
{
    public float speed = 1.0f; // 闪烁速度
    private Material mat;
    private Color originalColor;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        mat = renderer.material;
        originalColor = mat.color;
    }

    void Update()
    {
        // 让 alpha 在 0.2~1 之间平滑摆动
        float alpha = Mathf.Lerp(0.2f, 1f, (Mathf.Sin(Time.time * speed) + 1) / 2);
        Color newColor = originalColor;
        newColor.a = alpha;
        mat.color = newColor;
    }
}
