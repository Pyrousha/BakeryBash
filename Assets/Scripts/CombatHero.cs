using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;

public class CombatHero : NetworkBehaviour
{
    [SerializeField] private bool isMine;

    [Header("HeroObject")]
    [SerializeField] private HeroObject heroObj;

    [Header("References")]
    [SerializeField] private SpriteRenderer heroSpriteRend;
    [SerializeField] private Text nameText;
    private CombatManager combatManager;
    private PlayerControllerCombat playerController;

    private Animator anim;
    [SerializeField] private SpriteRenderer bgSpriteRend;

    [Header("Parametes and such")]
    [SerializeField] private int startingVertexId = -1;
    [SerializeField] private BoardVertex currVertex;
    public BoardVertex CurrVertex => currVertex;

    private bool doneStart = false;

    private void Start()
    {
        if (doneStart)
            return;

        combatManager = FindObjectOfType<CombatManager>();

        anim = GetComponent<Animator>();

        if (heroObj != null)
            LoadHeroObj();

        if (currVertex != null)
            MoveToVertex(currVertex);
        else
        {
            if (startingVertexId > -1)
            {
                BoardVertex vert = FindObjectOfType<GameBoardManager>().GetVertexWithId(startingVertexId);
                MoveToVertex(vert);
            }
        }

        doneStart = true;
    }

    public void SetOwnership(PlayerControllerCombat newPlayer, bool newIsMine)
    {
        if (doneStart == false)
            Start();

        isMine = newIsMine;

        //Set this unit color based on if it is owned by the player
        Color colToUse;

        if (isMine == true)
        {
            colToUse = combatManager.TeamColor;
            playerController = newPlayer;
        }
        else
            colToUse = combatManager.EnemyColor;

        bgSpriteRend.color = colToUse;
    }

    public void SetHeroObj(HeroObject newHeroObj)
    {
        heroObj = newHeroObj;
        LoadHeroObj();
    }

    public void LoadHeroObj()
    {
        heroSpriteRend.sprite = heroObj.HeroSprite;
        nameText.text = heroObj.name;
    }

    public void OnMouseDown()
    {
        if (isMine)
        {
            Debug.Log("clicked on my hero");
            playerController.OnHeroClicked(this);
            //DoSpin();
        }    
    }

    public void HighlightPossibleMoves()
    {

    }
        

    public void DoSpin()
    {
        SendSpinToServer();
    }

    [Command(requiresAuthority = false)]
    public void SendSpinToServer()
    {
        SendSpinToClients();
    }

    //called by server to load other player's selection
    [ClientRpc]
    public void SendSpinToClients()
    {
        anim.SetTrigger("SpinTrigger");
    }

    public void MoveToVertex(BoardVertex vertex)
    {
        currVertex = vertex;
        transform.position = vertex.transform.position + new Vector3(0,0,-0.2f);
    }
}
