using UnityEngine;

// Reusable gauge component. Attach to a GameObject with a SpriteRenderer that
// acts as the fill bar — scale its Y axis to represent the gauge level.
public class PowerGauge : MonoBehaviour
{
    public SpriteRenderer fill;

    // In local space of the child — always 1 if fill is a child of the background
    public float gaugeHeight = 2f;

    // How fast the gauge oscillates at the top vs bottom (top is this many times faster)
    public float speedMultiplier = 4f;

    private float level = 0f;     // 0 = bottom, 1 = top
    private float direction = 1f; // 1 = rising, -1 = falling
    private bool stopped = false;

    public float Power => level;
    public bool HasStopped => stopped;

    void Update()
    {
        if (stopped) return;

        // Speed scales with level — slow at bottom, fast at top
        float speed = Mathf.Lerp(0.2f, 0.2f * speedMultiplier, level);

        level += direction * speed * Time.deltaTime;

        if (level >= 1f)
        {
            level = 1f;
            direction = -1f;
        }
        else if (level <= 0f)
        {
            level = 0f;
            direction = 1f;
        }

        // Scale the fill and move it up so the bottom stays anchored
        Vector3 scale = fill.transform.localScale;
        scale.y = level * gaugeHeight;
        fill.transform.localScale = scale;

        Vector3 pos = fill.transform.localPosition;
        pos.y = (scale.y / 2f) - (gaugeHeight / 2f);
        fill.transform.localPosition = pos;
    }

    public void Stop()
    {
        stopped = true;
    }

    public void Reset()
    {
        level = 0f;
        direction = 1f;
        stopped = false;
    }
}
