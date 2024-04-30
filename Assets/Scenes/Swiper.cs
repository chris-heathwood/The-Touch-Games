using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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

    private int counter = 10;
    private Vector2 previousPoint;
    private float timer = 0;
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
        this.counter = 10;
        this.leftSpot.color = this.white;
        this.menuButton.gameObject.SetActive(false);
        this.resetButton.gameObject.SetActive(false);
        this.rightSpot.color = this.red;
        this.timer = 0;
        this.timerStarted = false;
        this.timerFinished = false;
        this.toggle = false;
    }

    // float CalculateCrossingTime(Vector2 topLeft, Vector2 bottomRight, Vector2 outsidePoint, Vector2 insidePoint, float time)
    // {
    //     // Calculate the total distance traveled (using Pythagorean theorem)
    //     double totalDistance = Math.Sqrt(Math.Pow(outsidePoint.x - insidePoint.x, 2) + Math.Pow(outsidePoint.y - insidePoint.y, 2));
    //     // Speed = Distance / Time
    //     double speed = totalDistance / Time.deltaTime;



    //         // TODO Which box did a finish on? us that for distanceToFinishLine
    //         // double distanceToFinishLine = Math.Sqrt(Math.Pow(point.x - this.previousPoint.x, 2) + Math.Pow(point.y - this.previousPoint.y, 2));

    //         // time = distance / speed
    //         // double finishLineTime = distanceToFinishLine / speed;
    //         // TODO Convert this to a float
    //         // Add it on

    //         // Could do with a function
    //         // bounding box
    //         // pointA pointB
    //         // time
    //         // returns time of intersection

    // }

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
        if (this.toggle == false && point.x > -7.25 && point.x < -4.75 && point.y > -1.25 && point.y < 1.25)
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
        if (this.toggle == true && point.x < 7.25 && point.x > 4.75 && point.y > -1.25 && point.y < 1.25)
        {
            this.toggle = false;
            this.rightSpot.color = this.red;
            this.leftSpot.color = this.white;

            this.counter--;
        }


        // Timer
        if (timerStarted == true && this.counter > 0)
        {
            this.timer += Time.deltaTime;
        }

        if (this.counter == 0)
        {



            this.EndGame();
        }

        // Update UI
        TimeSpan timespan = TimeSpan.FromSeconds(this.timer);

        counterText.text = this.counter.ToString();
        timerText.text = timespan.ToString(@"mm\:ss\:fff");
        this.previousPoint = point;
    }
}
