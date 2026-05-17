using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Balancer : MonoBehaviour
{
    // Buttons
    public UnityEngine.UI.Button menuButton;
    public UnityEngine.UI.Button resetButton;

    // Menu background
    public SpriteRenderer menuBackground;

    // Visual elements — PoleRoot contains pole, pivotBall and topBall as children
    public Transform poleRoot;   // empty GameObject at bottom pivot point, this rotates
    public Transform topBall;    // the ball at the top of the pole

    // Text
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI countdownText;

    // Tuning
    public float maxAngle = 90f;           // degrees either side before game over
    public float initialDriftSpeed = 3f;   // degrees/second the pole drifts at start
    public float driftEscalation = 0.5f;   // degrees/second added to drift speed per second
    public float correctionSpeed = 60f;    // degrees/second the pivot drag can correct
    public float dragRange = 3f;           // world units the pivot can be dragged across
    public float countdownStepDuration = 0.6f;
    public float nudgeAngle = 5f;          // degrees off-balance applied when countdown ends

    private float currentAngle = 0f;
    private bool countingDown = false;
    private int countdownValue;
    private float countdownTimer;
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

    private SpriteRenderer[] poleRenderers;
    private Color[] poleOriginalColors;

    void Start()
    {
        if (Application.isEditor) runningInEditor = true;

        poleRenderers = poleRoot.GetComponentsInChildren<SpriteRenderer>();
        poleOriginalColors = new Color[poleRenderers.Length];
        for (int i = 0; i < poleRenderers.Length; i++)
            poleOriginalColors[i] = poleRenderers[i].color;

        menuButton.onClick.AddListener(() => SceneManager.LoadScene("Menu"));
        resetButton.onClick.AddListener(() => ResetGame());
        ResetGame();
    }

    void ResetGame()
    {
        currentAngle = 0f;
        currentDriftSpeed = initialDriftSpeed;
        driftDirection = UnityEngine.Random.value > 0.5f ? 1f : -1f;
        if (poleRenderers != null)
            for (int i = 0; i < poleRenderers.Length; i++)
                poleRenderers[i].color = poleOriginalColors[i];
        timer = 0;
        gameStarted = false;
        gameOver = false;
        pivotX = 0f;
        dragging = false;
        timerText.text = "00:00:000";
        scoreText.text = "";
        menuButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);
        if (menuBackground != null) menuBackground.gameObject.SetActive(false);
        countingDown = true;
        countdownValue = 3;
        countdownTimer = 0f;
        if (countdownText != null) { countdownText.gameObject.SetActive(true); countdownText.text = "3"; }
        ApplyAngle();
    }

    void EndGame()
    {
        gameOver = true;
        foreach (var r in poleRenderers) r.color = Color.red;
        scoreText.text = TimeSpan.FromSeconds(timer).ToString(@"mm\:ss\:fff");
        menuButton.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(true);
        if (menuBackground != null) menuBackground.gameObject.SetActive(true);
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

        if (countingDown)
        {
            countdownTimer += Time.deltaTime;
            if (countdownTimer >= countdownStepDuration)
            {
                countdownTimer -= countdownStepDuration;
                countdownValue--;
                if (countdownValue < 0)
                {
                    countingDown = false;
                    if (countdownText != null) countdownText.gameObject.SetActive(false);
                    currentAngle = nudgeAngle * driftDirection;
                    gameStarted = true;
                }
                else if (countdownText != null)
                {
                    countdownText.text = countdownValue.ToString();
                }
            }
            return;
        }

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
            return;
        }

        if (topBall != null)
        {
            float vx = Camera.main.WorldToViewportPoint(topBall.position).x;
            if (vx < 0f || vx > 1f)
                EndGame();
        }
    }
}
