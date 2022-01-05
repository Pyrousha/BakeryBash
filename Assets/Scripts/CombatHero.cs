using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CombatHero : NetworkBehaviour
{
    [SerializeField] private bool isMine;

    [Header("HeroObject")]
    [SerializeField] private HeroObject heroObj;

    [Header("References")]
    [SerializeField] private Image heroImage;
    [SerializeField] private Text nameText;
    private CombatManager combatManager;

    private Animator anim;
    [SerializeField] private Image bgImage;

    private Color teamColor;
    private Color enemyColor;

    private bool doneStart = false;

    private void Start()
    {
        if (doneStart)
            return;

        combatManager = FindObjectOfType<CombatManager>();

        anim = GetComponent<Animator>();
        //bgImage = GetComponent<Image>();

        if (heroObj != null)
            LoadHeroObj();

        doneStart = true;
    }

    public void SetOwnership(bool newIsMine)
    {
        if (doneStart == false)
            Start();

        isMine = newIsMine;

        //Set this unit color based on if it is owned by the player
        Color colToUse;

        if (isMine == true)
            colToUse = combatManager.TeamColor;
        else
            colToUse = combatManager.EnemyColor;

        bgImage.color = colToUse;
    }

    public void SetHeroObj(HeroObject newHeroObj)
    {
        heroObj = newHeroObj;
        LoadHeroObj();
    }

    public void LoadHeroObj()
    {
        heroImage.sprite = heroObj.HeroSprite;
        nameText.text = heroObj.name;
    }

    public void OnClicked()
    {
        if (isMine)
            DoSpin();
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
}
