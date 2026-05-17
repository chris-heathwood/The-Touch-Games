using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Rotator : MonoBehaviour
{
    // Buttons
    public UnityEngine.UI.Button menuButton;
    public UnityEngine.UI.Button resetButton;

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

    // Configure in Inspector
    public int totalRotations = 3;

    // Text
    public TextMeshProUGUI rotationCountText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI countdownText;

    // Fixed acceleration curve — same every run
    public float baseSpeed = 120f;
    public float acceleration = 40f; // degrees/second added per second

    private enum State { Gauge, Countdown, Rotating, Finished }
    private State state;

    public float pauseTimeout = 5f;
    public float countdownStepDuration = 0.6f;

    private float currentAngle = 270f;
    private int countdownValue;
    private float countdownTimer; // start at bottom (6 o'clock)
    private int currentRotation = 0;
    private float currentSpeed;
    private float pauseTimer = 0f;
    private bool runningInEditor = false;

    // Colours
    private Color green = new(0.169f, 0.965f, 0.047f, 1);

    void Start()
    {
        if (Application.isEditor)
        {
            runningInEditor = true;
        }

        menuButton.onClick.AddListener(() => { Menu.returnScene = SceneManager.GetActiveScene().name; SceneManager.LoadScene("Menu"); });
        resetButton.onClick.AddListener(() => ResetGame());
        ResetGame();
    }

    void ShowElements(State forState)
    {
        gaugeElements.SetActive(forState == State.Gauge);
        rotatorElements.SetActive(forState == State.Countdown || forState == State.Rotating || forState == State.Finished);
        if (countdownText != null) countdownText.gameObject.SetActive(forState == State.Countdown);
    }

    void ResetGame()
    {
        state = State.Gauge;
        currentAngle = 270f;
        currentRotation = 0;
        currentSpeed = baseSpeed;
        pauseTimer = 0f;
        countdownValue = 3;
        countdownTimer = 0f;
        menuButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);
        if (menuBackground != null) menuBackground.gameObject.SetActive(false);
        rotationCountText.text = "";
        scoreText.text = "";
        powerGauge.Reset();
        UpdateBallPosition();
        ShowElements(state);
    }

    void EndGame(float score)
    {
        scoreText.text = Mathf.RoundToInt(score).ToString();
        menuButton.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(true);
        if (menuBackground != null) menuBackground.gameObject.SetActive(true);
        state = State.Finished;
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

    bool IsActivelyTouching()
    {
        if (runningInEditor) return Input.GetMouseButton(0);
        return Input.touchCount == 1;
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
        if (state == State.Gauge)
        {
            if (TapThisFrame())
            {
                powerGauge.Stop();
                state = State.Countdown;
                countdownValue = 3;
                countdownTimer = 0f;
                if (countdownText != null) countdownText.text = "3";
                ShowElements(state);
            }
        }
        else if (state == State.Countdown)
        {
            countdownTimer += Time.deltaTime;
            if (countdownTimer >= countdownStepDuration)
            {
                countdownTimer -= countdownStepDuration;
                countdownValue--;
                if (countdownValue < 0)
                {
                    state = State.Rotating;
                    currentRotation = 1;
                    rotationCountText.text = currentRotation.ToString();
                    ShowElements(state);
                }
                else if (countdownText != null)
                {
                    countdownText.text = countdownValue.ToString();
                }
            }
        }
        else if (state == State.Rotating)
        {
            if (IsActivelyTouching())
                pauseTimer = 0f;
            else
                pauseTimer += Time.deltaTime;

            if (pauseTimer >= pauseTimeout)
            {
                EndGame(0f);
                return;
            }

            currentSpeed += acceleration * Time.deltaTime;
            currentAngle -= currentSpeed * Time.deltaTime;

            // Check if we completed a rotation
            float rotationsDone = (270f - currentAngle) / 360f;
            if (rotationsDone >= currentRotation)
            {
                if (currentRotation < totalRotations)
                {
                    currentRotation++;
                    rotationCountText.text = currentRotation.ToString();
                }
            }

            UpdateBallPosition();

            // Any tap before the final rotation ends the game
            if (currentRotation < totalRotations && TapThisFrame())
            {
                EndGame(0f);
                return;
            }

            // On final rotation, end game if ball passes through zone without a tap
            if (currentRotation == totalRotations)
            {
                float prevNormalised = (((-currentAngle + currentSpeed * Time.deltaTime) % 360f) + 360f) % 360f;
                float currNormalised = (((-currentAngle) % 360f) + 360f) % 360f;
                bool passedZone = prevNormalised < 90f && currNormalised >= 90f;
                if (passedZone && !TapThisFrame())
                {
                    EndGame(0f);
                    return;
                }
            }

            // Only accept tap on the final rotation
            if (currentRotation == totalRotations && TapThisFrame())
            {
                float normalised = ((currentAngle % 360f) + 360f) % 360f;
                float accuracy = AccuracyScore(normalised);

                if (accuracy <= 0f)
                {
                    EndGame(0f);
                }
                else
                {
                    float score = powerGauge.Power * accuracy * 1000f;
                    EndGame(score);
                }
            }
        }
    }
}
