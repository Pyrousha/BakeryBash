using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UINumberBoxConnect : MonoBehaviour
{
    public Slider volumeSlider;
    private TMP_InputField numberBox;
    [SerializeField] private MenuMusicManager music;

    public void Start() {
        volumeSlider.onValueChanged.AddListener(delegate {ValueChangeCheck(); });
        numberBox = transform.GetComponent<TMP_InputField>();
        numberBox.text = (music.GetVolume() * 100).ToString();
    }

    public void ValueChangeCheck()
    {
        numberBox.text = volumeSlider.value.ToString();
        music.ChangeVolume(volumeSlider.value / 100);
    }
}
