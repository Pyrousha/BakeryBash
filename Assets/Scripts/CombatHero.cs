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

    private List<DotReticle> towerDots;

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

    //PNPStuff
    private bool doneStart = false;

    private List<PlayerControllerCombat> moveAuthPlayers;
    private List<PlayerControllerCombat> attackAuthPlayers;
    private List<PlayerControllerCombat> depositAuthPlayers;

    private PlayerControllerCombat allyPlayerController;

    [System.Serializable] public enum PlayerColorEnum
    { 
        Blue,
        Red
    }

    private PlayerColorEnum playerColor;

    public PlayerColorEnum AllyColor => playerColor;


    private void Start()
    {
        if (doneStart)
            return;

        moveAuthPlayers = new List<PlayerControllerCombat>();
        attackAuthPlayers = new List<PlayerControllerCombat>();
        depositAuthPlayers = new List<PlayerControllerCombat>();

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

                    currVertex.GetComponent<SpriteRenderer>().enabled = false; //Make any vertices with towers invisible

                    break;
                }
        }

        doneStart = true;
    }

    private bool moving;
    private List<BoardVertex> pathToMoveAlong;
    private Vector3 target;

    [SerializeField] private float moveSpd = 10;

    private void FixedUpdate()
    {
        if (moving == false)
            return;

        Vector3 dir = target - transform.position;

        float moveX;
        float moveY;
        //Move towards target
        if(dir.x < 0)
        {
            moveX = Mathf.Max(dir.x, dir.normalized.x * moveSpd);
        }
        else
        {
            moveX = Mathf.Min(dir.x, dir.normalized.x * moveSpd);
        }

        if(dir.y < 0)
        {
            moveY = Mathf.Max(dir.y, dir.normalized.y * moveSpd);
        }
        else
        {
            moveY = Mathf.Min(dir.y, dir.normalized.y * moveSpd);
        }

        transform.position += new Vector3(moveX, moveY, 0);

        //Debug.Log(dir.magnitude);

        //Get new target
        if ((dir).magnitude <= 0.5f)
        {
            transform.position = target;

            if(pathToMoveAlong.Count > 0)
            {
                target = pathToMoveAlong[0].transform.position;
                pathToMoveAlong.RemoveAt(0);
            }
            else
            {
                moving = false;
            }
        }
    }

    public bool HasMoveAuthority(PlayerControllerCombat currPlayer)
    {
        foreach (PlayerControllerCombat playerController in moveAuthPlayers)
        {
            if (playerController.GetInstanceID() == currPlayer.GetInstanceID())
                    return true;
        }
        return false;
    }
    public bool HasAttackAuthority(PlayerControllerCombat currPlayer)
    {
        foreach (PlayerControllerCombat playerController in attackAuthPlayers)
        {
            if (playerController.GetInstanceID() == currPlayer.GetInstanceID())
                return true;
        }
        return false;
    }

    public bool HasDepositAuthority(PlayerControllerCombat currPlayer)
    {
        foreach (PlayerControllerCombat playerController in depositAuthPlayers)
        {
            if (playerController.GetInstanceID() == currPlayer.GetInstanceID())
                return true;
        }
        return false;
    }

    public void PNPSetOwnership(PlayerControllerCombat newPlayer, int playerNum, bool onMySide)
    {
        if (doneStart == false)
            Start();

        if (onMySide)
            allyPlayerController = newPlayer;

        Color colToUse = new Color();

        //Set color
        if (onMySide)
        {
            if (playerNum == 1)
            {
                playerColor = PlayerColorEnum.Blue;
                colToUse = combatManager.TeamColor;
            }
            else
            {
                playerColor = PlayerColorEnum.Red;
                colToUse = combatManager.EnemyColor;
            }
        }

        switch (heroType)
        {
            case HeroTypeEnum.Hero:
                {
                    if (onMySide)
                    {
                        moveAuthPlayers.Add(newPlayer);
                        bgSpriteRend.color = colToUse;
                    }
                    else
                    {
                        attackAuthPlayers.Add(newPlayer);
                    }
                    break;
                }
            case HeroTypeEnum.Ingredient:
                {
                    //let anyone attack this
                    attackAuthPlayers.Add(newPlayer);

                    //Set sprite color based on if on ally or enemy side
                    if(onMySide)
                        heroSpriteRend.sprite = ingredientType.GetSprite(playerNum == 1);
                    break;
                }
            case HeroTypeEnum.Tower:
                {
                    //Allow player to attack enemy towers
                    if (onMySide == false)
                        attackAuthPlayers.Add(newPlayer);


                    //Set color, then calculate range and spawn dot icons
                    if (onMySide) //only want to do this part once per tower
                    {
                        //Check which color this tower should be
                        if (playerNum == 1) //blue side
                            heroSpriteRend.sprite = allySprite;
                        else
                            heroSpriteRend.sprite = enemySprite;


                        towerDots = new List<DotReticle>();

                        //Set tower range icons
                        List<BoardVertex> tempValidAttackVertices = GraphHelper.BFS(currVertex, basicAttackRange);
                        foreach (BoardVertex vert in tempValidAttackVertices) //all vertices in 2 spaces of tower
                        {
                            if ((vert.VertexId == currVertex.VertexId) || ((vert.combatHero != null) && (vert.combatHero.heroType == HeroTypeEnum.Deposit)))
                            {
                                continue;
                            }

                            DotReticle newDot = Instantiate(combatManager.dotReticle).GetComponent<DotReticle>();
                            newDot.SetVertex(vert);
                            newDot.SetColor(colToUse);

                            towerDots.Add(newDot);
                        }

                        //Make all adjacent vertices of towers pointy
                        foreach (BoardVertex vert in currVertex.AdjacentVertices)
                        {
                            foreach (DotReticle dot in towerDots)
                            {
                                if (dot.Vertex == vert)
                                {
                                    dot.SprRend.sprite = combatManager.circleReticleSprite;
                                }
                            }
                        }
                    }

                    break;
                }
            case HeroTypeEnum.Deposit:
                {
                    //Deposit on player's side of the map, need to attack local side instead of enemy's
                    if (onMySide)
                    {
                        depositAuthPlayers.Add(newPlayer);

                        if (playerNum == 1) //blue side
                            heroSpriteRend.sprite = allySprite;
                        else
                            heroSpriteRend.sprite = enemySprite;
                    }

                    break;
                }
        }
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

                    towerDots = new List<DotReticle>();

                    //Set tower range icons
                    List<BoardVertex> tempValidAttackVertices = GraphHelper.BFS(currVertex, basicAttackRange);
                    foreach (BoardVertex vert in tempValidAttackVertices) //all vertices in 2 spaces of tower
                    {
                        if ((vert.VertexId == currVertex.VertexId) || ((vert.combatHero != null) && (vert.combatHero.heroType == HeroTypeEnum.Deposit)))
                        {
                            continue;
                        }

                        DotReticle newDot = Instantiate(combatManager.dotReticle).GetComponent<DotReticle>();
                        newDot.SetVertex(vert);
                        newDot.SetColor(colToUse);

                        towerDots.Add(newDot);
                    }

                    //Make all adjacent vertices of towers pointy
                    foreach (BoardVertex vert in currVertex.AdjacentVertices)
                    {
                        foreach(DotReticle dot in towerDots)
                        {
                            if(dot.Vertex == vert)
                            {
                                dot.SprRend.sprite = combatManager.circleReticleSprite;
                            }
                        }
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

    public void PNPAddStats(int atkA, int hpA)
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

    //public void OnMouseDown()
    public void OnClicked()
    {
        Debug.Log("MouseDown on hero " + heroObj.name);

        if(PnPMode.Instance.IsPnpMode)
        {
            PlayerControllerCombat currPlayer = combatManager.GetCurrPlayerController();
            currPlayer.PNPHeroClicked(this, currPlayer == allyPlayerController);

            return;
        }

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

    public void PNPOnTurnStartTower()
    {
        if (heroType != HeroTypeEnum.Tower)
            return; //this shouldn't be called, but just in case

        if (gameObject.activeSelf)
            combatManager.GetCurrPlayerController().TryShootTower(this);
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

    public void PNPTryRespawn()
    {
        if (isDead && heroType == HeroTypeEnum.Hero)
        {
            hp += Mathf.CeilToInt(((float)maxHp) / 2f);
            if (hp >= maxHp)
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

    public void MoveToVertexPath(List<BoardVertex> path)
    {
        BoardVertex endVertex = path[path.Count - 1];
        if (currVertex != null)
            currVertex.SetCombatHero(null);

        currVertex = endVertex;
        endVertex.SetCombatHero(this);

        pathToMoveAlong = new List<BoardVertex>(path);
        moving = true;
        target = pathToMoveAlong[0].transform.position;
        pathToMoveAlong.RemoveAt(0);
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

    public void PNPTakeDamage(int dmg, Vector3 attackerPos, Vector3 targetPos)
    {
        switch (heroType)
        {
            case HeroTypeEnum.Ingredient:
                {
                    PNPTryAddIngredient(attackerPos, targetPos, ingredientType.Id);
                    break;
                }

            case HeroTypeEnum.Deposit:
                {
                    PNPTryTakeIngredient(attackerPos, targetPos);
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

                    PNPUpdateHP(hp, attackerPos, targetPos);

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

    public void PNPTryAddIngredient(Vector3 attackerPos, Vector3 targetPos, int ingredientId)
    {
        Vector3 dir = new Vector3(0, 0, 1);
        RaycastHit2D[] hitArr = Physics2D.RaycastAll(targetPos - new Vector3(0, 0, 0.5f), dir, 2);
        Debug.DrawRay(targetPos - new Vector3(0, 0, 1), dir, Color.red, 10f);

        Debug.Log("debugRay");

        foreach (RaycastHit2D hit in hitArr)
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
                break;
            }
        }

        if (enemyPlayerController != null)
            enemyPlayerController.TryReclickCurrHero();
    }

    public void PNPTryTakeIngredient(Vector3 attackerPos, Vector3 targetPos)
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

                combatManager.GetCurrPlayerController().AddIngredientToInventory(newIngredient);

                combatManager.SpawnInteractProjectile(newIngredient.Id, attackerPos, targetPos);
                break;
            }
        }

        if (enemyPlayerController != null)
            enemyPlayerController.TryReclickCurrHero();
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

    public void PNPUpdateHP(int newHp, Vector3 attackerPos, Vector3 targetPos)
    {
        //Play attack animation
        combatManager.SpawnAttackProjectile(combatManager.PlayerTurn == 1, attackerPos, targetPos);

        //Update HP values
        hp = newHp;

        UpdateHPBar();

        if (hp <= 0)
            PNPHeroKilled();

        combatManager.GetCurrPlayerController().PNPTryReclickCurrHero();
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
                        vert.RemoveTower();
                    }

                    foreach (DotReticle dot in towerDots)
                    {
                        dot.gameObject.SetActive(false);
                    }

                    gameObject.SetActive(false);
                    break;
                }
        }
    }

    public void PNPHeroKilled()
    {
        switch (heroType)
        {
            case HeroTypeEnum.Hero:
                {
                    allyPlayerController.HeroKilled(this);

                    allyPlayerController.ResetHeroMoveVisuals();

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
                                combatManager.PNPEndGame(playerColor == PlayerColorEnum.Red);
                        }
                    }

                    foreach (BoardVertex vert in trappedVertices)
                    {
                        vert.RemoveTower();
                    }

                    foreach (DotReticle dot in towerDots)
                    {
                        dot.gameObject.SetActive(false);
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

    public void PNPSteppedOnTrappedVertex(CombatHero tower)
    {
        if (playerColor == tower.AllyColor)
            return; //ally tower should not attack same team

        //Only process this on the client which owns this hero (just so no duplicate calls happen, idk what I'm doin lmaoooo)

        Debug.Log("stepped on trapped vertex!");

        PNPTakeDamage(tower.heroObj.BasicAttackDamage, tower.transform.position, transform.position);
    }
}
