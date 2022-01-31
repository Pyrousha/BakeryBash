using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenBakeButton : MonoBehaviour
{
    [SerializeField] private GameObject bakeOverlay;

    public void OnClicked()
    {
        bakeOverlay.SetActive(!bakeOverlay.activeSelf);
    }
}
