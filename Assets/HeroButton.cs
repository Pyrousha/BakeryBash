using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroButton : MonoBehaviour
{
    [SerializeField] private Image heroImage;
    [SerializeField] private CombatHero hero;

    [SerializeField] private CameraController camController;

    public void OnClicked()
    {
        camController.MoveToHero(hero);

        if(hero.AllyColor == CombatManager.Instance.GetCurrPlayerController().PlayerColor)
        {
            hero.OnClicked();
        }
    }

    public void SetVisual(Sprite spr)
    {
        heroImage.sprite = spr;
    }
}
