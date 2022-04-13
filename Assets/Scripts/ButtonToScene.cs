using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonToScene : MonoBehaviour
{
    public void LoadCharSelect() {
        SceneManager.LoadScene("CharSelect - Multiplayer");
    }

    public void LoadCharSelectPNP()
    {
        SceneManager.LoadScene("CharSelect - PassNPlay");
    }
}
