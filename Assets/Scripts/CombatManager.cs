using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CombatManager : NetworkBehaviour
{
    [SerializeField] private int playerTurn = 1;
    public int PlayerTurn => playerTurn;
    private int localPlayerNum;
    public void SetLocalPlayerNum(int newNum)
    {
        localPlayerNum = newNum;

        UpdateTurnIndicator();
    }

    [Header("References")]
    [SerializeField] private PlayerControllerCombat p1Controller;
    [SerializeField] private PlayerControllerCombat p2Controller;

    [SerializeField] private List<CombatHero> p1Heroes;
    [SerializeField] private List<CombatHero> p2Heroes;

    public List<CombatHero> GetP1Heroes => p1Heroes;
    public List<CombatHero> GetP2Heroes => p2Heroes;

    [SerializeField] private Image turnColorImg;
    [SerializeField] private Text turnText;

    [Header("Colors")]
    [SerializeField] private Color teamColor;
    [SerializeField] private Color enemyColor;
    public Color TeamColor => teamColor;
    public Color EnemyColor => enemyColor;

    [Command(requiresAuthority = false)]
    public void DoneTurn()
    {
        playerTurn = (playerTurn == 1) ? 2 : 1;

        SetPlayerTurnOnClient(playerTurn);
    }

    [ClientRpc]
    public void SetPlayerTurnOnClient(int playerTurnNum)
    {
        playerTurn = playerTurnNum;

        UpdateTurnIndicator();
    }

    public PlayerControllerCombat GetCurrPlayerController()
    {
        if (playerTurn == 1)
        {
            return p1Controller;
        }
        else
        {
            return p2Controller;
        }
    }

    public int GetIndexOfHero(int playerNum, CombatHero hero)
    {
        if (playerNum == 1)
        {
            return p1Heroes.IndexOf(hero);
        }
        else
        {
            return p2Heroes.IndexOf(hero);
        }
    }
        

    public CombatHero GetHeroByIndex(int playerNum, int index)
    {
        if(playerNum == 1)
        {
            return p1Heroes[index];
        }
        else
        {
            return p2Heroes[index];
        }
    }

    public void UpdateTurnIndicator()
    {
        bool isMyTurn = (localPlayerNum == playerTurn);

        if (isMyTurn)
        {
            turnColorImg.color = teamColor;
            turnText.text = "Your move!";
        }
        else
        {
            turnColorImg.color = enemyColor;
            turnText.text = "Enemy's turn";
        }
    }
}
