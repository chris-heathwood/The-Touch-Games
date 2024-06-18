using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UIElements;
using Helpers;

public class Swiper : MonoBehaviour
{
    // Buttons
    public UnityEngine.UI.Button menuButton;
    public UnityEngine.UI.Button resetButton;

    // Spots
    public SpriteRenderer leftSpot;
    public SpriteRenderer rightSpot;

    // Text
    public TextMeshProUGUI counterText;
    public TextMeshProUGUI timerText;

    private int counter = 1;
    private Vector2 previousPoint;
    private double timer = 0;
    private bool timerStarted = false;
    private bool timerFinished = false;
    private bool toggle = false; // false is left hit box, true is right hit box
    private bool runningInEditor = false;

    // Colours, these use floats from 0 -> 1 not standard rgb
    private Color red = new(0.96f, 0.03f, 0.03f, 1);
    private Color white = new(1f, 1f, 1f, 1);

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start");

        if (Application.isEditor)
        {
            this.runningInEditor = true;
        }

        this.menuButton.onClick.AddListener(() => SceneManager.LoadScene("Menu"));
        this.resetButton.onClick.AddListener(() => this.ResetGame());
        this.ResetGame();
    }

    void EndGame()
    {
        this.menuButton.gameObject.SetActive(true);
        this.resetButton.gameObject.SetActive(true);
        this.timerFinished = true;
    }


    void ResetGame()
    {
        this.counter = 2;
        this.leftSpot.color = this.white;
        this.menuButton.gameObject.SetActive(false);
        this.resetButton.gameObject.SetActive(false);
        this.rightSpot.color = this.red;
        this.timer = 0;
        this.timerStarted = false;
        this.timerFinished = false;
        this.toggle = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.timerFinished)
        {
            return;
        }

        Vector2 mousePosition = Input.mousePosition;
        Vector2 point = Camera.main.ScreenToWorldPoint(mousePosition);

        // Track
        if (this.timerStarted && (point.y < -2 || point.y > 2))
        {
            this.timer = 0;
            this.timerStarted = false;
            this.EndGame();
        }

        // Touches (hopefully stops tapping hack or multi touch hacks)
        if (!this.runningInEditor && this.timerStarted && Input.touchCount != 1)
        {
            this.timer = 0;
            this.timerStarted = false;
            this.EndGame();
        }

        // Left hit box
        if (this.toggle == false && this.leftSpot.bounds.Contains(point))
        {
            this.toggle = true;
            this.rightSpot.color = this.white;
            this.leftSpot.color = this.red;
            this.counter--;

            if (this.timerStarted == false)
            {
                this.timerStarted = true;
            }
        }

        // Right hit box
        if (this.toggle == true && this.rightSpot.bounds.Contains(point))
        {
            this.toggle = false;
            this.rightSpot.color = this.red;
            this.leftSpot.color = this.white;

            this.counter--;
        }

        // Timer
        if (timerStarted == true)
        {
            if (this.counter > 0)
            {
                this.timer += Time.deltaTime;
            }
            else if (this.counter == 0)
            {
                // On devices we are stuck at 30fps (or near to it so Time.deltaTime will always be 0.033...) so calculate a more accurate time for
                // the final delta
                double timeCheck = this.timer + Time.deltaTime;
                Debug.Log("Time would have been " + timeCheck);

                SpriteRenderer finalSpot = this.toggle == false ? this.rightSpot : this.leftSpot;

                this.timer += Timing.CalculateFinalDelta(finalSpot.bounds, previousPoint, point, Time.deltaTime);

                Debug.Log("Time actually is " + this.timer);

                this.EndGame();
            }
        }

        // Update UI
        TimeSpan timespan = TimeSpan.FromSeconds(this.timer); // This TimeSpan is rounded to the nearest ms

        counterText.text = this.counter.ToString();
        timerText.text = timespan.ToString(@"mm\:ss\:fff");
        this.previousPoint = point;
    }
}
