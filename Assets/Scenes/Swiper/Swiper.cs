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

    // Menu background
    public SpriteRenderer menuBackground;

    // Spots - assign in Inspector in the order the player should cycle through them
    public SpriteRenderer[] spots;

    // Configure per variant in Inspector (Sprint: 100, Middle: 400, Marathon: 40000)
    public int totalSwipes = 100;

    // Text
    public TextMeshProUGUI counterText;
    public TextMeshProUGUI timerText;

    public float pauseTimeout = 5f;

    private int counter;
    private int targetIndex = 0;
    private Vector2 previousPoint;
    private bool previousPointValid = false;
    private double timer = 0;
    private float pauseTimer = 0f;
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

        menuButton.onClick.AddListener(() => { Menu.returnScene = SceneManager.GetActiveScene().name; SceneManager.LoadScene("Menu"); });
        resetButton.onClick.AddListener(() => ResetGame());
        ResetGame();
    }

    void EndGame()
    {
        menuButton.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(true);
        if (menuBackground != null) menuBackground.gameObject.SetActive(true);
        timerFinished = true;
    }

    void ResetGame()
    {
        counter = totalSwipes;
        targetIndex = 0;
        timer = 0;
        pauseTimer = 0f;
        previousPointValid = false;
        timerStarted = false;
        timerFinished = false;
        menuButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);
        if (menuBackground != null) menuBackground.gameObject.SetActive(false);

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

    void Update()
    {
        if (timerFinished)
        {
            return;
        }

        Vector2 point = TouchPoint();

        if (!HasTouch())
        {
            previousPointValid = false;
            if (timerStarted && counter > 0)
            {
                EndGame();
                return;
            }
            return;
        }

        if (previousPointValid && SwipedThrough(spots[targetIndex].bounds, previousPoint, point))
        {
            spots[targetIndex].color = red;
            targetIndex = (targetIndex + 1) % spots.Length;
            spots[targetIndex].color = white;
            counter--;

            if (!timerStarted)
                timerStarted = true;
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
        previousPointValid = true;
    }

    // True if the path from 'from' to 'to' passes through or ends inside the bounds
    bool SwipedThrough(Bounds bounds, Vector2 from, Vector2 to)
    {
        if (bounds.Contains(to)) return true;

        Vector2 dir = to - from;
        float dist = dir.magnitude;
        if (dist < 0.001f) return false;

        Ray ray = new Ray(new Vector3(from.x, from.y, 0f), new Vector3(dir.x, dir.y, 0f));
        return bounds.IntersectRay(ray, out float hitDist) && hitDist <= dist;
    }
}
