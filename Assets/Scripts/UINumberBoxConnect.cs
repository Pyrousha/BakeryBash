using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UINumberBoxConnect : MonoBehaviour
{
    public Slider volumeSlider;
    private TMP_InputField numberBox;

    public void Start() {
        volumeSlider.onValueChanged.AddListener (delegate {ValueChangeCheck();});
        numberBox = transform.GetComponent<TMP_InputField>();
        ValueChangeCheck();
    }

    public void ValueChangeCheck()
    {
        numberBox.text = Mathf.Round(volumeSlider.value).ToString();
    }
}
