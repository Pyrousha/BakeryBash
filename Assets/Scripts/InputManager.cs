using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    //Used for singleton
    public static InputManager Instance { get; private set; }
 
    //Create Keycodes that will be associated with each of our commands.
    //These can be accessed by any other script in our game
    private int inputMode;
 
    void Awake()
    {
        //Singleton pattern
        if(Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if(Instance != this)
        {
            Destroy(Instance.gameObject);
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        inputMode = PlayerPrefs.GetInt("inputMode", 0);
    }

    public void SetInputMode(int dropdownValue) {
        Debug.Log(dropdownValue);
        Instance.inputMode = dropdownValue;
        PlayerPrefs.SetInt("inputMode", dropdownValue);
    }

    public bool IsInMouseMode() {
        return (Instance.inputMode == 0 ? true : false);
    }
}