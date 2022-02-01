using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenBakeButton : MonoBehaviour
{
    [SerializeField] private GameObject bakeOverlay;

    [SerializeField] private PlayerControllerCombat p1;
    [SerializeField] private PlayerControllerCombat p2;

    public void OnClicked()
    {
        bakeOverlay.SetActive(!bakeOverlay.activeSelf);

        p1.OpenBakeClicked();
        p2.OpenBakeClicked();
    }
}
