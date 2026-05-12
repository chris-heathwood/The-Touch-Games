using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public UnityEngine.UI.Button swiperSprintButton;
    public UnityEngine.UI.Button swiperMiddleButton;
    public UnityEngine.UI.Button swiperMarathonButton;
    public UnityEngine.UI.Button tapperButton;
    public UnityEngine.UI.Button beeperButton;
    public UnityEngine.UI.Button rotatorButton;
    public UnityEngine.UI.Button timerButton;
    public UnityEngine.UI.Button balancerButton;
    public UnityEngine.UI.Button tracerButton;

    public TMPro.TextMeshProUGUI touchGames;

    private bool menuDisplayed = false;
    private float timer = 0;

    void Start()
    {
        swiperSprintButton.onClick.AddListener(() => SceneManager.LoadScene("Swiper"));
        swiperMiddleButton.onClick.AddListener(() => SceneManager.LoadScene("SwiperMiddle"));
        swiperMarathonButton.onClick.AddListener(() => SceneManager.LoadScene("SwiperMarathon"));
        tapperButton.onClick.AddListener(() => SceneManager.LoadScene("Tapper"));
        beeperButton.onClick.AddListener(() => SceneManager.LoadScene("Beeper"));
        rotatorButton.onClick.AddListener(() => SceneManager.LoadScene("Rotator"));
        timerButton.onClick.AddListener(() => SceneManager.LoadScene("Timer"));
        balancerButton.onClick.AddListener(() => SceneManager.LoadScene("Balancer"));
        tracerButton.onClick.AddListener(() => SceneManager.LoadScene("Tracer"));

        swiperSprintButton.gameObject.SetActive(false);
        swiperMiddleButton.gameObject.SetActive(false);
        swiperMarathonButton.gameObject.SetActive(false);
        tapperButton.gameObject.SetActive(false);
        beeperButton.gameObject.SetActive(false);
        rotatorButton.gameObject.SetActive(false);
        timerButton.gameObject.SetActive(false);
        balancerButton.gameObject.SetActive(false);
        tracerButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!menuDisplayed && timer > 2)
        {
            menuDisplayed = true;
        }

        if (menuDisplayed)
        {
            swiperSprintButton.gameObject.SetActive(true);
            swiperMiddleButton.gameObject.SetActive(true);
            swiperMarathonButton.gameObject.SetActive(true);
            tapperButton.gameObject.SetActive(true);
            beeperButton.gameObject.SetActive(true);
            rotatorButton.gameObject.SetActive(true);
            timerButton.gameObject.SetActive(true);
            balancerButton.gameObject.SetActive(true);
            tracerButton.gameObject.SetActive(true);
            touchGames.gameObject.SetActive(false);
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
}
