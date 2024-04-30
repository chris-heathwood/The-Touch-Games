using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Buttons
    public UnityEngine.UI.Button swiperButton;
    public UnityEngine.UI.Button tapperButton;

    // Text
    public TMPro.TextMeshProUGUI touchGames;

    private bool menuDisplayed = false;

    private float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        this.swiperButton.onClick.AddListener(() => SceneManager.LoadScene("Swiper"));
        this.swiperButton.gameObject.SetActive(false);
        this.tapperButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (this.menuDisplayed == false && this.timer > 2) {
            this.menuDisplayed = true;
        }

        if (this.menuDisplayed == true) {
            this.swiperButton.gameObject.SetActive(true);
            this.tapperButton.gameObject.SetActive(true);
            this.touchGames.gameObject.SetActive(false);
        } else {
            this.timer += Time.deltaTime;
        }
    }
}
