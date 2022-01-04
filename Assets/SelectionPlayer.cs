using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class SelectionPlayer : NetworkBehaviour
{
    public GameObject activePlayerBorder;

    public Text playerNameText;

    public Text selectedHeroNameText;
    public Image selectedHeroImage;

    [SerializeField] private Sprite[] heroSprites;

    private void Start()
    {
        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localScale = new Vector3(1, 1, 1);
        activePlayerBorder.SetActive(isLocalPlayer);

        if (isLocalPlayer)
        {
            playerNameText.text = "You!";
        }
        else
        {
            playerNameText.text = "Enemy Gamer";
        }
    }

    //Called when player clicks on a hero
    [Client]
    public void SelectHero(string heroName, Sprite heroImage)
    {
        if (!isLocalPlayer)
            return;

        selectedHeroNameText.text = heroName;
        selectedHeroImage.sprite = heroImage;

        SendHeroSelectionToServer(heroName);
    }

    [Command]
    public void SendHeroSelectionToServer(string heroName)
    {
        Debug.Log("player clicked on hero: " + heroName);
        int heroNum = (int)Char.GetNumericValue(heroName[4]) - 1;
        SetHero(heroName, heroNum);
    }

    //called by server to load other player's selection
    [ClientRpc]
    public void SetHero(string heroName, int heroNum)
    {
        selectedHeroNameText.text = heroName;
        selectedHeroImage.sprite = heroSprites[heroNum];
    }
}
