using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditCard : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public string nextScene;

    public float fadeInDuration = 0.8f;
    public float holdDuration = 1.5f;
    public float fadeOutDuration = 0.6f;

    private float timer = 0f;
    private enum Phase { FadeIn, Hold, FadeOut }
    private Phase phase = Phase.FadeIn;

    private SpriteRenderer[] spriteRenderers;

    void Start()
    {
        spriteRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        SetAlpha(0f);
    }

    void SetAlpha(float alpha)
    {
        if (canvasGroup != null) canvasGroup.alpha = alpha;
        foreach (var sr in spriteRenderers)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        switch (phase)
        {
            case Phase.FadeIn:
                SetAlpha(Mathf.Clamp01(timer / fadeInDuration));
                if (timer >= fadeInDuration) NextPhase();
                break;

            case Phase.Hold:
                if (timer >= holdDuration) NextPhase();
                break;

            case Phase.FadeOut:
                SetAlpha(Mathf.Clamp01(1f - timer / fadeOutDuration));
                if (timer >= fadeOutDuration)
                    SceneManager.LoadScene(nextScene);
                break;
        }
    }

    void NextPhase()
    {
        phase++;
        timer = 0f;
    }
}
