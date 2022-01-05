using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class NetworkManagerPlayerSelect : NetworkManager
{
    // Custom NetworkManager that simply assigns the correct racket positions when
    // spawning players. The built in RoundRobin spawn method wouldn't work after
    // someone reconnects (both players would be on the same side).

    NetworkConnection p1Connection;
    NetworkConnection p2Connection;

    [SerializeField] private Transform player1Pos;
    [SerializeField] private Transform player2Pos;

    [SerializeField] private PlayerLobbyDetails P1LobbyDetails;
    [SerializeField] private PlayerLobbyDetails P2LobbyDetails;

    public HeroObject p1Hero;
    public HeroObject p2Hero;

    private bool p1Ready = false;
    private bool p2Ready = false;

    /*
    [Server]
    public void DebugHeroes()
    {
        string p1HeroName = (p1Hero == null ? "null" : p1Hero.name);
        string p2HeroName = (p2Hero == null ? "null" : p2Hero.name);
        Debug.Log("P1: " + p1HeroName + ", P2: " + p2HeroName);

        if ((p1Hero != null) && (p2Hero != null))
        {

            GameObject oldObj1 = p1.identity.gameObject;
            NetworkServer.ReplacePlayerForConnection(p1, P1LobbyDetails.gameObject);
            NetworkServer.Destroy(oldObj1);

            GameObject oldObj2 = p2.identity.gameObject;
            NetworkServer.ReplacePlayerForConnection(p2, P2LobbyDetails.gameObject);
            NetworkServer.Destroy(oldObj2);

            //both players have selected, start game
            ServerChangeScene("TestBattleScene");
        }
    }*/

    [Server]
    public void LoadCombatScene()
    {
        GameObject oldObj1 = p1Connection.identity.gameObject;
        NetworkServer.ReplacePlayerForConnection(p1Connection, P1LobbyDetails.gameObject);
        NetworkServer.Destroy(oldObj1);

        GameObject oldObj2 = p2Connection.identity.gameObject;
        NetworkServer.ReplacePlayerForConnection(p2Connection, P2LobbyDetails.gameObject);
        NetworkServer.Destroy(oldObj2);

        //both players have selected, start game
        ServerChangeScene("TestBattleScene");
    }

    [Server]
    public void SetReady(int playerNum, bool isReady)
    {
        switch (playerNum)
        {
            case 1:
                {
                    p1Ready = isReady;
                    break;
                }
            case 2:
                {
                    p2Ready = isReady;
                    break;
                }
            default:
                {
                    Debug.Log("Oh no");
                    break;
                }
        }
        if (p1Ready && p2Ready)
        {
            VerifyHeroSelections();
            LoadCombatScene();
        }
    }

    public void VerifyHeroSelections()
    {
        P1LobbyDetails.LoadDataFromServer();
        P2LobbyDetails.LoadDataFromServer();
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if (numPlayers == 0)
            p1Connection = conn;
        else
            p2Connection = conn;

        // add player at correct spawn position
        Transform start = numPlayers == 0 ? player1Pos : player2Pos;
        GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
