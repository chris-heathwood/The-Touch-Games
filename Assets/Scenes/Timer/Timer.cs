using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Timer : MonoBehaviour
{
    // Buttons
    public UnityEngine.UI.Button menuButton;
    public UnityEngine.UI.Button resetButton;

    // Menu background
    public SpriteRenderer menuBackground;

    // Elements
    public GameObject timerElements;
    public GameObject gaugeElements;

    // Gauge
    public PowerGauge powerGauge;

    // The circle that flashes
    public SpriteRenderer circle;

    // Text — scoreText must be outside timerElements and gaugeElements so it stays visible
    public TextMeshProUGUI scoreText;

    // Tuning
    public int totalFlashes = 5;
    public float maxInterval = 3f;   // max seconds between flashes
    public float maxReactionTime = 1f; // reaction time (seconds) that ends the game

    private enum State { Waiting, Flashing, Gauge, Finished }
    private State state;

    private int flashCount = 0;
    private float intervalTimer = 0f;
    private float nextFlashAt = 0f;
    private float reactionTimer = 0f;
    private float totalScore = 0f;
    private bool runningInEditor = false;

    // Colours
    private Color blue = new(0.204f, 0.212f, 0.302f, 1);
    private Color white = new(1f, 1f, 1f, 1);
    private Color red = new(0.96f, 0.03f, 0.03f, 1);

    void Start()
    {
        if (Application.isEditor)
        {
            runningInEditor = true;
        }

        menuButton.onClick.AddListener(() => SceneManager.LoadScene("Menu"));
        resetButton.onClick.AddListener(() => ResetGame());
        ResetGame();
    }

    void ResetGame()
    {
        state = State.Waiting;
        flashCount = 0;
        intervalTimer = 0f;
        reactionTimer = 0f;
        totalScore = 0f;
        nextFlashAt = UnityEngine.Random.Range(0.5f, maxInterval);
        circle.color = blue;
        scoreText.text = "";
        menuButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);
        if (menuBackground != null) menuBackground.gameObject.SetActive(false);
        timerElements.SetActive(true);
        gaugeElements.SetActive(false);
        powerGauge.Reset();
    }

    void EndGame()
    {
        float score = totalScore * powerGauge.Power * 1000f;
        scoreText.text = Mathf.RoundToInt(score).ToString();
        menuButton.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(true);
        if (menuBackground != null) menuBackground.gameObject.SetActive(true);
        state = State.Finished;
    }

    bool TapThisFrame()
    {
        if (runningInEditor)
        {
            return Input.GetMouseButtonDown(0);
        }

        return Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began;
    }

    // Returns 0-1 based on how fast the player reacted, 1 = instant
    float ReactionScore(float reactionTime)
    {
        return 1f - reactionTime;
    }

    void Update()
    {
        if (state == State.Finished) return;

        if (state == State.Gauge)
        {
            if (TapThisFrame())
            {
                powerGauge.Stop();
                EndGame();
            }
            return;
        }

        if (TapThisFrame())
        {
            if (state == State.Waiting)
            {
                // False start — tapped before flash
                circle.color = red;
                state = State.Finished;
                scoreText.text = "0";
                menuButton.gameObject.SetActive(true);
                resetButton.gameObject.SetActive(true);
                if (menuBackground != null) menuBackground.gameObject.SetActive(true);
                return;
            }

            if (state == State.Flashing)
            {
                // Valid tap — measure reaction time
                float score = ReactionScore(reactionTimer);
                totalScore += score;
                flashCount++;
                circle.color = blue;

                if (flashCount >= totalFlashes)
                {
                    // All flashes done — show gauge
                    timerElements.SetActive(false);
                    gaugeElements.SetActive(true);
                    state = State.Gauge;
                    return;
                }

                // Queue next flash
                state = State.Waiting;
                intervalTimer = 0f;
                nextFlashAt = UnityEngine.Random.Range(0.5f, maxInterval);
            }
        }

        if (state == State.Waiting)
        {
            intervalTimer += Time.deltaTime;
            if (intervalTimer >= nextFlashAt)
            {
                circle.color = white;
                reactionTimer = 0f;
                state = State.Flashing;
            }
        }
        else if (state == State.Flashing)
        {
            reactionTimer += Time.deltaTime;

            if (reactionTimer >= maxReactionTime)
            {
                // Too slow — game over
                circle.color = red;
                scoreText.text = "0";
                menuButton.gameObject.SetActive(true);
                resetButton.gameObject.SetActive(true);
                if (menuBackground != null) menuBackground.gameObject.SetActive(true);
                state = State.Finished;
            }
        }
    }
}
