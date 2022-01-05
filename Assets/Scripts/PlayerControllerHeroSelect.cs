using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerControllerHeroSelect : NetworkBehaviour
{
    private int playerNum;

    public GameObject activePlayerBorder;

    public Text playerNameText;

    private HeroObject selectedHero;

    public Text selectedHeroNameText;
    public Image selectedHeroImage;

    private NetworkManagerPlayerSelect networkManager;
    private PlayerLobbyDetails playerLobbyDetails;

    [SerializeField] private HeroObject[] selectableHeroes;
    [SerializeField] private Image readyButtonImage;
    [SerializeField] private Text readyButtonText;
    private bool isReady = false;

    private void Start()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManagerPlayerSelect>();

        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localScale = new Vector3(1, 1, 1);
        activePlayerBorder.SetActive(isLocalPlayer);

        Debug.Log("name: " + gameObject.name + ", localX: " + transform.localPosition.x);

        if (transform.localPosition.x < 0)
        {
            //is player 1
            playerNum = 1;
            playerLobbyDetails = GameObject.Find("P1LobbyDetails").GetComponent<PlayerLobbyDetails>();
        }
        else
        {
            //is player 2
            playerNum = 2;
            playerLobbyDetails = GameObject.Find("P2LobbyDetails").GetComponent<PlayerLobbyDetails>();
        }

        if (isLocalPlayer)
        {
            playerNameText.text = "You!";

            playerLobbyDetails.SetIsMe(true);
        }
        else
        {
            playerNameText.text = "Enemy Gamer";
        }

        SetReadyVisuals();
    }

    //Called when player clicks on a hero
    [Client]
    public void SelectHero(int heroId)
    {
        if ((!isLocalPlayer) || (isReady))
            return;

        SendHeroSelectionToServer(heroId);
    }

    [Command]
    public void SendHeroSelectionToServer(int heroId)
    {
        HeroObject newHero = selectableHeroes[heroId];
        selectedHero = newHero;

        playerLobbyDetails.SetData(newHero);

        SetHero(heroId);
    }

    //called by server to load other player's selection
    [ClientRpc]
    public void SetHero(int heroId)
    {
        HeroObject newHero = selectableHeroes[heroId];
        selectedHero = newHero;

        playerLobbyDetails.SetData(newHero);

        selectedHero = playerLobbyDetails.GetHeroObj;
        selectedHeroNameText.text = playerLobbyDetails.GetHeroObj.name;
        selectedHeroImage.sprite = playerLobbyDetails.GetHeroObj.HeroSprite;
    }

    [Server]
    public void LoadDataFromServer()
    {
        SetHero(selectedHero.Id);
    }

    public void ReadyClicked()
    {
        if (!isLocalPlayer)
            return;

        if ((!isReady) && (selectedHero == null))
            return;

        isReady = !isReady;

        SendReadyToServer(playerNum, isReady);

        SetReadyVisuals();
    }

    [Command]
    public void SendReadyToServer(int num, bool ready)
    {
        networkManager.SetReady(num, ready);
        LoadReadyStatusFromServer(ready);
    }

    [ClientRpc]
    public void LoadReadyStatusFromServer(bool newIsReady)
    {
        isReady = newIsReady;
        SetReadyVisuals();
    }

    public void SetReadyVisuals()
    {
        if (isReady)
        {
            readyButtonImage.color = Color.green;
            readyButtonText.text = "Ready!";
        }
        else
        {
            readyButtonImage.color = Color.white;
            readyButtonText.text = "Click to Ready Up";
        }
    }
}
