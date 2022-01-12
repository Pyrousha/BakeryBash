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
    private int basicAttackRange;
    public int BasicAttackRange => basicAttackRange;

    private int basicAttackDamage;
    public int BasicAttackDamage => basicAttackDamage;

    private int maxHp;
    private int hp;
    public int Hp => hp;

    [Header("References")]
    [SerializeField] private SpriteRenderer heroSpriteRend;
    [SerializeField] private Text nameText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Text hpText;

    private CombatManager combatManager;
    private PlayerControllerCombat playerController;
    private PlayerControllerCombat enemyPlayerController;

    [SerializeField] private SpriteRenderer bgSpriteRend;

    [Header("Parametes and such")]
    [SerializeField] private int startingVertexId = -1;
    private BoardVertex currVertex;
    public BoardVertex CurrVertex => currVertex;


    private bool doneStart = false;

    private void Start()
    {
        if (doneStart)
            return;

        combatManager = FindObjectOfType<CombatManager>();

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
        {
            colToUse = combatManager.EnemyColor;
            enemyPlayerController = newPlayer;
        }

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

        basicAttackRange = heroObj.BasicAttackRange;
        basicAttackDamage = heroObj.BasicAttackDamage;
        maxHp = heroObj.MaxHp;
        hp = maxHp;

        UpdateHPBar();
    }

    public void OnMouseDown()
    {
        if (isMine)
            playerController.OnHeroClicked(this, true);
        else
            enemyPlayerController.OnHeroClicked(this, false);
    }
       
    public void MoveToVertex(BoardVertex vertex)
    {
        if (currVertex != null)
            currVertex.SetCombatHero(null);

        currVertex = vertex;
        transform.position = vertex.transform.position + new Vector3(0,0,-0.2f);
        vertex.SetCombatHero(this);
    }

    [Command(requiresAuthority =false)]
    public void TakeDamageServer(int dmg)
    {
        if(dmg <= 0)
        {
            Debug.LogError("Attempting to deal " + dmg.ToString() + " damage to hero: " + heroObj.name);
            return;
        }

        hp = Mathf.Max(0, hp - dmg);

        UpdateHPClient(hp);

        if (hp <= 0)
            HeroKilledServer();
    }

    [ClientRpc]
    public void UpdateHPClient(int newHp)
    {
        hp = newHp;

        UpdateHPBar();

        if (hp <= 0)
            HeroKilledClient();
    }

    private void UpdateHPBar()
    {
        hpSlider.value = ((float)hp) / ((float)maxHp);
        hpText.text = hp.ToString() + "/" + maxHp.ToString();
    }

    public void HeroKilledServer()
    {

    }

    public void HeroKilledClient()
    {

    }
}
