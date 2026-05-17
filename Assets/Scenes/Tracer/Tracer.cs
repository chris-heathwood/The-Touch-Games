using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Tracer : MonoBehaviour
{
    // Menu/Reset buttons (UI)
    public UnityEngine.UI.Button menuButton;
    public UnityEngine.UI.Button resetButton;
    public UnityEngine.UI.Button leaderboardButton;

    // Menu background
    public SpriteRenderer menuBackground;

    // Shooter sprite buttons
    public SpriteRenderer steadySprite;
    public SpriteRenderer shootSprite;

    // Elements
    public GameObject tracerElements;
    public GameObject shooterElements;

    // Tracer
    public LineRenderer trackLine;
    public Transform fingerMarker;
    public float pathThreshold = 1.0f;
    public float gateRadius = 0.8f;     // how close to a gate to trigger it
    public int lapsRequired = 5;

    // Shooter
    public Transform[] violetCircles;
    public Transform whiteCircle;
    public SpriteRenderer steadyGaugeFill;
    public float orbitRadius = 1f;
    public float targetScaleMultiplier = 0.5f;
    public float orbitSpeed = 90f;
    public float jitterSpeed = 45f;
    public float steadyDrain = 0.3f;
    public float jitterReduction = 0.8f;

    // Text
    public TextMeshProUGUI lapCountText;
    public TextMeshProUGUI scoreText;

    private enum State { Tracing, Shooting, Finished }
    private State state;

    // Path
    private List<Vector2> pathPoints = new();
    private float scaleX = 6f;
    private float scaleY = 3f;

    // Gates — player must hit in order: left → centerA → right → centerB → lap
    // centerA and centerB are the same position but approached from opposite sides
    private Vector2[] gates;
    private int nextGate = 0;
    private int direction = 0;          // 0 = undecided, 1 = clockwise, -1 = counter

    public float pauseTimeout = 5f;

    // Tracer state
    private double tracerTimer = 0;
    private float pauseTimer = 0f;
    private bool tracingStarted = false;
    private int lapsCompleted = 0;

    // Shooter state
    private int currentTarget = 0;
    private Vector2 aimPosition;
    private Vector2 aimVelocity;
    private float steadyGauge = 1f;
    private bool isSteadying = false;
    private float shooterScore = 0f;
    private float tracerScore = 0f;

    private bool runningInEditor = false;

    private Vector3[] violetOriginalScales;
    private Vector3 gaugeOriginalLocalScale;
    private Vector3 gaugeOriginalLocalPos;

    void Start()
    {
        if (Application.isEditor) runningInEditor = true;

        menuButton.onClick.AddListener(() => { Menu.returnScene = SceneManager.GetActiveScene().name; SceneManager.LoadScene("Menu"); });
        resetButton.onClick.AddListener(() => ResetGame());
        if (leaderboardButton != null) leaderboardButton.onClick.AddListener(() => GameCenter.ShowLeaderboard(GameCenter.Tracer));

        gaugeOriginalLocalScale = steadyGaugeFill.transform.localScale;
        gaugeOriginalLocalPos = steadyGaugeFill.transform.localPosition;

        violetOriginalScales = new Vector3[violetCircles.Length];
        for (int i = 0; i < violetCircles.Length; i++)
            violetOriginalScales[i] = violetCircles[i].localScale;

        GenerateFigureOfEightPath();
        ResetGame();
    }

    void GenerateFigureOfEightPath()
    {
        pathPoints.Clear();
        int pointCount = 200;
        int overlapCount = 6; // extra points past the full cycle to hide the seam

        for (int i = 0; i <= pointCount + overlapCount; i++)
        {
            float t = (i / (float)pointCount) * 2f * Mathf.PI + Mathf.PI;
            float denom = 1f + Mathf.Sin(t) * Mathf.Sin(t);
            float x = scaleX * Mathf.Cos(t) / denom;
            float y = scaleY * Mathf.Sin(t) * Mathf.Cos(t) / denom;
            pathPoints.Add(new Vector2(x, y));
        }

        trackLine.positionCount = pathPoints.Count;
        trackLine.startWidth = pathThreshold * 2f;
        trackLine.endWidth = pathThreshold * 2f;
        trackLine.loop = false;
        for (int i = 0; i < pathPoints.Count; i++)
        {
            trackLine.SetPosition(i, new Vector3(pathPoints[i].x, pathPoints[i].y, 0f));
        }

        // Gates: left, center (going right), right, center (going left)
        gates = new Vector2[]
        {
            new(-scaleX, 0f),    // gate 0: left extreme — start/finish
            new(0f, 0f),         // gate 1: center first pass
            new(scaleX, 0f),     // gate 2: right extreme
            new(0f, 0f),         // gate 3: center second pass
        };

        if (fingerMarker != null)
            fingerMarker.position = new Vector3(-scaleX, 0f, 0f);
    }

    void ResetGame()
    {
        state = State.Tracing;
        tracerTimer = 0;
        pauseTimer = 0f;
        tracingStarted = false;
        lapsCompleted = 0;
        nextGate = 1;           // start at gate 1 — player begins at left (gate 0)
        direction = 0;
        currentTarget = 0;
        aimPosition = Vector2.zero;
        aimVelocity = Vector2.zero;
        steadyGauge = 1f;
        isSteadying = false;
        if (steadyGaugeFill != null)
        {
            steadyGaugeFill.transform.localScale = gaugeOriginalLocalScale;
            steadyGaugeFill.transform.localPosition = gaugeOriginalLocalPos;
        }
        shooterScore = 0f;
        tracerScore = 0f;
        lapCountText.text = "0";
        scoreText.text = "";
        menuButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);
        if (leaderboardButton != null) leaderboardButton.gameObject.SetActive(false);
        if (menuBackground != null) menuBackground.gameObject.SetActive(false);
        tracerElements.SetActive(true);
        shooterElements.SetActive(false);

        if (fingerMarker != null)
            fingerMarker.position = new Vector3(-scaleX, 0f, 0f);

        foreach (var c in violetCircles) c.gameObject.SetActive(false);
    }

    void EndGame()
    {
        float finalScore = tracerScore + shooterScore;
        scoreText.text = Mathf.RoundToInt(finalScore).ToString();
        GameCenter.ReportScore((long)finalScore, GameCenter.Tracer);
        menuButton.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(true);
        if (leaderboardButton != null) leaderboardButton.gameObject.SetActive(true);
        if (menuBackground != null) menuBackground.gameObject.SetActive(true);
        state = State.Finished;
    }

    float DistanceToPath(Vector2 point)
    {
        float minDist = float.MaxValue;
        for (int i = 0; i < pathPoints.Count; i++)
        {
            float d = Vector2.Distance(point, pathPoints[i]);
            if (d < minDist) minDist = d;
        }
        return minDist;
    }

    void Shoot()
    {
        if (state != State.Shooting) return;

        Vector2 whitePos = whiteCircle.position;
        Vector2 targetPos = violetCircles[currentTarget].position;
        float dist = Vector2.Distance(whitePos, targetPos);
        float targetRadius = violetCircles[currentTarget].localScale.x * 0.5f;

        if (dist <= targetRadius)
        {
            float accuracy = 1f - (dist / targetRadius);
            shooterScore += accuracy * 100f;
        }

        violetCircles[currentTarget].gameObject.SetActive(false);
        currentTarget++;

        if (currentTarget >= violetCircles.Length)
        {
            EndGame();
        }
        else
        {
            violetCircles[currentTarget].gameObject.SetActive(true);
            violetCircles[currentTarget].localScale = violetOriginalScales[currentTarget] * targetScaleMultiplier;
            float nextRadius = violetOriginalScales[currentTarget].x * targetScaleMultiplier * 0.5f;
            aimPosition = (Vector2)violetCircles[currentTarget].position + UnityEngine.Random.insideUnitCircle.normalized * (nextRadius + orbitRadius);
            aimVelocity = Vector2.zero;
        }
    }

    Vector2 InputPosition()
    {
        if (runningInEditor) return Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.touchCount == 0) return new Vector2(-9999f, -9999f);
        return Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
    }

    bool InputHeld()
    {
        if (runningInEditor) return Input.GetMouseButton(0);
        return Input.touchCount == 1;
    }

    bool TapThisFrame()
    {
        if (runningInEditor) return Input.GetMouseButtonDown(0);
        return Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began;
    }

    void UpdateTracer()
    {
        if (!InputHeld())
        {
            tracingStarted = false;

            // End game if player stops tracing mid-run
            if (direction != 0)
            {
                pauseTimer += Time.deltaTime;
                if (pauseTimer >= pauseTimeout)
                {
                    scoreText.text = "0";
                    menuButton.gameObject.SetActive(true);
                    resetButton.gameObject.SetActive(true);
                    state = State.Finished;
                }
            }
            return;
        }

        pauseTimer = 0f;

        Vector2 point = InputPosition();

        if (fingerMarker != null)
            fingerMarker.position = new Vector3(point.x, point.y, 0f);

        if (!tracingStarted)
        {
            // Only start if near the left gate
            if (Vector2.Distance(point, gates[0]) <= gateRadius)
            {
                tracingStarted = true;
            }
            return;
        }

        if (DistanceToPath(point) > pathThreshold)
        {
            scoreText.text = "0";
            menuButton.gameObject.SetActive(true);
            resetButton.gameObject.SetActive(true);
            if (menuBackground != null) menuBackground.gameObject.SetActive(true);
            state = State.Finished;
            return;
        }

        tracerTimer += Time.deltaTime;

        // First center gate — lock direction on first lap, enforce it on subsequent laps
        if (nextGate == 1)
        {
            if (Vector2.Distance(point, gates[1]) <= gateRadius)
            {
                if (direction == 0)
                {
                    direction = point.y > 0 ? 1 : -1;
                    nextGate = 2;
                }
                else if ((direction == 1 && point.y > 0) || (direction == -1 && point.y < 0))
                {
                    nextGate = 2;
                }
            }
        }
        else if (nextGate == 2)
        {
            if (Vector2.Distance(point, gates[2]) <= gateRadius)
                nextGate = 3;
        }
        else if (nextGate == 3)
        {
            if (Vector2.Distance(point, gates[3]) <= gateRadius)
            {
                // Must approach center from opposite side to first pass
                bool validApproach = direction == 1 ? point.y < 0 : point.y > 0;
                if (validApproach)
                    nextGate = 4;
            }
        }
        else if (nextGate == 4)
        {
            if (Vector2.Distance(point, gates[0]) <= gateRadius)
            {
                lapsCompleted++;
                lapCountText.text = lapsCompleted.ToString();
                nextGate = 1;

                if (lapsCompleted >= lapsRequired)
                {
                    tracerScore = Mathf.Max(0f, 1000f - (float)tracerTimer * 10f);
                    tracerElements.SetActive(false);
                    shooterElements.SetActive(true);
                    violetCircles[0].gameObject.SetActive(true);
                    violetCircles[0].localScale = violetOriginalScales[0] * targetScaleMultiplier;
                    float firstTargetRadius = violetOriginalScales[0].x * targetScaleMultiplier * 0.5f;
                    aimPosition = (Vector2)violetCircles[0].position + UnityEngine.Random.insideUnitCircle.normalized * (firstTargetRadius + orbitRadius);
                    aimVelocity = Vector2.zero;
                    state = State.Shooting;
                }
            }
        }
    }

    void UpdateShooter()
    {
        Vector2 point = InputPosition();

        if (TapThisFrame())
        {
            if (shootSprite.bounds.Contains(point))
            {
                Shoot();
                return;
            }
        }

        bool heldOnSteady = InputHeld() && steadySprite.bounds.Contains(point);
        isSteadying = runningInEditor
            ? heldOnSteady || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
            : heldOnSteady;

        if (isSteadying && steadyGauge > 0f)
        {
            steadyGauge = Mathf.Max(0f, steadyGauge - steadyDrain * Time.deltaTime);
        }

        Vector3 gaugeScale = gaugeOriginalLocalScale;
        gaugeScale.y = gaugeOriginalLocalScale.y * steadyGauge;
        steadyGaugeFill.transform.localScale = gaugeScale;

        // Offset position to keep one end anchored as gauge drains
        float shrink = gaugeOriginalLocalScale.y * (1f - steadyGauge);
        steadyGaugeFill.transform.localPosition = new Vector3(
            gaugeOriginalLocalPos.x,
            gaugeOriginalLocalPos.y - shrink * 0.5f,
            gaugeOriginalLocalPos.z
        );

        // Aim wanders randomly across the target; steady slows it down
        bool steadyActive = isSteadying && steadyGauge > 0f;
        float noise = steadyActive ? jitterSpeed * jitterReduction : jitterSpeed;

        // Natural velocity decay — steady kills it fast, unsteady lets it drift
        float decay = steadyActive ? 0.01f : 0.4f;
        aimVelocity *= Mathf.Pow(decay, Time.deltaTime);

        aimVelocity.x += UnityEngine.Random.Range(-noise, noise) * Time.deltaTime;
        aimVelocity.y += UnityEngine.Random.Range(-noise, noise) * Time.deltaTime;
        aimVelocity = Vector2.ClampMagnitude(aimVelocity, orbitSpeed);

        aimPosition += aimVelocity * Time.deltaTime;

        // Elastic boundary — keep aim within wander radius of the target
        Vector2 center = violetCircles[currentTarget].position;
        Vector2 drift = aimPosition - center;
        if (drift.magnitude > orbitRadius)
        {
            aimVelocity -= drift.normalized * noise * Time.deltaTime;
            aimPosition = center + drift.normalized * orbitRadius;
        }

        whiteCircle.position = new Vector3(aimPosition.x, aimPosition.y, 0f);
    }

    void Update()
    {
        if (state == State.Finished) return;

        if (state == State.Tracing) UpdateTracer();
        else if (state == State.Shooting) UpdateShooter();
    }
}
