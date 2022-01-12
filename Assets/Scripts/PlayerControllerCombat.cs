using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerControllerCombat : NetworkBehaviour
{
    [SerializeField] private int playerNum;
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private GameBoardManager boardManager;
    private PlayerLobbyDetails playerLobbyDetails;
    private PlayerLobbyDetails enemyLobbyDetails;

    private CombatHero selectedHero;
    private List<BoardVertex> validVertices;

    [Client]
    // Start is called before the first frame update
    void Start()
    {
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

            List<BoardVertex> adjVertices = newClickedHero.CurrVertex.AdjacentVertices;

            foreach (BoardVertex vertex in adjVertices)
            {
                vertex.Highlight();
            }

            validVertices = adjVertices;
        }
        else
        {
            //Clicked on an enemy hero, try to see if attacking is possible

            if (selectedHero == null)
                return;

            CombatHero enemyHero = newClickedHero;

            List<BoardVertex> adjVertices = selectedHero.CurrVertex.AdjacentVertices;
            BoardVertex enemyHeroVertex = enemyHero.CurrVertex;

            if(adjVertices.IndexOf(enemyHeroVertex) > -1)
            {
                //Enemy hero is in an adjacent space, so attack is valid

                enemyHero.TakeDamage(selectedHero.BasicAttackDamage);

                EndMyTurn();
            }
        }
    }

    [Client]
    public void OnVertexClicked(BoardVertex vertex)
    {
        if (combatManager.PlayerTurn != playerNum)
        {
            selectedHero = null;
            validVertices = null;

            return;
        }

        if (validVertices.IndexOf(vertex) > -1)
        {
            int heroIndex = combatManager.GetIndexOfHero(playerNum, selectedHero);
            ServerMoveHero(playerNum, heroIndex, vertex.VertexId);

            EndMyTurn();
        }
    }

    [Client]
    public void EndMyTurn()
    {
        foreach (BoardVertex vert in validVertices)
        {
            vert.ResetColor();
        }

        selectedHero = null;
        validVertices = null;

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
    }
}
