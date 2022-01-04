using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.Examples.Pong
{
    public class NetworkManagerPlayerSelect : NetworkManager
    {
        // Custom NetworkManager that simply assigns the correct racket positions when
        // spawning players. The built in RoundRobin spawn method wouldn't work after
        // someone reconnects (both players would be on the same side).

        public SelectionPlayer player1;
        public SelectionPlayer player2;

        public Transform player1Pos;
        public Transform player2Pos;

        NetworkConnection p1;
        NetworkConnection p2;

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            // add player at correct spawn position
            Transform start = numPlayers == 0 ? player1Pos : player2Pos;
            GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
            NetworkServer.AddPlayerForConnection(conn, player);
        }

    public override void OnServerConnect(NetworkConnection conn)
        {
            if (numPlayers == 0)
            {
                //new player will be player 1
                p1 = conn;
            }
            else
            {
                //new player will be player 2
                p2 = conn;
            }
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
        }
        /*
        public override void OnServerConnect(NetworkConnection conn)
        {
            // add player at correct spawn position
            Transform start;
            if (numPlayers == 0)
            {
                start = leftRacketSpawn;
            }
            else
            {
                start = rightRacketSpawn;
            }

            Debug.Log("Number of players: " + numPlayers);

            GameObject player = Instantiate(playerPrefab);
            player.transform.SetParent(canvas, false);
            player.transform.position = start.position;
            NetworkServer.AddPlayerForConnection(conn, player);

            base.OnServerConnect(conn);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
        }*/
    }
}