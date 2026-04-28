using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Balancer : MonoBehaviour
{
    // Buttons
    public UnityEngine.UI.Button menuButton;
    public UnityEngine.UI.Button resetButton;

    // Visual elements — PoleRoot contains pole, pivotBall and topBall as children
    public Transform poleRoot;   // empty GameObject at bottom pivot point, this rotates

    // Text
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;

    // Tuning
    public float maxAngle = 30f;           // degrees either side before game over
    public float initialDriftSpeed = 3f;   // degrees/second the pole drifts at start
    public float driftEscalation = 0.5f;   // degrees/second added to drift speed per second
    public float correctionSpeed = 60f;    // degrees/second the pivot drag can correct
    public float dragRange = 3f;           // world units the pivot can be dragged across

    private float currentAngle = 0f;
    private float currentDriftSpeed = 0f;
    private float driftDirection = 1f;
    private double timer = 0;
    private bool gameStarted = false;
    private bool gameOver = false;
    private float pivotX = 0f;
    private float dragStartX = 0f;
    private float pivotStartX = 0f;
    private bool dragging = false;
    private bool runningInEditor = false;

    // Colours
    private Color indigo = new(0.569f, 0.043f, 0.965f, 1);
    private Color white = new(1f, 1f, 1f, 1);

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
        currentAngle = 0f;
        currentDriftSpeed = initialDriftSpeed;
        driftDirection = UnityEngine.Random.value > 0.5f ? 1f : -1f;
        timer = 0;
        gameStarted = false;
        gameOver = false;
        pivotX = 0f;
        dragging = false;
        timerText.text = "00:00:000";
        scoreText.text = "";
        menuButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);
        ApplyAngle();
    }

    void EndGame()
    {
        gameOver = true;
        scoreText.text = TimeSpan.FromSeconds(timer).ToString(@"mm\:ss\:fff");
        menuButton.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(true);
    }

    void ApplyAngle()
    {
        poleRoot.localRotation = Quaternion.Euler(0f, 0f, -currentAngle);
        poleRoot.localPosition = new Vector3(pivotX, poleRoot.localPosition.y, 0f);
    }

    Vector2 InputPosition()
    {
        if (runningInEditor)
        {
            return Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        return Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
    }

    bool InputBegan()
    {
        if (runningInEditor) return Input.GetMouseButtonDown(0);
        return Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began;
    }

    bool InputHeld()
    {
        if (runningInEditor) return Input.GetMouseButton(0);
        return Input.touchCount == 1;
    }

    void Update()
    {
        if (gameOver) return;

        // Handle drag input
        if (InputBegan())
        {
            dragging = true;
            dragStartX = InputPosition().x;
            pivotStartX = pivotX;
            gameStarted = true;
        }

        if (dragging && InputHeld())
        {
            float dragDelta = InputPosition().x - dragStartX;
            pivotX = Mathf.Clamp(pivotStartX + dragDelta, -dragRange, dragRange);
        }
        else
        {
            dragging = false;
        }

        if (!gameStarted) return;

        timer += Time.deltaTime;

        // Continuous drift in one direction, escalating over time
        currentDriftSpeed += driftEscalation * Time.deltaTime;
        currentAngle += driftDirection * currentDriftSpeed * Time.deltaTime;

        // Pivot correction — offset from centre pulls angle back toward vertical
        float correction = -pivotX * (correctionSpeed * Time.deltaTime);
        currentAngle += correction;

currentAngle = Mathf.Clamp(currentAngle, -maxAngle * 2f, maxAngle * 2f);

        ApplyAngle();

        // Update timer display
        timerText.text = TimeSpan.FromSeconds(timer).ToString(@"mm\:ss\:fff");

        // Check game over
        if (Mathf.Abs(currentAngle) >= maxAngle)
        {
            EndGame();
        }
    }
}
