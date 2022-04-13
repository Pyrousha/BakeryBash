using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class SlideTransition : NetworkBehaviour
{
    [SerializeField] private NetworkManagerPlayerSelect networkManager;

    private Animator anim;

    [SerializeField] private HeroSelectController heroSelectController;

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

    //PassNPlay Funcions

    public void PNPStartSlideUp()
    {
        anim.SetTrigger("SlideUpTrigger");
    }

    public void PNPSlideUpDone()
    {
        heroSelectController.StartGame();
    }

    private void OnLevelWasLoaded(int level)
    {
        if (level == 3)
            anim.SetTrigger("SlideDownTrigger");
    }
}
