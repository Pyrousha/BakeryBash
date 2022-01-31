using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerControllerHeroSelect : NetworkBehaviour
{
    private int playerNum;

    public GameObject activePlayerBorder;

    //public Text playerNameText;

    public Text selectedHeroNameText;
    public Image selectedHeroImage;

    [SerializeField] private Text bioText;

    private NetworkManagerPlayerSelect networkManager;
    private PlayerLobbyDetails playerLobbyDetails;

    [SerializeField] private HeroObject[] selectableHeroes;
    [SerializeField] private Image readyButtonImage;
    [SerializeField] private Text readyButtonText;
    [SerializeField] private GameObject readyButtonHighlight;
    [SerializeField] private Image enemyReadyButtonImage;
    [SerializeField] private Text enemyReadyButtonText;
    private bool isReady = false;

    private List<int> selectedHeroIds;

    private void Start()
    {
        selectedHeroIds = new List<int>();

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
            networkManager.SetP1Controller(this);
        }
        else
        {
            //is player 2
            playerNum = 2;
            playerLobbyDetails = GameObject.Find("P2LobbyDetails").GetComponent<PlayerLobbyDetails>();
            networkManager.SetP2Controller(this);
        }

        if (isLocalPlayer)
        {
            Debug.Log("I am player" + playerNum);
            //playerNameText.text = "You!";

            playerLobbyDetails.SetIsMe(true);
        }
        else
        {
            //playerNameText.text = "Enemy Gamer";
        }

        if(isClient)
            CheckDestroySelf();

        transform.localPosition = Vector3.zero;

        SetReadyVisuals();
    }

    [Client]
    public void CheckDestroySelf()
    {
        if (!isLocalPlayer)
            gameObject.SetActive(false);
    }

    //Called when player clicks on a hero
    [Client]
    public void SelectHero(int heroId, GameObject selectionObj)
    {
        SetHero(heroId);

        if ((!isLocalPlayer) || (isReady))
            return;

        if (selectedHeroIds.IndexOf(heroId) > -1)
        {
            //hero already selected, deselect
            selectionObj.SetActive(false);
            selectedHeroIds.Remove(heroId);
        }
        else
        {
            //Clicked on a hero that is not yet selected

            //If space left
            if (selectedHeroIds.Count >= 3)
                return;

            //select hero
            selectedHeroIds.Add(heroId);
            selectionObj.SetActive(true);
        }

        if (selectedHeroIds.Count >= 3)
        {
            SendHeroSelectionToServer(selectedHeroIds);
            readyButtonHighlight.SetActive(true);
        }
        else
        {
            readyButtonHighlight.SetActive(false);
        }
    }

    [Command]
    public void SendHeroSelectionToServer(List<int> heroIds)
    {
        Debug.Log("sending arr to server: " + heroIds);
        playerLobbyDetails.SetData(heroIds);
    }

    //Loads clicked on hero's name, image, and bio
    public void SetHero(int heroId)
    {
        HeroObject newHero = selectableHeroes[heroId];

        selectedHeroNameText.text = newHero.name;
        selectedHeroImage.sprite = newHero.HeroSprite;
        bioText.text = newHero.BioText;
    }

    public void ReadyClicked()
    {
        if (isReady)
            return;

        if (!isLocalPlayer)
            return;

        if (selectedHeroIds.Count < 3)
            return;

        isReady = true;

        SendReadyToServer(playerNum, true);

        SetReadyVisuals();
    }

    [Command]
    public void SendReadyToServer(int pNum, bool ready)
    {
        networkManager.SetReady(pNum, true, playerLobbyDetails.HeroIds);
        networkManager.LoadReadyVisuals(pNum, true);
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
            readyButtonText.text = "Ready?";
        }
    }

    [ClientRpc]
    public void LoadEnemyReadyVisuals(int pNum, bool enemyReady)
    {
        Debug.Log("player num " + pNum + " is " + (enemyReady ? "" : "not ")+ " ready");


        if (playerNum == pNum)
            return;

        if (enemyReady)
        {
            enemyReadyButtonImage.color = Color.green;
            enemyReadyButtonText.text = "Opponent is Ready!";
        }
        else
        {
            enemyReadyButtonImage.color = Color.white;
            enemyReadyButtonText.text = "Opponent Not Ready";
        }
    }
}
