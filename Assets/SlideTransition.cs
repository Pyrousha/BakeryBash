using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SlideTransition : NetworkBehaviour
{
    [SerializeField] private NetworkManagerPlayerSelect networkManager;

    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    [Server]
    public void StartSlideUp()
    {
        //called from the sever to start slideup
        anim.SetTrigger("SlideUpTrigger");

        StartSlideUpClient();
    }

    [ClientRpc]
    public void StartSlideUpClient()
    {
        anim.SetTrigger("SlideUpTrigger");
    }


    public void StartSlideDown()
    {
        anim.SetTrigger("SlideDownTrigger");
    }

    public void SlideUpDone()
    {
        networkManager.SlideUpDone();
    }
}
