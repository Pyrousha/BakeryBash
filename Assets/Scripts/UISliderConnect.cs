using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISliderConnect : MonoBehaviour
{
    private Slider volumeSlider;
    public TMP_InputField numberBox;
    [SerializeField] private MenuMusicManager music;

    public void Start() {
        numberBox.onValueChanged.AddListener(delegate {ValueChangeCheck(); });
        volumeSlider = transform.GetComponent<Slider>();
        volumeSlider.value = (music.GetVolume() * 100);
    }

    public void ValueChangeCheck()
    {
        int.TryParse(numberBox.text, out int result);
        volumeSlider.value = result;
        music.ChangeVolume(volumeSlider.value / 100);
    }
}