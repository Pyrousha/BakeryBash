using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.UI;

public class PlayerControllerCombat : NetworkBehaviour
{
    [SerializeField] private int playerNum;
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private GameBoardManager boardManager;
    private PlayerLobbyDetails playerLobbyDetails;

    private PlayerLobbyDetails enemyLobbyDetails;

    private CombatHero selectedHero;
    private List<BoardVertex> validMoveVertices;
    private List<BoardVertex> validAttackVertices;

    private int numCurrTokens;
    private int maxTokens = 15;

    private List<IngredientObject> ingredientInventory = new List<IngredientObject>();
    public List<IngredientObject> Inventory => ingredientInventory;
    
    [SerializeField] private List<IngredientObject> p2StartingInventory;

    [SerializeField] private Transform inventoryParent;
    [SerializeField] private Sprite transparentSprite;

    private List<Image> inventoryImages;

    [SerializeField] private int[] respawnIndices;

    private bool interactable = true;

    [SerializeField] private List<BakingObjectButton> bakingObjs;

    private ItemObject currItem = null;

    [SerializeField] private GameObject bakingOverlay;

    [SerializeField] private GameObject chooseHeroOverlay;
    [SerializeField] private Image chosenItemImage;

    //PNP Stuff
    [Header("PNP Stuff")]
    private HeroSelectController heroSelectController;
    private List<CombatHero> heroes;
    [SerializeField] private Color playerColor;

    private bool pnpMode = false;

    void Start()
    {
        if (PnPMode.Instance.IsPnpMode)
        {
            pnpMode = true;

            heroSelectController = HeroSelectController.Instance;

            //pnp start
            PnPStart();
        }
        else
        {
            //Multiplayer start
            MultiplayerStart();
        }
    }

    void PnPStart()
    {
        validMoveVertices = new List<BoardVertex>();
        validAttackVertices = new List<BoardVertex>();

        inventoryImages = new List<Image>();
        for (int i = 0; i < inventoryParent.childCount; i++)
        {
            inventoryImages.Add(inventoryParent.GetChild(i).GetComponent<Image>());
        }

        numCurrTokens = maxTokens;

        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localScale = new Vector3(1, 1, 1);

        List<int> heroIndices = new List<int>();

        if (playerNum == 1)
        {
            heroes = combatManager.GetP1Heroes;
            heroIndices = heroSelectController.P1HeroIndices;
        }
        else if (playerNum == 2)
        {
            heroes = combatManager.GetP2Heroes;
            heroIndices = heroSelectController.P2HeroIndices;

            ingredientInventory = p2StartingInventory;
        }

        //Set heroes to this player's selection
        for(int i = 0; i<heroes.Count; i++)
        {
            int heroIndex = heroIndices[i];
            heroes[i].SetHeroObj(HeroIndexToHeroObjectConverter.Instance.GetHeroOfIndex(heroIndex));
        }

        //Set hero color and ownership
        foreach (CombatHero hero in combatManager.GetP1Heroes)
        {
            hero.PNPSetOwnership(this, playerNum, playerNum == 1);
        }
        foreach (CombatHero hero in combatManager.GetP2Heroes)
        {
            hero.PNPSetOwnership(this, playerNum, playerNum == 2);
        }

        //Set tower color and ownership
        foreach (CombatHero tower in combatManager.GetP1Towers)
        {
            tower.PNPSetOwnership(this, playerNum, playerNum == 1);
        }
        foreach (CombatHero tower in combatManager.GetP2Towers)
        {
            tower.PNPSetOwnership(this, playerNum, playerNum == 2);
        }

        //Set map ingredient color
        foreach (CombatHero ingredient in combatManager.GetP1Ingredients)
        {
            ingredient.PNPSetOwnership(this, playerNum, playerNum == 1);
        }
        foreach (CombatHero ingredient in combatManager.GetP2Ingredients)
        {
            ingredient.PNPSetOwnership(this, playerNum, playerNum == 2);
        }

        //Set misc map object ownership (just deposits rn)
        foreach (CombatHero mapObj in combatManager.GetP1MapObjs)
        {
            mapObj.PNPSetOwnership(this, playerNum, playerNum == 1);
        }
        foreach (CombatHero mapObj in combatManager.GetP2MapObjs)
        {
            mapObj.PNPSetOwnership(this, playerNum, playerNum == 2);
        }

        //Set baking object references
        foreach (BakingObjectButton bobj in bakingObjs)
            bobj.SetControllerPlayerNum(playerNum, this);
    }

    [Client]
    // Start is called before the first frame update
    void MultiplayerStart()
    {
        validMoveVertices = new List<BoardVertex>();
        validAttackVertices = new List<BoardVertex>();

        inventoryImages = new List<Image>();
        for(int i=0; i< inventoryParent.childCount; i++)
        {
            inventoryImages.Add(inventoryParent.GetChild(i).GetComponent<Image>());
        }

        numCurrTokens = maxTokens;

        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localScale = new Vector3(1, 1, 1);

        if (playerNum == 1)
        {
            playerNum = 1;
            playerLobbyDetails = GameObject.Find("P1LobbyDetails").GetComponent<PlayerLobbyDetails>();
            enemyLobbyDetails = GameObject.Find("P2LobbyDetails").GetComponent<PlayerLobbyDetails>();
        }
        else if (playerNum == 2)
        {
            playerLobbyDetails = GameObject.Find("P2LobbyDetails").GetComponent<PlayerLobbyDetails>();
            enemyLobbyDetails = GameObject.Find("P1LobbyDetails").GetComponent<PlayerLobbyDetails>();

            if (playerLobbyDetails.isLocalPlayer)
            {
                ingredientInventory = p2StartingInventory;
                UpdatePlayerInventoryUI();
            }
        }

        if (playerLobbyDetails.isLocalPlayer)
        {
            SetHeroColors();
            combatManager.SetLocalPlayerNum(playerNum);

            foreach(BakingObjectButton bobj in bakingObjs)
            {
                bobj.SetController(this);
            }
        }
    }

    [Server]
    public void TurnStartedServer()
    {
        numCurrTokens = maxTokens;

        TurnStartedClient();
    }

    [ClientRpc]
    public void TurnStartedClient()
    {
        numCurrTokens = maxTokens;
        combatManager.UpdateTokenVisualCount(numCurrTokens);
    }

    [Client]
    public void SetHeroColors()
    {
        if (playerNum == 1)
        {
            //Set hero color and ownership
            for(int i=0; i< combatManager.GetP1Heroes.Count; i++)
            {
                CombatHero hero = combatManager.GetP1Heroes[i];
                hero.SetOwnership(this, true);
                int heroId = playerLobbyDetails.GetChosenHeroIdByIndex(i);
                hero.SetHeroObj(playerLobbyDetails.GetHeroById(heroId));
            }
            for (int i = 0; i < combatManager.GetP2Heroes.Count; i++)
            {
                CombatHero hero = combatManager.GetP2Heroes[i];
                hero.SetOwnership(this, false);
                int heroId = enemyLobbyDetails.GetChosenHeroIdByIndex(i);
                hero.SetHeroObj(enemyLobbyDetails.GetHeroById(heroId));
            }

            //Set tower color and ownership
            foreach(CombatHero tower in combatManager.GetP1Towers)
            {
                tower.SetOwnership(this, true);
            }
            foreach (CombatHero tower in combatManager.GetP2Towers)
            {
                tower.SetOwnership(this, false);
            }

            //Set map ingredient color
            foreach (CombatHero ingredient in combatManager.GetP1Ingredients)
            {
                ingredient.SetOwnership(this, true);
            }
            foreach (CombatHero ingredient in combatManager.GetP2Ingredients)
            {
                ingredient.SetOwnership(this, false);
            }

            //Set misc map object ownership (just deposits rn)
            foreach (CombatHero mapObj in combatManager.GetP1MapObjs)
            {
                mapObj.SetOwnership(this, true);
            }
            foreach (CombatHero mapObj in combatManager.GetP2MapObjs)
            {
                mapObj.SetOwnership(this, false);
            }

            return;
        }
        if (playerNum == 2)
        {
            //Set hero color and ownership
            for (int i = 0; i < combatManager.GetP1Heroes.Count; i++)
            {
                CombatHero hero = combatManager.GetP1Heroes[i];
                hero.SetOwnership(this, false);
                int heroId = enemyLobbyDetails.GetChosenHeroIdByIndex(i);
                hero.SetHeroObj(enemyLobbyDetails.GetHeroById(heroId));
            }
            for (int i = 0; i < combatManager.GetP2Heroes.Count; i++)
            {
                CombatHero hero = combatManager.GetP2Heroes[i];
                hero.SetOwnership(this, true);
                int heroId = playerLobbyDetails.GetChosenHeroIdByIndex(i);
                hero.SetHeroObj(playerLobbyDetails.GetHeroById(heroId));
            }

            //Set tower color and ownership
            foreach (CombatHero tower in combatManager.GetP1Towers)
            {
                tower.SetOwnership(this, false);
            }
            foreach (CombatHero tower in combatManager.GetP2Towers)
            {
                tower.SetOwnership(this, true);
            }

            //Set map ingredient color
            foreach (CombatHero ingredient in combatManager.GetP1Ingredients)
            {
                ingredient.SetOwnership(this, false);
            }
            foreach (CombatHero ingredient in combatManager.GetP2Ingredients)
            {
                ingredient.SetOwnership(this, true);
            }

            //Set misc map object ownership (just deposits rn)
            foreach (CombatHero mapObj in combatManager.GetP1MapObjs)
            {
                mapObj.SetOwnership(this, false);
            }
            foreach (CombatHero mapObj in combatManager.GetP2MapObjs)
            {
                mapObj.SetOwnership(this, true);
            }
        }
    }

    public void SetInteractable(bool newInteractable)
    {
        ResetHeroMoveVisuals();

        interactable = newInteractable;
    }

    [Client]
    public void OnHeroClicked(CombatHero newClickedHero, bool isMine)
    {
        if ((interactable == false) && (currItem == null))
            return;


        //Note: this is only called on the player's combatController

        Debug.Log("clicked on hero " + newClickedHero.gameObject.name);

        if (combatManager.PlayerTurn != playerNum)
        {
            Debug.Log("not my turn, exiting");
            return;
        }

        if (isMine)
        {
            ResetHeroMoveVisuals();

            if (currItem != null)
            {
                newClickedHero.AddStatsServer(currItem.Atk, currItem.Hp);
                chooseHeroOverlay.SetActive(false);
                currItem = null;
                interactable = true;
                return;
            }

            //Clicked on one of their own heroes, highlight adjacent vertices

            selectedHero = newClickedHero;

            List<BoardVertex> adjVertices = boardManager.GetWalkableEdges(newClickedHero.CurrVertex, selectedHero.CanWalkOverSpecialTerrain);

            foreach (BoardVertex vertex in adjVertices)
            {
                vertex.HighlightMove();
            }

            validMoveVertices = adjVertices;

            //Calculate valid attack vertices
            validAttackVertices = new List<BoardVertex>();

            //Get valid attack vertices (towers + heroes)
            List<BoardVertex> tempValidAttackVertices = GraphHelper.BFS(newClickedHero.CurrVertex, newClickedHero.BasicAttackRange);
            foreach(BoardVertex vert in tempValidAttackVertices)
            {
                if ((vert.combatHero != null) && (vert.combatHero.IsMine == false) && (vert.combatHero.IsInvincible == false))
                {
                    if ((vert.combatHero.type == CombatHero.HeroTypeEnum.Tower) || (vert.combatHero.type == CombatHero.HeroTypeEnum.Hero))
                    {
                        validAttackVertices.Add(vert);
                        vert.SetReticleActive(true, combatManager.AttackSpriteIcon);
                    }
                }
            }

            //Add ingredients and deposits to valid attack vertices
            tempValidAttackVertices = GraphHelper.BFS(newClickedHero.CurrVertex, 1);
            foreach (BoardVertex vert in tempValidAttackVertices)
            {
                if ((vert.combatHero != null) && (vert.combatHero.IsMine == false) && (vert.combatHero.IsInvincible == false))
                {
                    if (vert.combatHero.type == CombatHero.HeroTypeEnum.Ingredient)
                    {
                        if (selectedHero.IsInventoryFull() == false)
                        {
                            validAttackVertices.Add(vert); //only add ingredient if inventory is not full and vertex is adjacent
                            vert.SetReticleActive(true, combatManager.InteractSpriteIcon);
                        }
                    }
                    else if(vert.combatHero.type == CombatHero.HeroTypeEnum.Deposit)
                    {
                        if (selectedHero.IsInventoryEmpty() == false)
                        {
                            validAttackVertices.Add(vert); //Has ingredient to deposit
                            vert.SetReticleActive(true, combatManager.InteractSpriteIcon);
                        }
                    }
                }
            }
        }
        else
        {
            //Clicked on an enemy hero, try to see if attacking is possible

            if (selectedHero == null)
                return;

            CombatHero enemyHero = newClickedHero;

            BoardVertex enemyHeroVertex = enemyHero.CurrVertex;

            if(validAttackVertices.IndexOf(enemyHeroVertex) > -1)
            {
                //Enemy hero is in range, so attack is valid
                if (TryUseMoveTokens(1))
                {
                    switch(enemyHero.type)
                    {
                        case CombatHero.HeroTypeEnum.Ingredient:
                            {
                                if (selectedHero.IsInventoryFull() == false) //inventory not full, attack (add item to inventory) and use move token
                                {
                                    enemyHero.TakeDamageServer(0, newClickedHero.transform.position, selectedHero.transform.position);
                                }
                                else
                                {
                                    Debug.Log("inventory full, refunding token");
                                    //Inventory is full, refund spent move tokens and do not process attack
                                    numCurrTokens += 1;
                                    combatManager.UpdateTokenVisualCount(numCurrTokens);
                                }
                                break;
                            }

                        case CombatHero.HeroTypeEnum.Deposit:
                            {
                                //Check if hero had ingredients to deposit
                                if(selectedHero.IsInventoryEmpty() == false) //if(valid recipe)
                                {
                                    enemyHero.TakeDamageServer(0, selectedHero.transform.position, newClickedHero.transform.position);

                                    //depositing does not cost tokens
                                    numCurrTokens += 1;
                                    combatManager.UpdateTokenVisualCount(numCurrTokens);
                                }
                                else
                                {
                                    Debug.Log("inventory empty, cannot deposit, refunding token");
                                    //Inventory is full, refund spent move tokens and do not process attack
                                    numCurrTokens += 1;
                                    combatManager.UpdateTokenVisualCount(numCurrTokens);
                                }
                                break;
                            }

                        default:
                            {
                                //Hero or Tower
                                enemyHero.TakeDamageServer(selectedHero.BasicAttackDamage, selectedHero.transform.position, newClickedHero.transform.position);
                                break;
                            }
                    }
                }
                if (numCurrTokens == 0)
                    EndMyTurn();
            }
        }
    }

    public void TryShootTower(CombatHero tower)
    {
        List<BoardVertex> tempValidAttackVertices = GraphHelper.BFS(tower.CurrVertex, tower.BasicAttackRange);
        foreach (BoardVertex vert in tempValidAttackVertices)
        {
            CombatHero possibleEnemyHero = vert.combatHero;
            if ((possibleEnemyHero != null) && (possibleEnemyHero.IsMine == false) && (possibleEnemyHero.type == CombatHero.HeroTypeEnum.Hero))
            {
                //Hero is an enemy and is in range, attack!
                Debug.Log("tower with name "+tower.gameObject.name+" attacking player"+ possibleEnemyHero.gameObject.name+" at turn start");
                possibleEnemyHero.TakeDamageServer(tower.BasicAttackDamage, tower.transform.position, possibleEnemyHero.transform.position);
            }
        }
    }

    [Client]
    public void OnVertexClicked(BoardVertex vertex)
    {
        if (combatManager.PlayerTurn != playerNum)
            return;

        if (validMoveVertices.IndexOf(vertex) > -1)
        {
            int heroIndex = combatManager.GetIndexOfHero(playerNum, selectedHero);
            if (TryUseMoveTokens(1))
                ServerMoveHero(playerNum, heroIndex, vertex.VertexId);

            if (numCurrTokens == 0)
            {
                ResetHeroMoveVisuals();
                EndMyTurn();
            }
        }
    }

    [Client]
    public void TryReclickCurrHero()
    {
        if (selectedHero != null)
            TryReclickHero(selectedHero);
    }

    [Client]
    public void TryReclickHero(CombatHero hero)
    {
        if (combatManager.PlayerTurn != playerNum) 
            return;

        ResetHeroMoveVisuals();

        if (numCurrTokens > 0)
        {
            OnHeroClicked(hero, true);
        }
    }

    [Client]
    public void ResetHeroMoveVisuals()
    {
        Debug.Log("hero visuals reset");

        foreach (BoardVertex vert in validMoveVertices)
        {
            if (vert != null)
                vert.ResetColor();
        }

        foreach (BoardVertex vert in validAttackVertices)
        {
            if (vert != null)
                vert.SetReticleActive(false, null);
        }

        validMoveVertices = new List<BoardVertex>();
        validAttackVertices = new List<BoardVertex>();
    }

    [Client]
    private bool TryUseMoveTokens(int tokenCost)
    {
        if(numCurrTokens - tokenCost >= 0)
        {
            //player has enough tokens to do this move
            numCurrTokens -= tokenCost;
            combatManager.UpdateTokenVisualCount(numCurrTokens);

            return true;
        }
        return false;
    }

    [Client]
    public void EndMyTurn()
    {
        numCurrTokens = 0;
        combatManager.UpdateTokenVisualCount(numCurrTokens);

        ResetHeroMoveVisuals();

        bakingOverlay.SetActive(false);

        selectedHero = null;

        combatManager.DoneTurn();
    }

    [Command(requiresAuthority = false)]
    public void ServerMoveHero(int playerNum, int heroIndex, int newVertexId)
    {
        //CombatHero heroToMove = combatManager.GetHeroByIndex(playerNum, heroIndex);
        //BoardVertex vertex = boardManager.GetVertexWithId(newVertexId);
        //heroToMove.MoveToVertex(vertex);

        MoveHeroOnClients(playerNum, heroIndex, newVertexId);
    }

    [ClientRpc]
    public void MoveHeroOnClients(int playerNum, int heroIndex, int newVertexId)
    {
        CombatHero heroToMove = combatManager.GetHeroByIndex(playerNum, heroIndex);
        BoardVertex vertex = boardManager.GetVertexWithId(newVertexId);
        heroToMove.MoveToVertex(vertex);

        if(heroToMove.IsMine)
            TryReclickHero(heroToMove);
    }

    [Client]
    public void TryBake(ItemObject item)
    {
        if(item.HasIngredientsInInventory(ingredientInventory))
        {
            //Update inventory
            ingredientInventory = item.GetNewInventory();
            UpdatePlayerInventoryUI();

            //Close baking menu thingie
            bakingOverlay.SetActive(false);

            //Set the currently selected item
            currItem = item;
            
            //Don't allow the player to move any of their heroes/attack/etc.
            SetInteractable(false);

            //Show the overlay for "Who's hungry!"
            chooseHeroOverlay.SetActive(true);
            chosenItemImage.sprite = item.GetSprite;
        }
    }

    [Client]
    public void OpenBakeClicked()
    {
        if (playerLobbyDetails.isLocalPlayer == false)
            return;

        foreach(BakingObjectButton bakeObj in bakingObjs)
        {
            ItemObject item = bakeObj.Item;

            if (item.HasIngredientsInInventory(ingredientInventory))
            {
                bakeObj.SetVisualsFromInventory(item, true);
            }
            else
            {
                bakeObj.SetVisualsFromInventory(item, false);
            }
        }
    }

    [Client]
    public void AddIngredientToInventory(IngredientObject newIngredient)
    {
        ingredientInventory.Add(newIngredient);
        UpdatePlayerInventoryUI();
    }

    public void UpdatePlayerInventoryUI()
    {
        for(int i=0; i<inventoryImages.Count;i++)
        {
            if(ingredientInventory.Count > i)
            {
                inventoryImages[i].sprite = ingredientInventory[i].GetSprite(true);
            }
            else
            {
                inventoryImages[i].sprite = transparentSprite;
            }
        }
    }

    public void HeroKilled(CombatHero killedHero)
    {
        foreach(int vertIndex in respawnIndices)
        {
            BoardVertex vert = boardManager.GetVertexWithId(vertIndex);
            if (vert.combatHero == null)
            {
                //this spot is empty
                killedHero.MoveToVertex(vert);
                return;
            }
        }
    }
}
