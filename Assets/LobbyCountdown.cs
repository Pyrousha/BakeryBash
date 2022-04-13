using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LobbyCountdown : NetworkBehaviour
{
    [SerializeField] private NetworkManagerPlayerSelect networkManager;
    private Animator anim;

    [SerializeField] private SlideTransition slideTransition;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void CountdownDone()
    {
        if(networkManager != null)
            networkManager.CountdownDone();
        else
        {
            //PassNPlayMode
            slideTransition.PNPStartSlideUp();
        }

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

    public void StartCountdownPNP()
    {
        //called from the sever to start countdown
        anim.SetTrigger("StartCountdown");
    }
}
