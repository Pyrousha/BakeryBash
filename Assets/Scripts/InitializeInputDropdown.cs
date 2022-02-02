using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InitializeInputDropdown : MonoBehaviour
{
    private TMP_Dropdown dropdown;

    // Start is called before the first frame update
    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.value = PlayerPrefs.GetInt("inputMode", 0);
    }
}
