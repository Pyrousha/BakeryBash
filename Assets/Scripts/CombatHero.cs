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

    [Header("Ingredient Stuff")]
    [SerializeField] private IngredientObject ingredientType;
    [SerializeField] private bool isIngredient = false;
    [SerializeField] private SpriteRenderer[] inventorySprites;
    public bool IsIngredient => isIngredient;
    private List<IngredientObject> inventory;
    public List<IngredientObject> Inventory => inventory;
    public bool IsInventoryFull()
    {
        return (inventory.Count >= 5);
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

    [Header("Parametes and such")]
    [SerializeField] private bool isTower = false;
    [SerializeField] private bool isCore = false;
    [SerializeField] private bool isInvincible = false;
    public bool IsInvincible => isInvincible;
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

        if (isTower)
        {
            if (isInvincible)
            {
                hpSlider.gameObject.SetActive(false);
                hpText.gameObject.SetActive(false);
            }

            trappedVertices = currVertex.AdjacentVertices;
            foreach(BoardVertex vert in trappedVertices)
            {
                vert.SetTower(this);
            }
        }
        else
        {
            if(isIngredient)
            {

            }
            else
            {
                //is a hero
                inventory = new List<IngredientObject>();
            }
        }

        doneStart = true;
    }

    public void SetOwnership(PlayerControllerCombat newPlayer, bool newIsMine)
    {
        if (doneStart == false)
            Start();

        isMine = newIsMine;

        if (isIngredient)
        {
            heroSpriteRend.sprite = ingredientType.GetSprite(newIsMine);

            isMine = false; //let anyone attack this

            enemyPlayerController = newPlayer;
            return;
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

        if (isTower)
            heroSpriteRend.color = colToUse;
        else
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
        canWalkOverSpecialTerrain = heroObj.CanWalkOverSpecialTerrain;
        maxHp = heroObj.MaxHp;
        hp = maxHp;

        UpdateHPBar();
    }

    public void OnMouseDown()
    {
        if (isMine) 
        {
            //player should not be able to click their own towers to attack
            if (!isTower)
                playerController.OnHeroClicked(this, true);
        }
        else
            //Since this is an enemy, need to get its "enemy" player controller to get the local player's controller
            enemyPlayerController.OnHeroClicked(this, false);
    }

    [ClientRpc]
    public void OnTurnStartTower()
    {
        if (!isTower)
            return; //this shouldn't be called, but just in case

        if (!isMine)
            return;

        if(gameObject.activeSelf)
            playerController.TryShootTower(this);
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
    public void TakeDamageServer(int dmg, Vector3 attackerPos)
    {
        if(dmg <= 0)
        {
            Debug.LogError("Attempting to deal " + dmg.ToString() + " damage to hero: " + heroObj.name);
            return;
        }

        if(isIngredient)
        {
            TryAddIngredientClient(attackerPos, ingredientType.Id);
            return;
        }

        hp = Mathf.Max(0, hp - dmg);

        UpdateHPClient(hp, attackerPos);

        if (hp <= 0)
            HeroKilledServer();
    }

    [ClientRpc]
    public void TryAddIngredientClient(Vector3 attackerPos, int ingredientId)
    {
        Vector3 dir = new Vector3(0, 0, 1);
        RaycastHit2D[] hitArr = Physics2D.RaycastAll(attackerPos - new Vector3(0, 0, 0.5f), dir, 2);
        Debug.DrawRay(attackerPos - new Vector3(0, 0, 1), dir, Color.red, 10f);

        Debug.Log("debugRay");

        foreach(RaycastHit2D hit in hitArr)
        {
            CombatHero tempHero = hit.collider.gameObject.GetComponent<CombatHero>();
            if (tempHero != null)
            {
                tempHero.AddIngredientToInventory(combatManager.GetIngredientWithId(ingredientId));
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
    public void UpdateHPClient(int newHp, Vector3 attackerPos)
    {
        //Play attack animation
        combatManager.SpawnAttackProjectile(!isMine, attackerPos, transform.position);

        //Update HP values
        hp = newHp;

        UpdateHPBar();

        if (hp <= 0)
            HeroKilledClient();

        enemyPlayerController.TryReclickCurrHero();
    }

    private void UpdateHPBar()
    {
        hpSlider.value = ((float)hp) / ((float)maxHp);
        hpText.text = hp.ToString() + "/" + maxHp.ToString();
    }

    public void HeroKilledServer()
    {
        if(isTower)
        {
            gameObject.SetActive(false);
        }
        else
        {

        }
    }

    public void HeroKilledClient()
    {
        if(isTower)
        {
            if (currVertex != null)
            {
                FindObjectOfType<GameBoardManager>().RemoveVertex(currVertex);
                if(nextTower!= null)
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

            gameObject.SetActive(false);
        }
        else
        {

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

        TakeDamageServer(tower.heroObj.BasicAttackDamage, tower.transform.position);
    }
}
