using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LobbyCountdown : NetworkBehaviour
{
    [SerializeField] private NetworkManagerPlayerSelect networkManager;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void CountdownDone()
    {
        networkManager.CountdownDone();

        gameObject.SetActive(false);
    }

    [Server]
    public void StartCountdown()
    {
        //called from the sever to start countdown
        anim.SetTrigger("StartCountdown");

        StartCountdownClient();
    }

    [ClientRpc]
    public void StartCountdownClient()
    {
        anim.SetTrigger("StartCountdown");
    }
}
