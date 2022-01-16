using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CutsceneManager : MonoBehaviour
{
    public TextMeshProUGUI Text1;
    public TextMeshProUGUI Text2;
    public TextMeshProUGUI Text3;
    public TextMeshProUGUI Text4;
    public GameObject MainMenu;

    public float fadeSpeed;

    enum FadeStages {
        Start,
        Text1,
        Text2,
        Text3,
        Text4,
        Menu
    }

    FadeStages fadeStage = FadeStages.Start;

    float fadeAmount;

    void Start() {
        Text1.color = new Color32(255, 255, 255, 0);
        Text2.color = new Color32(255, 255, 255, 0);
        Text3.color = new Color32(255, 255, 255, 0);
        Text4.color = new Color32(255, 255, 255, 0);
    }

    public void FadeText1() {
        fadeStage = FadeStages.Text1;
        fadeAmount = 0.0f;
    }

    public void FadeText2() {
        fadeStage = FadeStages.Text2;
        fadeAmount = 0.0f;
    }

    public void FadeText3() {
        fadeStage = FadeStages.Text3;
        fadeAmount = 0.0f;
    }

    public void FadeText4() {
        fadeStage = FadeStages.Text4;
        fadeAmount = 0.0f;
    }

    public void StartMenu() {
        Debug.Log("Bruh!");
        fadeStage = FadeStages.Menu;
        MainMenu.SetActive(true);
    }

    void Update() {
        if(fadeAmount < 255.0f) {
            fadeAmount += fadeSpeed * Time.deltaTime;
        } else {
            fadeAmount = 255.0f;
        }
        switch (fadeStage) {
            case FadeStages.Text1:
                Text1.color = new Color32(255, 255, 255, (byte) Mathf.RoundToInt(fadeAmount));
                break;
            case FadeStages.Text2:
                Text2.color = new Color32(255, 255, 255, (byte) Mathf.RoundToInt(fadeAmount));
                break;
            case FadeStages.Text3:
                Text3.color = new Color32(255, 255, 255, (byte) Mathf.RoundToInt(fadeAmount));
                break;
            case FadeStages.Text4:
                Text4.color = new Color32(255, 255, 255, (byte) Mathf.RoundToInt(fadeAmount));
                break;
        }
    }
}
