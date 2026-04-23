using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Helpers;

public class Swiper : MonoBehaviour
{
    // Buttons
    public UnityEngine.UI.Button menuButton;
    public UnityEngine.UI.Button resetButton;

    // Spots - assign in Inspector in the order the player should cycle through them
    public SpriteRenderer[] spots;

    // Configure per variant in Inspector (Sprint: 100, Middle: 400, Marathon: 40000)
    public int totalSwipes = 100;

    // Text
    public TextMeshProUGUI counterText;
    public TextMeshProUGUI timerText;

    private int counter;
    private int targetIndex = 0;
    private Vector2 previousPoint;
    private double timer = 0;
    private bool timerStarted = false;
    private bool timerFinished = false;
    private bool runningInEditor = false;

    // Colours, these use floats from 0 -> 1 not standard rgb
    private Color red = new(0.96f, 0.03f, 0.03f, 1);
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
        menuButton.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(true);
        timerFinished = true;
    }

    void ResetGame()
    {
        counter = totalSwipes;
        targetIndex = 0;
        timer = 0;
        timerStarted = false;
        timerFinished = false;
        menuButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);

        for (int i = 0; i < spots.Length; i++)
        {
            spots[i].color = i == 0 ? white : red;
        }
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

    void Update()
    {
        if (timerFinished)
        {
            return;
        }

        Vector2 point = TouchPoint();

        if (!HasTouch())
        {
            return;
        }

        if (spots[targetIndex].bounds.Contains(point))
        {
            spots[targetIndex].color = red;
            targetIndex = (targetIndex + 1) % spots.Length;
            spots[targetIndex].color = white;
            counter--;

            if (!timerStarted)
            {
                timerStarted = true;
            }
        }

        // Timer
        if (timerStarted)
        {
            if (counter > 0)
            {
                timer += Time.deltaTime;
            }
            else if (counter == 0)
            {
                int previousIndex = (targetIndex - 1 + spots.Length) % spots.Length;
                SpriteRenderer finalSpot = spots[previousIndex];

                timer += Timing.CalculateFinalDelta(finalSpot.bounds, previousPoint, point, Time.deltaTime);

                EndGame();
            }
        }

        // Update UI
        TimeSpan timespan = TimeSpan.FromSeconds(timer);
        counterText.text = counter.ToString();
        timerText.text = timespan.ToString(@"mm\:ss\:fff");
        previousPoint = point;
    }
}
