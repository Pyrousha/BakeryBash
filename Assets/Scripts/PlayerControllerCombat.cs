﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerControllerCombat : NetworkBehaviour
{
    [SerializeField] private int playerNum;
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private GameBoardManager boardManager;
    private PlayerLobbyDetails playerLobbyDetails;
    private PlayerLobbyDetails enemyLobbyDetails;

    private CombatHero selectedHero;
    private List<BoardVertex> validMoveVertices;
    private List<BoardVertex> validAttackVertices;

    private int numCurrTokens;
    private int maxTokens = 15;

    [Client]
    // Start is called before the first frame update
    void Start()
    {
        validMoveVertices = new List<BoardVertex>();
        validAttackVertices = new List<BoardVertex>();

        numCurrTokens = maxTokens;

        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localScale = new Vector3(1, 1, 1);

        if (playerNum == 1)
        {
            playerNum = 1;
            playerLobbyDetails = GameObject.Find("P1LobbyDetails").GetComponent<PlayerLobbyDetails>();
            enemyLobbyDetails = GameObject.Find("P2LobbyDetails").GetComponent<PlayerLobbyDetails>();
        }
        else if (playerNum == 2)
        {
            playerLobbyDetails = GameObject.Find("P2LobbyDetails").GetComponent<PlayerLobbyDetails>();
            enemyLobbyDetails = GameObject.Find("P1LobbyDetails").GetComponent<PlayerLobbyDetails>();
        }

        if (playerLobbyDetails.isLocalPlayer)
        {
            SetHeroColors();
            combatManager.SetLocalPlayerNum(playerNum);
        }
    }

    [Server]
    public void TurnStartedServer()
    {
        numCurrTokens = maxTokens;

        TurnStartedClient();
    }

    [ClientRpc]
    public void TurnStartedClient()
    {
        numCurrTokens = maxTokens;
        combatManager.UpdateTokenVisualCount(numCurrTokens);
    }

    [Client]
    public void SetHeroColors()
    {
        if (playerNum == 1)
        {
            foreach(CombatHero hero in combatManager.GetP1Heroes)
            {
                hero.SetOwnership(this, true);
                hero.SetHeroObj(playerLobbyDetails.GetHeroObj);
            }
            foreach (CombatHero hero in combatManager.GetP2Heroes)
            {
                hero.SetOwnership(this, false);
                hero.SetHeroObj(enemyLobbyDetails.GetHeroObj);
            }

            return;
        }
        if (playerNum == 2)
        {
            foreach (CombatHero hero in combatManager.GetP1Heroes)
            {
                hero.SetOwnership(this, false);
                hero.SetHeroObj(enemyLobbyDetails.GetHeroObj);
            }
            foreach (CombatHero hero in combatManager.GetP2Heroes)
            {
                hero.SetOwnership(this, true);
                hero.SetHeroObj(playerLobbyDetails.GetHeroObj);
            }
        }
    }

    [Client]
    public void OnHeroClicked(CombatHero newClickedHero, bool isMine)
    {
        //Note: this is only called on the player's combatController

        if (combatManager.PlayerTurn != playerNum)
        {
            Debug.Log("not my turn, exiting");
            return;
        }

        if (isMine)
        {
            //Clicked on one of their own heroes, highlight adjacent vertices

            selectedHero = newClickedHero;

            List<BoardVertex> adjVertices = boardManager.GetWalkableEdges(newClickedHero.CurrVertex, selectedHero.CanWalkOverSpecialTerrain);

            foreach (BoardVertex vertex in adjVertices)
            {
                vertex.HighlightMove();
            }

            validMoveVertices = adjVertices;

            //Calculate valid attack vertices
            validAttackVertices = new List<BoardVertex>();

            List<BoardVertex> tempValidAttackVertices = GraphHelper.BFS(newClickedHero.CurrVertex, newClickedHero.BasicAttackRange);
            foreach(BoardVertex vert in tempValidAttackVertices)
            {
                if (vert.combatHero != null)
                    validAttackVertices.Add(vert);
            }

            foreach(BoardVertex vert in validAttackVertices)
            {
                vert.SetReticleActive(true);
            }
        }
        else
        {
            //Clicked on an enemy hero, try to see if attacking is possible

            if (selectedHero == null)
                return;

            CombatHero enemyHero = newClickedHero;

            BoardVertex enemyHeroVertex = enemyHero.CurrVertex;

            if(validAttackVertices.IndexOf(enemyHeroVertex) > -1)
            {
                //Enemy hero is in range, so attack is valid
                if(TryUseMoveTokens(1))
                    enemyHero.TakeDamageServer(selectedHero.BasicAttackDamage);

                if (numCurrTokens == 0)
                    EndMyTurn();
            }
        }
    }

    [Client]
    public void OnVertexClicked(BoardVertex vertex)
    {
        if (combatManager.PlayerTurn != playerNum)
            return;

        if (validMoveVertices.IndexOf(vertex) > -1)
        {
            int heroIndex = combatManager.GetIndexOfHero(playerNum, selectedHero);
            if (TryUseMoveTokens(1))
                ServerMoveHero(playerNum, heroIndex, vertex.VertexId);

            if (numCurrTokens == 0)
            {
                ResetHeroMoveVisuals();
                EndMyTurn();
            }
        }
    }

    [Client]
    public void TryReclickHero(CombatHero hero)
    {
        if (combatManager.PlayerTurn != playerNum) 
            return;

        ResetHeroMoveVisuals();

        if (numCurrTokens > 0)
        {
            OnHeroClicked(hero, true);
        }
    }

    [Client]
    private void ResetHeroMoveVisuals()
    {
        foreach (BoardVertex vert in validMoveVertices)
        {
            vert.ResetColor();
        }

        foreach (BoardVertex vert in validAttackVertices)
        {
            vert.SetReticleActive(false);
        }

        validMoveVertices = new List<BoardVertex>(); ;
        validAttackVertices = new List<BoardVertex>(); ;
    }

    [Client]
    private bool TryUseMoveTokens(int tokenCost)
    {
        if(numCurrTokens - tokenCost >= 0)
        {
            //player has enough tokens to do this move
            numCurrTokens -= tokenCost;
            combatManager.UpdateTokenVisualCount(numCurrTokens);

            return true;
        }
        return false;
    }

    [Client]
    public void EndMyTurn()
    {
        numCurrTokens = 0;
        combatManager.UpdateTokenVisualCount(numCurrTokens);

        foreach (BoardVertex vert in validMoveVertices)
        {
            vert.ResetColor();
        }

        foreach (BoardVertex vert in validAttackVertices)
        {
            vert.SetReticleActive(false);
        }

        selectedHero = null;
        validMoveVertices = new List<BoardVertex>();
        validAttackVertices = new List<BoardVertex>();

        combatManager.DoneTurn();
    }

    [Command(requiresAuthority = false)]
    public void ServerMoveHero(int playerNum, int heroIndex, int newVertexId)
    {
        CombatHero heroToMove = combatManager.GetHeroByIndex(playerNum, heroIndex);
        BoardVertex vertex = boardManager.GetVertexWithId(newVertexId);
        heroToMove.MoveToVertex(vertex);

        MoveHeroOnClients(playerNum, heroIndex, newVertexId);
    }

    [ClientRpc]
    public void MoveHeroOnClients(int playerNum, int heroIndex, int newVertexId)
    {
        CombatHero heroToMove = combatManager.GetHeroByIndex(playerNum, heroIndex);
        BoardVertex vertex = boardManager.GetVertexWithId(newVertexId);
        heroToMove.MoveToVertex(vertex);

        if(heroToMove.IsMine)
            TryReclickHero(heroToMove);
    }
}
