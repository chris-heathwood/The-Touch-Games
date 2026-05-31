using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Rotator : MonoBehaviour
{
    // Buttons
    public UnityEngine.UI.Button menuButton;
    public UnityEngine.UI.Button resetButton;
    public UnityEngine.UI.Button leaderboardButton;

    // Menu background
    public SpriteRenderer menuBackground;

    // Root GameObjects for each phase — assign the empty parent GameObjects in Inspector
    public GameObject gaugeElements;
    public GameObject rotatorElements;

    // Gauge
    public PowerGauge powerGauge;

    // Rotator
    public Transform ball;
    public Transform trackCenter;
    public float trackRadius = 3f;

    // Text
    public TextMeshProUGUI rotationCountText;
    public TextMeshProUGUI scoreText;

    // Configure in Inspector
    public int totalRotations = 3;

    // Fixed acceleration curve — same every run
    public float baseSpeed = 120f;
    public float acceleration = 40f;

    private enum State { Countdown, HitWindow, Finished }
    private State state;

    private float currentAngle = 270f;
    private int countdownValue;
    private float currentSpeed;
    private bool runningInEditor = false;


    void Start()
    {
        if (Application.isEditor)
        {
            runningInEditor = true;
        }

        menuButton.onClick.AddListener(() => { Menu.returnScene = SceneManager.GetActiveScene().name; SceneManager.LoadScene("Menu"); });
        resetButton.onClick.AddListener(() => ResetGame());
        if (leaderboardButton != null) leaderboardButton.onClick.AddListener(() => GameCenter.ShowLeaderboard(GameCenter.Rotator));
        ResetGame();
    }

    void ResetGame()
    {
        state = State.Countdown;
        currentAngle = 270f;
        currentSpeed = baseSpeed;
        countdownValue = totalRotations;
        menuButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);
        if (leaderboardButton != null) leaderboardButton.gameObject.SetActive(false);
        if (menuBackground != null) menuBackground.gameObject.SetActive(false);
        scoreText.text = "";
        rotationCountText.text = totalRotations.ToString();
        if (gaugeElements != null) gaugeElements.SetActive(false);
        if (rotatorElements != null) rotatorElements.SetActive(true);
        UpdateBallPosition();
    }

    void EndGame(float score)
    {
        state = State.Finished;
        scoreText.text = Mathf.RoundToInt(score).ToString();
        GameCenter.ReportScore((long)score, GameCenter.Rotator);
        StartCoroutine(ShowButtonsDelayed());
    }

    IEnumerator ShowButtonsDelayed()
    {
        yield return new WaitForSeconds(1f);
        menuButton.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(true);
        if (leaderboardButton != null) leaderboardButton.gameObject.SetActive(true);
        if (menuBackground != null) menuBackground.gameObject.SetActive(true);
    }

    void UpdateBallPosition()
    {
        float rad = currentAngle * Mathf.Deg2Rad;
        ball.position = trackCenter.position + new Vector3(
            Mathf.Cos(rad) * trackRadius,
            Mathf.Sin(rad) * trackRadius,
            0f
        );
    }

    bool TapThisFrame()
    {
        if (runningInEditor) return Input.GetMouseButtonDown(0);
        return Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began;
    }

    // Returns how close the angle is to 12 o'clock (90 degrees), 0 = outside zone, 1 = perfect
    float AccuracyScore(float angle)
    {
        // Normalise to 0-360
        float a = ((angle % 360f) + 360f) % 360f;

        // Zone: 10 o'clock = 120 degrees, 2 o'clock = 60 degrees (in standard math angles)
        float zoneMin = 60f;
        float zoneMax = 120f;

        if (a < zoneMin || a > zoneMax) return 0f;

        // Distance from 12 o'clock (90 degrees) — closer = higher score
        float distanceFrom12 = Mathf.Abs(a - 90f);
        float maxDistance = 30f; // half the zone width
        return 1f - (distanceFrom12 / maxDistance);
    }

    void Update()
    {
        if (state == State.Finished) return;

        float angleBefore = currentAngle;
        currentSpeed += acceleration * Time.deltaTime;
        currentAngle -= currentSpeed * Time.deltaTime;
        UpdateBallPosition();

        if (state == State.Countdown)
        {
            // Tap before 0 = fail
            if (TapThisFrame())
            {
                EndGame(0f);
                return;
            }

            // Decrement countdown once per completed rotation (ball starts at 270°)
            int rotationsDone = Mathf.FloorToInt((270f - currentAngle) / 360f);
            int newCountdown = totalRotations - rotationsDone;
            if (newCountdown != countdownValue)
            {
                countdownValue = newCountdown;
                rotationCountText.text = Mathf.Max(0, countdownValue).ToString();
                if (countdownValue <= 0)
                    state = State.HitWindow;
            }
        }
        else if (state == State.HitWindow)
        {
            // Ball passed through gap without a tap = fail
            float prevNorm = (((-angleBefore) % 360f) + 360f) % 360f;
            float currNorm = (((-currentAngle) % 360f) + 360f) % 360f;
            bool passedGap = prevNorm < 90f && currNorm >= 90f;

            if (TapThisFrame())
            {
                float normalised = ((currentAngle % 360f) + 360f) % 360f;
                EndGame(AccuracyScore(normalised) * 1000f);
                return;
            }

            if (passedGap)
                EndGame(0f);
        }
    }
}
