using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Rotator : MonoBehaviour
{
    // Buttons
    public UnityEngine.UI.Button menuButton;
    public UnityEngine.UI.Button resetButton;

    // Canvases
    public GameObject gaugeCanvas;
    public GameObject rotatorCanvas;

    // Gauge
    public PowerGauge powerGauge;

    // Rotator
    public Transform ball;
    public Transform trackCenter;
    public float trackRadius = 3f;

    // Markers at 10 o'clock (300 degrees) and 2 o'clock (60 degrees)
    public Transform markerLeft;
    public Transform markerRight;

    // Configure in Inspector
    public int totalRotations = 3;

    // Text
    public TextMeshProUGUI rotationCountText;
    public TextMeshProUGUI scoreText;

    // Fixed acceleration curve — same every run
    // Base speed in degrees/second for rotation 1, multiplied each rotation
    public float baseSpeed = 90f;
    public float speedIncreasePerRotation = 1.4f;

    private enum State { Gauge, Rotating, Finished }
    private State state;

    private float currentAngle = 270f; // start at bottom (6 o'clock)
    private int currentRotation = 0;
    private float currentSpeed;
    private bool runningInEditor = false;

    // Colours
    private Color green = new(0.169f, 0.965f, 0.047f, 1);

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

    void ShowCanvas(State forState)
    {
        gaugeCanvas.SetActive(forState == State.Gauge);
        rotatorCanvas.SetActive(forState == State.Rotating || forState == State.Finished);
    }

    void ResetGame()
    {
        state = State.Gauge;
        currentAngle = 270f;
        currentRotation = 0;
        currentSpeed = baseSpeed;
        menuButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);
        rotationCountText.text = "";
        scoreText.text = "";
        powerGauge.Reset();
        UpdateBallPosition();
        ShowCanvas(state);
    }

    void EndGame(float score)
    {
        scoreText.text = Mathf.RoundToInt(score).ToString();
        menuButton.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(true);
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
        if (runningInEditor)
        {
            return Input.GetMouseButtonDown(0);
        }

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
        if (state == State.Gauge)
        {
            if (TapThisFrame())
            {
                powerGauge.Stop();
                state = State.Rotating;
                currentRotation = 1;
                rotationCountText.text = currentRotation.ToString();
                ShowCanvas(state);
            }
        }
        else if (state == State.Rotating)
        {
            float previousAngle = currentAngle;
            currentAngle += currentSpeed * Time.deltaTime;

            // Check if we completed a rotation
            float rotationsDone = (currentAngle - 270f) / 360f;
            if (rotationsDone >= currentRotation)
            {
                if (currentRotation < totalRotations)
                {
                    currentRotation++;
                    currentSpeed *= speedIncreasePerRotation;
                    rotationCountText.text = currentRotation.ToString();
                }
            }

            UpdateBallPosition();

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
