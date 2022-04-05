using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerLobbyDetails : NetworkBehaviour
{
    private int playerNum;
    public int GetPlayerNum => playerNum;

    private bool isMe = false;
    public bool GetIsMe => isMe;

    private List<int> heroIds;
    public List<int> HeroIds => heroIds;

    [SerializeField] private HeroObject[] selectableHeroes;

    [SerializeField] private List<HeroObject> P1Heroes;
    [SerializeField] private List<HeroObject> P2Heroes;

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

    [Server]
    public void SetData(List<int> newHeroIds)
    {
        heroIds = newHeroIds;
        Debug.Log("sending list to client: " + newHeroIds);
        SetDataClient(newHeroIds);
    }

    [ClientRpc]
    public void SetDataClient(List<int> newHeroIds)
    {
        Debug.Log("list got! " + newHeroIds);
        heroIds = newHeroIds;
    }

    public int GetChosenHeroIdByIndex(int index)
    {
        return heroIds[index];
    }

    public HeroObject GetHeroById(int id)
    {
        return selectableHeroes[id];
    }
}
