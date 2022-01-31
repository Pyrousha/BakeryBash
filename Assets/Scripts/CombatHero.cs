using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;

public class CombatHero : NetworkBehaviour
{
    [SerializeField] private bool isMine;
    public bool IsMine => isMine;

    [Header("HeroObject")]
    [SerializeField] private HeroObject heroObj;
    private int basicAttackRange;
    public int BasicAttackRange => basicAttackRange;

    private int basicAttackDamage;
    public int BasicAttackDamage => basicAttackDamage;

    private int maxHp;
    private int hp;
    public int Hp => hp;
    private bool canWalkOverSpecialTerrain;
    public bool CanWalkOverSpecialTerrain => canWalkOverSpecialTerrain;

    [SerializeField] private Text atkText;

    [Header("Data")]
    [SerializeField] private Sprite allySprite;
    [SerializeField] private Sprite enemySprite;

    [Header("Ingredient Stuff")]
    [SerializeField] private IngredientObject ingredientType;
    //[SerializeField] private bool isIngredient = false;
    [SerializeField] private SpriteRenderer[] inventorySprites;
    //public bool IsIngredient => heroType == HeroTypeEnum.Ingredient;
    private List<IngredientObject> inventory;
    //public List<IngredientObject> Inventory => inventory;
    public bool IsInventoryFull()
    {
        return (inventory.Count >= 5);
    }
    public bool IsInventoryEmpty()
    {
        return (inventory.Count == 0);
    }
    public IngredientObject PopIngredient()
    {
        Debug.Log("inv count: " + inventory.Count);
        Debug.Log("inv[count-1] = inv[" + (inventory.Count - 1) + "] = ");
        Debug.Log(inventory[inventory.Count - 1].name);

        IngredientObject ingredient = inventory[inventory.Count - 1];
        inventory.RemoveAt(inventory.Count - 1);
        return (ingredient);
    }

    [Header("References")]
    [SerializeField] private SpriteRenderer heroSpriteRend;
    [SerializeField] private Text nameText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Text hpText;

    private CombatManager combatManager;
    private PlayerControllerCombat playerController;
    private PlayerControllerCombat enemyPlayerController;

    [SerializeField] private SpriteRenderer bgSpriteRend;

    private List<SpriteRenderer> towerIconSprites;

    //[Header("Parametes and such")]
    [Serializable] public enum HeroTypeEnum
    {
        Hero,
        Tower,
        Ingredient,
        Deposit
    }
    public HeroTypeEnum type => heroType;

    [Header("Parametes and such")]
    [SerializeField] private bool disableAllyHitbox = false;
    [SerializeField] private HeroTypeEnum heroType;

    //[SerializeField] private bool isTower = false;
    [SerializeField] private bool isCore = false;
    [SerializeField] private bool isInvincible = false;
    public bool IsInvincible => isInvincible;
    private bool isDead = false;
    [SerializeField] private CombatHero nextTower;
    private List<BoardVertex> trappedVertices;

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

        switch(heroType)
        {
            case HeroTypeEnum.Hero:
                {
                    inventory = new List<IngredientObject>();
                    break;
                }

            case HeroTypeEnum.Tower:
                {
                    if (isInvincible)
                    {
                        hpSlider.gameObject.SetActive(false);
                        hpText.gameObject.SetActive(false);
                    }

                    trappedVertices = currVertex.AdjacentVertices;
                    foreach (BoardVertex vert in trappedVertices)
                    {
                        vert.SetTower(this);
                    }
                    break;
                }
        }

        doneStart = true;
    }

    public void SetOwnership(PlayerControllerCombat newPlayer, bool newIsMine)
    {
        if (doneStart == false)
            Start();

        isMine = newIsMine;

        if (heroType == HeroTypeEnum.Ingredient)
        {
            isMine = false; //let anyone attack this
        }

        if (heroType == HeroTypeEnum.Deposit)
        {   
            //Deposit on player's side of the map, need to attack local side instead of enemy's
            isMine = !isMine;
        }

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

        switch (heroType)
        {
            case HeroTypeEnum.Hero:
                {
                    bgSpriteRend.color = colToUse;
                    break;
                }
            case HeroTypeEnum.Ingredient:
                {
                    //Set sprite color based on if on ally or enemy side
                    heroSpriteRend.sprite = ingredientType.GetSprite(newIsMine);
                    break;
                }
            case HeroTypeEnum.Tower:
                {
                    if (isCore && isMine && disableAllyHitbox)
                        GetComponent<BoxCollider2D>().enabled = false;

                    if (isMine) //ally side
                        heroSpriteRend.sprite = allySprite;
                    else
                        heroSpriteRend.sprite = enemySprite;

                    towerIconSprites = new List<SpriteRenderer>();

                    //Set tower range icons
                    CombatHero tower = this;
                    List<BoardVertex> tempValidAttackVertices = GraphHelper.BFS(tower.CurrVertex, tower.BasicAttackRange);
                    foreach (BoardVertex vert in tempValidAttackVertices)
                    {
                        if(vert.TowerRangeIcon == null)
                        {
                            SpriteRenderer newIcon = Instantiate(combatManager.dotReticle).GetComponent<SpriteRenderer>();
                            towerIconSprites.Add(newIcon);
                            vert.SetTowerRangeIcon(newIcon, colToUse);
                        }
                    }

                    foreach (BoardVertex vert in currVertex.AdjacentVertices)
                    {
                        vert.ReplaceTowerRangeIcon(combatManager.circleReticleSprite);
                    }

                    break;
                }
            case HeroTypeEnum.Deposit:
                {
                    if (disableAllyHitbox && isMine) //local player can't attack this (enemy side deposit, remove hitbox to click on core)
                    {
                        GetComponent<BoxCollider2D>().enabled = false;
                    }

                    if (newIsMine) //ally side
                        heroSpriteRend.sprite = allySprite;
                    else
                        heroSpriteRend.sprite = enemySprite;
                    break;
                }
        }
    }

    [Command(requiresAuthority =false)]
    public void AddStatsServer(int atkA, int hpA)
    {
        AddStatsClient(atkA, hpA);
    }

    [ClientRpc]
    public void AddStatsClient(int atkA, int hpA)
    {
        basicAttackDamage += atkA;
        maxHp += hpA;
        hp += hpA;

        atkText.text = BasicAttackDamage.ToString();

        UpdateHPBar();
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
        canWalkOverSpecialTerrain = heroObj.CanWalkOverSpecialTerrain;
        maxHp = heroObj.MaxHp;
        hp = maxHp;

        if (atkText != null)
            atkText.text = BasicAttackDamage.ToString();

        UpdateHPBar();
    }

    public void OnMouseDown()
    {
        if (isDead)
            return;

        if (isMine) 
        {
            //player should not be able to click their own towers or ovens to attack
            if (heroType == HeroTypeEnum.Hero)
                playerController.OnHeroClicked(this, true);
        }
        else
            //Since this is an enemy, need to get its "enemy" player controller to get the local player's controller
            enemyPlayerController.OnHeroClicked(this, false);
    }

    [ClientRpc]
    public void OnTurnStartTower()
    {
        if (heroType != HeroTypeEnum.Tower)
            return; //this shouldn't be called, but just in case

        if (!isMine)
            return;

        if(gameObject.activeSelf)
            playerController.TryShootTower(this);
    }

    [ClientRpc]
    public void TryRespawn()
    {
        if(isDead && heroType == HeroTypeEnum.Hero)
        {
            hp += Mathf.CeilToInt(((float)maxHp) / 2f);
            if(hp >= maxHp)
            {
                hp = maxHp;
                //healed enough to respawn
                isInvincible = false;
                isDead = false;
                heroSpriteRend.enabled = true;
            }

            UpdateHPBar();
        }
    }
       
    
    public void MoveToVertex(BoardVertex vertex)
    {
        if (currVertex != null)
            currVertex.SetCombatHero(null);

        currVertex = vertex;
        transform.position = vertex.transform.position + new Vector3(0,0,-0.2f);
        vertex.SetCombatHero(this);
    }

    [Command(requiresAuthority = false)]
    public void TakeDamageServer(int dmg, Vector3 attackerPos, Vector3 targetPos)
    {
        switch(heroType)
        {
            case HeroTypeEnum.Ingredient:
                {
                    TryAddIngredientClient(attackerPos, targetPos, ingredientType.Id);
                    break;
                }

            case HeroTypeEnum.Deposit:
                {
                    TryTakeIngredientClient(attackerPos, targetPos);
                    break;
                }
            
            default:
                {
                    //Hero or tower, process damage

                    if (dmg <= 0)
                    {
                        Debug.LogError("Attempting to deal " + dmg.ToString() + " damage to hero: " + heroObj.name);
                        return;
                    }

                    hp = Mathf.Max(0, hp - dmg);

                    UpdateHPClient(hp, attackerPos, targetPos);

                    if (hp <= 0)
                        HeroKilledServer();
                    break;
                }
        }
    }

    [ClientRpc]
    public void TryAddIngredientClient(Vector3 attackerPos, Vector3 targetPos, int ingredientId)
    {
        Vector3 dir = new Vector3(0, 0, 1);
        RaycastHit2D[] hitArr = Physics2D.RaycastAll(targetPos - new Vector3(0, 0, 0.5f), dir, 2);
        Debug.DrawRay(targetPos - new Vector3(0, 0, 1), dir, Color.red, 10f);

        Debug.Log("debugRay");

        foreach(RaycastHit2D hit in hitArr)
        {
            CombatHero tempHero = hit.collider.gameObject.GetComponent<CombatHero>();
            if (tempHero != null)
            {
                tempHero.AddIngredientToInventory(combatManager.GetIngredientWithId(ingredientId));

                combatManager.SpawnInteractProjectile(ingredientId, attackerPos, targetPos);
                return;
            }
        }
    }

    public void AddIngredientToInventory(IngredientObject newIngredient)
    {
        if (IsInventoryFull() == false)
            inventory.Add(newIngredient);
        else
            Debug.Log("inventory already full!");

        Debug.Log(newIngredient.name + " added to inventory");
        if(playerController != null)
            playerController.TryReclickCurrHero();

        UpdateInventoryUI();
    }

    [ClientRpc]
    public void TryTakeIngredientClient(Vector3 attackerPos, Vector3 targetPos)
    {
        Vector3 dir = new Vector3(0, 0, 1);
        RaycastHit2D[] hitArr = Physics2D.RaycastAll(attackerPos - new Vector3(0, 0, 0.5f), dir, 2);

        foreach (RaycastHit2D hit in hitArr)
        {
            CombatHero tempHero = hit.collider.gameObject.GetComponent<CombatHero>();
            if (tempHero != null)
            {
                IngredientObject newIngredient = tempHero.PopIngredient();
                tempHero.UpdateInventoryUI();

                if(enemyPlayerController != null) //only update local player inventory
                    enemyPlayerController.AddIngredientToInventory(newIngredient);

                combatManager.SpawnInteractProjectile(newIngredient.Id, attackerPos, targetPos);
                return;
            }
        }
    }

    public void UpdateInventoryUI()
    {
        for(int i=0; i<inventorySprites.Length;i++)
        {
            Sprite tempSprite = null;
            if(inventory.Count > i)
            {
                tempSprite = inventory[i].GetInventorySprite();
            }

            inventorySprites[i].sprite = tempSprite;
        }
    }

    [ClientRpc]
    public void UpdateHPClient(int newHp, Vector3 attackerPos, Vector3 targetPos)
    {
        //Play attack animation
        combatManager.SpawnAttackProjectile(!isMine, attackerPos, targetPos);

        //Update HP values
        hp = newHp;

        UpdateHPBar();

        if (hp <= 0)
            HeroKilledClient();

        if(enemyPlayerController != null)
            enemyPlayerController.TryReclickCurrHero();
    }

    private void UpdateHPBar()
    {
        hpSlider.value = ((float)hp) / ((float)maxHp);
        hpText.text = hp.ToString() + "/" + maxHp.ToString();
    }

    public void HeroKilledServer()
    {
        switch(heroType)
        {
            case HeroTypeEnum.Tower:
                {
                    gameObject.SetActive(false);
                    break;
                }
        }
    }

    public void HeroKilledClient()
    {
        switch(heroType)
        {
            case HeroTypeEnum.Hero:
                {
                    if (playerController == null)
                    {
                        int enemyPlayerNum = (combatManager.LocalPlayerNum == 1) ? 2 : 1;
                        playerController = combatManager.GetController(enemyPlayerNum);
                    }
                    playerController.HeroKilled(this);

                    playerController.ResetHeroMoveVisuals();

                    heroSpriteRend.enabled = false;
                    isInvincible = true;
                    isDead = true;
                    break;
                }

            case HeroTypeEnum.Tower:
                {
                    if (currVertex != null)
                    {
                        FindObjectOfType<GameBoardManager>().RemoveVertex(currVertex);
                        if (nextTower != null)
                            nextTower.PrevTowerDestroyed();
                        else
                        {
                            if (isCore)
                                combatManager.EndGame(isMine);
                        }
                    }

                    foreach (BoardVertex vert in trappedVertices)
                    {
                        vert.SetTower(null);
                    }

                    foreach (SpriteRenderer sprRend in towerIconSprites)
                    {
                        sprRend.gameObject.SetActive(false);
                    }

                    gameObject.SetActive(false);
                    break;
                }
        }
    }

    public void PrevTowerDestroyed()
    {
        isInvincible = false;
        hpSlider.gameObject.SetActive(true);
        hpText.gameObject.SetActive(true);
    }

    [Client]
    public void SteppedOnTrappedVertex(CombatHero tower)
    {
        if ((!isClient) || (!isMine))
            return;

        if (isMine == tower.IsMine)
            return; //ally tower should not attack

        //Only process this on the client which owns this hero (just so no duplicate calls happen, idk what I'm doin lmaoooo)

        Debug.Log("stepped on trapped vertex!");

        TakeDamageServer(tower.heroObj.BasicAttackDamage, tower.transform.position, transform.position);
    }
}
