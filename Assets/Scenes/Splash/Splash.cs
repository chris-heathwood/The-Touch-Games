using UnityEngine;
using UnityEngine.SceneManagement;

public class Splash : MonoBehaviour
{
    public SpriteRenderer thumbRed;
    public SpriteRenderer thumbGreen;
    public SpriteRenderer thumbBlue;

    public float orbitRadius = 1.5f;
    public float orbitSpeed = 80f;
    public float spinSpeed = -50f;

    public float fadeInDuration = 1.5f;
    public float swirlDuration = 2.5f;
    public float convergeDuration = 1f;
    public float pulseDuration = 0.8f;
    public float holdDuration = 1f;
    public float fadeOutDuration = 0.6f;

    private enum Phase { FadeIn, Swirl, Converge, Pulse, Hold, FadeOut, Done }
    private Phase phase = Phase.FadeIn;
    private float phaseTimer = 0f;
    private float angle = 0f;
    private float currentRadius;
    private Quaternion[] startRotations;

    private Color red   = new(0.96f, 0.03f, 0.03f, 1f);
    private Color green = new(0.169f, 0.965f, 0.047f, 1f);
    private Color blue  = new(0.043f, 0.133f, 0.965f, 1f);
    private Color white = new(1f, 1f, 1f, 1f);

    void Start()
    {
        currentRadius = orbitRadius;
        SetAlpha(0f);
    }

    void Update()
    {
        phaseTimer += Time.deltaTime;
        angle += orbitSpeed * Time.deltaTime;

        switch (phase)
        {
            case Phase.FadeIn:
            {
                float t = phaseTimer / fadeInDuration;
                SetAlpha(Mathf.Clamp01(t));
                UpdateOrbit(currentRadius);
                if (t >= 1f) NextPhase();
                break;
            }

            case Phase.Swirl:
            {
                SetAlpha(1f);
                SetColors(red, green, blue);
                UpdateOrbit(currentRadius);
                if (phaseTimer >= swirlDuration) NextPhase();
                break;
            }

            case Phase.Converge:
            {
                float t = phaseTimer / convergeDuration;
                currentRadius = Mathf.Lerp(orbitRadius, 0f, t);
                Color r = Color.Lerp(red,   white, t);
                Color g = Color.Lerp(green, white, t);
                Color b = Color.Lerp(blue,  white, t);
                SetColors(r, g, b);
                UpdateOrbit(currentRadius);
                thumbRed.transform.rotation   = Quaternion.Slerp(startRotations[0], Quaternion.identity, t);
                thumbGreen.transform.rotation = Quaternion.Slerp(startRotations[1], Quaternion.identity, t);
                thumbBlue.transform.rotation  = Quaternion.Slerp(startRotations[2], Quaternion.identity, t);
                if (t >= 1f) NextPhase();
                break;
            }

            case Phase.Pulse:
            {
                float t = phaseTimer / pulseDuration;
                // Two quick pulses then settle
                float pulse = 1f + Mathf.Sin(t * Mathf.PI * 2f) * 0.25f * (1f - t);
                Vector3 s = Vector3.one * pulse;
                thumbRed.transform.localScale   = s;
                thumbGreen.transform.localScale = s;
                thumbBlue.transform.localScale  = s;
                SetColors(white, white, white);
                UpdateOrbit(0f);
                if (t >= 1f) NextPhase();
                break;
            }

            case Phase.Hold:
            {
                SetColors(white, white, white);
                UpdateOrbit(0f);
                if (phaseTimer >= holdDuration) NextPhase();
                break;
            }

            case Phase.FadeOut:
            {
                float t = phaseTimer / fadeOutDuration;
                float a = Mathf.Clamp01(1f - t);
                Color w = new(1f, 1f, 1f, a);
                SetColors(w, w, w);
                UpdateOrbit(0f);
                if (t >= 1f) NextPhase();
                break;
            }

            case Phase.Done:
                SceneManager.LoadScene("Menu");
                break;
        }

        if (phase != Phase.Pulse && phase != Phase.Hold && phase != Phase.FadeOut && phase != Phase.Done)
        {
            float spin = spinSpeed * Time.deltaTime;
            thumbRed.transform.Rotate(0f, 0f, spin);
            thumbGreen.transform.Rotate(0f, 0f, spin);
            thumbBlue.transform.Rotate(0f, 0f, spin);
        }
    }

    void UpdateOrbit(float radius)
    {
        float a0 = angle * Mathf.Deg2Rad;
        float a1 = (angle + 120f) * Mathf.Deg2Rad;
        float a2 = (angle + 240f) * Mathf.Deg2Rad;
        thumbRed.transform.position   = new Vector3(Mathf.Cos(a0) * radius, Mathf.Sin(a0) * radius, 0f);
        thumbGreen.transform.position = new Vector3(Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius, 0f);
        thumbBlue.transform.position  = new Vector3(Mathf.Cos(a2) * radius, Mathf.Sin(a2) * radius, 0f);
    }

    void SetColors(Color r, Color g, Color b)
    {
        thumbRed.color   = r;
        thumbGreen.color = g;
        thumbBlue.color  = b;
    }

    void SetAlpha(float a)
    {
        thumbRed.color   = new Color(red.r,   red.g,   red.b,   a);
        thumbGreen.color = new Color(green.r,  green.g, green.b, a);
        thumbBlue.color  = new Color(blue.r,   blue.g,  blue.b,  a);
    }

    void NextPhase()
    {
        phase++;
        phaseTimer = 0f;
        if (phase == Phase.Converge)
        {
            currentRadius = orbitRadius;
            startRotations = new Quaternion[]
            {
                thumbRed.transform.rotation,
                thumbGreen.transform.rotation,
                thumbBlue.transform.rotation
            };
        }
    }
}
