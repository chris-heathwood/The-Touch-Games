using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Beeper : MonoBehaviour
{
    // Buttons
    public UnityEngine.UI.Button menuButton;
    public UnityEngine.UI.Button resetButton;
    public UnityEngine.UI.Button leaderboardButton;

    // Menu background
    public SpriteRenderer menuBackground;

    // Spots - two spots, assigned in Inspector (0 = left, 1 = right)
    public SpriteRenderer[] spots;

    // Tuning - adjust in Inspector
    public float initialWindow = 2f;
    public float windowShrinkAmount = 0.17f;

    // Text
    public TextMeshProUGUI swipeCountText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;

    public enum State { Holding, Ready, Failed }
    public State state;

    public float pauseTimeout = 5f;

    private int currentSpot = 0;
    private int swipeCount = 0;
    private float windowDuration;
    private float windowTimer = 0;
    private float delayTimer = 0;
    private float randomDelay = 0;
    private float pauseTimer = 0f;
    private double timer = 0;
    private bool timerStarted = false;
    private bool runningInEditor = false;

    // Colours, these use floats from 0 -> 1 not standard rgb
    private Color yellow = new(0.965f, 0.792f, 0.047f, 1);
    private Color white = new(1f, 1f, 1f, 1);

    void Start()
    {
        if (Application.isEditor)
        {
            runningInEditor = true;
        }

        menuButton.onClick.AddListener(() => { Menu.returnScene = SceneManager.GetActiveScene().name; SceneManager.LoadScene("Menu"); });
        resetButton.onClick.AddListener(() => ResetGame());
        leaderboardButton?.onClick.AddListener(() => GameCenter.ShowLeaderboard(GameCenter.Beeper));
        ResetGame();
    }

    void EndGame()
    {
        spots[currentSpot].color = yellow;
        GameCenter.ReportScore(swipeCount, GameCenter.Beeper);
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(true);
            scoreText.text = swipeCount.ToString();
        }
        state = State.Failed;
        StartCoroutine(ShowButtonsDelayed());
    }

    IEnumerator ShowButtonsDelayed()
    {
        yield return new WaitForSeconds(1f);
        swipeCountText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
        menuButton.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(true);
        if (leaderboardButton != null) leaderboardButton.gameObject.SetActive(true);
        if (menuBackground != null) menuBackground.gameObject.SetActive(true);
    }

    void ResetGame()
    {
        currentSpot = 0;
        swipeCount = 0;
        windowDuration = initialWindow;
        windowTimer = 0;
        delayTimer = 0;
        pauseTimer = 0f;
        timer = 0;
        timerStarted = false;
        state = State.Holding;
        randomDelay = UnityEngine.Random.Range(1f, 3f);
        menuButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);
        if (leaderboardButton != null) leaderboardButton.gameObject.SetActive(false);
        if (menuBackground != null) menuBackground.gameObject.SetActive(false);
        if (scoreText != null) scoreText.gameObject.SetActive(false);
        swipeCountText.gameObject.SetActive(true);
        timerText.gameObject.SetActive(true);

        spots[0].color = white;
        spots[1].color = yellow;
    }

    bool HasTouch()
    {
        if (runningInEditor) return true;
        return Input.touchCount == 1;
    }

    bool IsActivelyTouching()
    {
        if (runningInEditor) return Input.GetMouseButton(0);
        return Input.touchCount == 1;
    }

    Vector2 TouchPoint()
    {
        Vector2 screenPoint = runningInEditor ? Input.mousePosition : Input.GetTouch(0).position;
        return Camera.main.ScreenToWorldPoint(screenPoint);
    }

    int OtherSpot() => currentSpot == 0 ? 1 : 0;

    void Update()
    {
        if (state == State.Failed)
        {
            return;
        }

        if (timerStarted)
        {
            timer += Time.deltaTime;

            if (IsActivelyTouching())
                pauseTimer = 0f;
            else
                pauseTimer += Time.deltaTime;

            if (pauseTimer >= pauseTimeout)
            {
                state = State.Failed;
                EndGame();
                return;
            }
        }

        bool touching = HasTouch();
        Vector2 point = touching ? TouchPoint() : Vector2.zero;

        if (state == State.Holding)
        {
            if (touching && spots[currentSpot].bounds.Contains(point))
            {
                spots[currentSpot].color = yellow; // visual feedback - holding this spot

                if (!timerStarted)
                {
                    timerStarted = true;
                }

                delayTimer += Time.deltaTime;

                if (delayTimer >= randomDelay)
                {
                    // Signal - go to the other spot
                    spots[OtherSpot()].color = white;
                    windowTimer = 0;
                    state = State.Ready;
                }
            }
            else if (timerStarted)
            {
                // Finger left the spot before signal — end game
                state = State.Failed;
                EndGame();
            }
        }
        else if (state == State.Ready)
        {
            windowTimer += Time.deltaTime;

            if (windowTimer >= windowDuration)
            {
                // Window expired
                state = State.Failed;
                EndGame();
                return;
            }

            if (touching && spots[OtherSpot()].bounds.Contains(point))
            {
                // Successful swipe
                swipeCount++;
                currentSpot = OtherSpot();
                windowDuration = Mathf.Max(0.3f, windowDuration - windowShrinkAmount);
                delayTimer = 0;
                randomDelay = UnityEngine.Random.Range(1f, 3f);

                spots[currentSpot].color = yellow; // now holding this spot
                spots[OtherSpot()].color = yellow; // other is inactive

                state = State.Holding;
            }
        }

        // Update UI
        TimeSpan timespan = TimeSpan.FromSeconds(timer);
        swipeCountText.text = swipeCount.ToString();
        timerText.text = timespan.ToString(@"mm\:ss\:fff");
    }
}
