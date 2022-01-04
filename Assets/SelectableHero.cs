using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableHero : MonoBehaviour
{
    public SelectionPlayer player;
    Sprite heroSprite;

    private void Start()
    {
        heroSprite = transform.GetChild(0).GetComponent<Image>().sprite;
    }


    public void OnClicked()
    {
        player.SelectHero(gameObject.name, heroSprite);
    }
}
