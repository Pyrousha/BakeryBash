using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerLobbyDetails : NetworkBehaviour
{
    private int playerNum;
    public int GetPlayerNum => playerNum;

    private bool isMe = false;
    public bool GetIsMe => isMe;

    [SerializeField] private HeroObject heroObj;
    public HeroObject GetHeroObj => heroObj;

    [SerializeField] private NetworkManagerPlayerSelect networkManager;
    [SerializeField] private HeroObject[] selectableHeroes;

    private void Start()
    {
        if (transform.localPosition.x < 0)
            playerNum = 1;
        else
            playerNum = 2;

        transform.parent = null;
        DontDestroyOnLoad(transform.gameObject);
    }

    [Client]
    public void SetIsMe(bool newIsMe)
    {
        isMe = newIsMe;
    }

    public void SetData(HeroObject newHeroObj)
    {
        heroObj = newHeroObj;
    }

    [Server]
    public void LoadDataFromServer()
    {
        SetHero(heroObj.Id);
    }

    //called by server to load other player's selection
    [ClientRpc]
    public void SetHero(int heroId)
    {
        HeroObject newHero = selectableHeroes[heroId];
        heroObj = newHero;
    }
}
