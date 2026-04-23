using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Beeper : MonoBehaviour
{
    // Buttons
    public UnityEngine.UI.Button menuButton;
    public UnityEngine.UI.Button resetButton;

    // Spots - two spots, assigned in Inspector (0 = left, 1 = right)
    public SpriteRenderer[] spots;

    // Tuning - adjust in Inspector
    public float initialWindow = 2f;
    public float windowShrinkAmount = 0.05f;

    // Text
    public TextMeshProUGUI swipeCountText;
    public TextMeshProUGUI timerText;

    public enum State { Holding, Ready, Failed }
    public State state;

    private int currentSpot = 0;
    private int swipeCount = 0;
    private float windowDuration;
    private float windowTimer = 0;
    private float delayTimer = 0;
    private float randomDelay = 0;
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

        menuButton.onClick.AddListener(() => SceneManager.LoadScene("Menu"));
        resetButton.onClick.AddListener(() => ResetGame());
        ResetGame();
    }

    void EndGame()
    {
        spots[currentSpot].color = yellow;
        menuButton.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(true);
    }

    void ResetGame()
    {
        currentSpot = 0;
        swipeCount = 0;
        windowDuration = initialWindow;
        windowTimer = 0;
        delayTimer = 0;
        timer = 0;
        timerStarted = false;
        state = State.Holding;
        randomDelay = UnityEngine.Random.Range(1f, 3f);
        menuButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);

        spots[0].color = white;
        spots[1].color = yellow;
    }

    bool HasTouch()
    {
        if (runningInEditor)
        {
            return true;
        }

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
        }

        Vector2 point = TouchPoint();
        bool touching = HasTouch();

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
            else
            {
                // Finger left the spot before signal — reset
                spots[currentSpot].color = white;
                delayTimer = 0;
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
