using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Buttons using UnityEngine.UI.Button
    public UnityEngine.UI.Button swiperSprintButton;
    public UnityEngine.UI.Button swiperMiddleButton;
    public UnityEngine.UI.Button swiperMarathonButton;
    public UnityEngine.UI.Button tapperButton;

    // Text using TMPro.TextMeshProUGUI
    public TMPro.TextMeshProUGUI touchGames;

    private bool menuDisplayed = false;
    private float timer = 0;

    void Start()
    {
        swiperSprintButton.onClick.AddListener(() => SceneManager.LoadScene("Swiper"));
        swiperMiddleButton.onClick.AddListener(() => SceneManager.LoadScene("SwiperMiddle"));
        swiperMarathonButton.onClick.AddListener(() => SceneManager.LoadScene("SwiperMarathon"));

        swiperSprintButton.gameObject.SetActive(false);
        swiperMiddleButton.gameObject.SetActive(false);
        swiperMarathonButton.gameObject.SetActive(false);
        tapperButton.gameObject.SetActive(false);
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
            touchGames.gameObject.SetActive(false);
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
}
