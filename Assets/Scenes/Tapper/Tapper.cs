using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Tapper : MonoBehaviour
{
    // Buttons
    public UnityEngine.UI.Button menuButton;
    public UnityEngine.UI.Button resetButton;

    // Spots - assign in Inspector in the order the player should cycle through them
    public SpriteRenderer[] spots;

    // Configure per variant in Inspector
    public int totalTaps = 100;

    // Text
    public TextMeshProUGUI counterText;
    public TextMeshProUGUI timerText;

    private int counter;
    private int targetIndex = 0;
    private double timer = 0;
    private bool timerStarted = false;
    private bool timerFinished = false;
    private bool runningInEditor = false;

    // Colours, these use floats from 0 -> 1 not standard rgb
    private Color orange = new(0.965f, 0.447f, 0.043f, 1);
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
        counter = totalTaps;
        targetIndex = 0;
        timer = 0;
        timerStarted = false;
        timerFinished = false;
        menuButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);

        for (int i = 0; i < spots.Length; i++)
        {
            spots[i].color = i == 0 ? white : orange;
        }
    }

    bool TapBegan()
    {
        if (runningInEditor)
        {
            return Input.GetMouseButtonDown(0);
        }

        return Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began;
    }

    void Update()
    {
        if (timerFinished)
        {
            return;
        }

        Vector2 mousePosition = Input.mousePosition;
        Vector2 point = Camera.main.ScreenToWorldPoint(mousePosition);

        if (TapBegan() && spots[targetIndex].bounds.Contains(point))
        {
            spots[targetIndex].color = orange;
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
            timer += Time.deltaTime;

            if (counter == 0)
            {
                // Tapper has no boundary crossing to interpolate against (unlike Swiper), so the final time
                // is always a multiple of ~33ms at 30fps. Adding timer/10000 breaks the uniform rounding
                // with a small deterministic offset derived from the player's actual run time.
                timer += timer / 10000;
                EndGame();
            }
        }

        // Update UI
        TimeSpan timespan = TimeSpan.FromSeconds(timer);
        counterText.text = counter.ToString();
        timerText.text = timespan.ToString(@"mm\:ss\:fff");
    }
}
