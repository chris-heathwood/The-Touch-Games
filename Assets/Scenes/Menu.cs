using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public UnityEngine.UI.Button swiperSprintButton;
    public UnityEngine.UI.Button swiperMiddleButton;
    public UnityEngine.UI.Button swiperMarathonButton;
    public UnityEngine.UI.Button tapperButton;
    public UnityEngine.UI.Button beeperButton;
    public UnityEngine.UI.Button rotatorButton;
    public UnityEngine.UI.Button timerButton;
    public UnityEngine.UI.Button balancerButton;
    public UnityEngine.UI.Button tracerButton;

    public CanvasGroup fadeOverlay;
    public float fadeInDuration = 0.5f;

    private float fadeTimer = 0f;

    void Start()
    {
        swiperSprintButton.onClick.AddListener(() => SceneManager.LoadScene("Swiper"));
        swiperMiddleButton.onClick.AddListener(() => SceneManager.LoadScene("SwiperMiddle"));
        swiperMarathonButton.onClick.AddListener(() => SceneManager.LoadScene("SwiperMarathon"));
        tapperButton.onClick.AddListener(() => SceneManager.LoadScene("Tapper"));
        beeperButton.onClick.AddListener(() => SceneManager.LoadScene("Beeper"));
        rotatorButton.onClick.AddListener(() => SceneManager.LoadScene("Rotator"));
        timerButton.onClick.AddListener(() => SceneManager.LoadScene("Timer"));
        balancerButton.onClick.AddListener(() => SceneManager.LoadScene("Balancer"));
        tracerButton.onClick.AddListener(() => SceneManager.LoadScene("Tracer"));

        if (fadeOverlay != null) fadeOverlay.alpha = 1f;
    }

    void Update()
    {
        if (fadeOverlay == null || fadeOverlay.alpha <= 0f) return;

        fadeTimer += Time.deltaTime;
        fadeOverlay.alpha = Mathf.Clamp01(1f - fadeTimer / fadeInDuration);
    }
}
