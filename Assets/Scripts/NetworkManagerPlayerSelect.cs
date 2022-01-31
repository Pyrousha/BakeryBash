using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System;

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

    [SerializeField] private LobbyCountdown lobbyCountdown;
    [SerializeField] private SlideTransition slideTransition;

    private bool p1Ready = false;
    private bool p2Ready = false;

    private PlayerControllerHeroSelect p1Controller;
    private PlayerControllerHeroSelect p2Controller;

    private List<int> p1heroIds;
    private List<int> p2heroIds;

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

    public void SetP1Controller(PlayerControllerHeroSelect playerControllerHeroSelect)
    {
        p1Controller = playerControllerHeroSelect;
    }

    public void SetP2Controller(PlayerControllerHeroSelect playerControllerHeroSelect)
    {
        p2Controller = playerControllerHeroSelect;
    }

    [Server]
    public void LoadCombatScene()
    {
        GameObject oldObj1 = p1Connection.identity.gameObject;
        NetworkServer.ReplacePlayerForConnection(p1Connection, P1LobbyDetails.gameObject, true);
        NetworkServer.Destroy(oldObj1);

        GameObject oldObj2 = p2Connection.identity.gameObject;
        NetworkServer.ReplacePlayerForConnection(p2Connection, P2LobbyDetails.gameObject, true);
        NetworkServer.Destroy(oldObj2);

        //both players have selected, start game
        ServerChangeScene("CombatScene");
    }

    [Server]
    public void SetReady(int playerNum, bool isReady, List<int> heroIds)
    {
        switch (playerNum)
        {
            case 1:
                {
                    p1Ready = true;
                    p1heroIds = heroIds;
                    P1LobbyDetails.SetDataClient(heroIds);
                    break;
                }
            case 2:
                {
                    p2Ready = true;
                    p2heroIds = heroIds;
                    P2LobbyDetails.SetDataClient(heroIds);
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
            P1LobbyDetails.SetDataClient(p1heroIds);
            P2LobbyDetails.SetDataClient(p2heroIds);

            lobbyCountdown.StartCountdown();
        }
    }

    public void CountdownDone()
    {
        slideTransition.StartSlideUp();
    }

    [Server]
    public void SlideUpDone()
    {
        LoadCombatScene();
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        slideTransition.StartSlideDown();

        base.OnServerSceneChanged(sceneName);
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        slideTransition.StartSlideDown();

        base.OnClientSceneChanged(conn);
    }

    public void LoadReadyVisuals(int pNum, bool v)
    {
        p1Controller.LoadEnemyReadyVisuals(pNum, true); //Load "enemy is ready" on opponent's screen
        p2Controller.LoadEnemyReadyVisuals(pNum, true); //Load "enemy is ready" on opponent's screen
    }
}
