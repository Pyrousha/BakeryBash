using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CombatManager : NetworkBehaviour
{
    public static CombatManager Instance;

    [SerializeField] private int playerTurn = 1;
    public int PlayerTurn => playerTurn;
    private int localPlayerNum;
    public int LocalPlayerNum => localPlayerNum;
    public void SetLocalPlayerNum(int newNum)
    {
        localPlayerNum = newNum;

        UpdateTurnIndicator();
        SetTokenColor();
        endTurnButton.SetActive(playerTurn == localPlayerNum);
    }

    [Header("Data")]
    [SerializeField] private IngredientObject[] ingredients;
    public IngredientObject GetIngredientWithId(int id)
    {
        return ingredients[id];
    }

    [SerializeField] private Sprite attackSpriteIcon;
    [SerializeField] private Sprite interactSpriteIcon;

    public Sprite AttackSpriteIcon => attackSpriteIcon;
    public Sprite InteractSpriteIcon => interactSpriteIcon;

    [Header("References")]
    [SerializeField] private PlayerControllerCombat p1Controller;
    [SerializeField] private PlayerControllerCombat p2Controller;

    [SerializeField] private List<CombatHero> p1Heroes;
    [SerializeField] private List<CombatHero> p2Heroes;

    [SerializeField] private List<CombatHero> p1Towers;
    [SerializeField] private List<CombatHero> p2Towers;
    public List<CombatHero> GetP1Towers => p1Towers;
    public List<CombatHero> GetP2Towers => p2Towers;

    [SerializeField] private List<CombatHero> p1Ingredients;
    [SerializeField] private List<CombatHero> p2Ingredients;
    public List<CombatHero> GetP1Ingredients => p1Ingredients;
    public List<CombatHero> GetP2Ingredients => p2Ingredients;

    [SerializeField] private BoxCollider2D blueCoreDepositHitbox;
    [SerializeField] private BoxCollider2D redCoreDepositHitbox;
    [SerializeField] private BoxCollider2D blueCoreTowerHitbox;
    [SerializeField] private BoxCollider2D redCoreTowerHitbox;

    [SerializeField] private List<CombatHero> p1MapObjs;
    [SerializeField] private List<CombatHero> p2MapObjs;
    public List<CombatHero> GetP1MapObjs => p1MapObjs;
    public List<CombatHero> GetP2MapObjs => p2MapObjs;

    [SerializeField] private Transform tokenSpriteParent;
    [SerializeField] private GameObject endTurnButton;

    [SerializeField] private GameObject winOverlay;
    [SerializeField] private GameObject loseOverlay;

    public List<CombatHero> GetP1Heroes => p1Heroes;
    public List<CombatHero> GetP2Heroes => p2Heroes;

    [SerializeField] private Image turnColorImg;
    [SerializeField] private Text turnText;

    [Header("Prefabs")]
    [SerializeField] private GameObject blueAttackProjectile;
    [SerializeField] private GameObject redAttackProjectile;
    [SerializeField] private GameObject interactProjectile;
    [SerializeField] private GameObject vertexArrow;
    public GameObject VertexArrow => vertexArrow;

    public Sprite circleReticleSprite;
    public GameObject dotReticle;

    [Header("Colors")]
    [SerializeField] private Color teamColor;
    [SerializeField] private Color enemyColor;
    public Color TeamColor => teamColor;
    public Color EnemyColor => enemyColor;

    [SerializeField] private Color playerTokenColor;
    [SerializeField] private Color enemyTokenColor;

    [Header("PNP Stuff")]
    private PlayerControllerCombat currPlayerController;
    [SerializeField] private GameObject blueWinOverlay;
    [SerializeField] private GameObject redWinOverlay;

    private void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            Debug.LogError("Multiple Combatmanagers found");

        if(PnPMode.Instance.IsPnpMode)
        {
            //Game is in PNP mode

            localPlayerNum = 1; //Blue side will be player1 (left), this value is only used for colors
            playerTurn = 1;

            SetTokenColor();

            currPlayerController = p1Controller;
        }
    }

    [Command(requiresAuthority = false)]
    public void DoneTurn()
    {
        playerTurn = (playerTurn == 1) ? 2 : 1;

        SetPlayerTurnOnClient(playerTurn);

        SetTokenColorClient();

        GetCurrPlayerController().TurnStartedServer();

        //Try to shoot at all enemies that are in the new player's tower range
        if (playerTurn == 1)
        {
            TryRespawnHeroes(p1Heroes);
            FireTowers(p1Towers);
        }
        else
        {
            TryRespawnHeroes(p2Heroes);
            FireTowers(p2Towers);
        }

        //Get rid of enemy's core deposit to ensure it can be clicked on
        if(playerTurn == 1)
        {
            blueCoreTowerHitbox.enabled = false;
            blueCoreDepositHitbox.enabled = true;

            redCoreTowerHitbox.enabled = true;
            redCoreDepositHitbox.enabled = false;
        }
        else
        {
            blueCoreTowerHitbox.enabled = true;
            blueCoreDepositHitbox.enabled = false;

            redCoreTowerHitbox.enabled = false;
            redCoreDepositHitbox.enabled = true;
        }
    }

    public void PNPDoneTurn()
    {
        playerTurn = (playerTurn == 1) ? 2 : 1;

        PNPUpdateTurnIndicator();
        SetTokenColor();

        currPlayerController = GetCurrPlayerController();
        currPlayerController.UpdatePlayerInventoryUI();

        //Try to shoot at all enemies that are in the new player's tower range
        if (playerTurn == 1)
        {
            PNPTryRespawnHeroes(p1Heroes);
            PNPFireTowers(p1Towers);
        }
        else
        {
            PNPTryRespawnHeroes(p2Heroes);
            PNPFireTowers(p2Towers);
        }

        currPlayerController.PNPTurnStarted();
    }

    [ClientRpc]
    public void SetPlayerTurnOnClient(int playerTurnNum)
    {
        playerTurn = playerTurnNum;

        UpdateTurnIndicator();
        endTurnButton.SetActive(playerTurn == localPlayerNum);
    }

    public PlayerControllerCombat GetCurrPlayerController()
    {
        if (playerTurn == 1)
        {
            return p1Controller;
        }
        else
        {
            return p2Controller;
        }
    }

    public PlayerControllerCombat GetController(int playerNum)
    {
        if (playerNum == 1)
        {
            return p1Controller;
        }
        else
        {
            return p2Controller;
        }
    }

    public int GetIndexOfHero(int playerNum, CombatHero hero)
    {
        if (playerNum == 1)
        {
            return p1Heroes.IndexOf(hero);
        }
        else
        {
            return p2Heroes.IndexOf(hero);
        }
    }
        

    public CombatHero GetHeroByIndex(int playerNum, int index)
    {
        if(playerNum == 1)
        {
            return p1Heroes[index];
        }
        else
        {
            return p2Heroes[index];
        }
    }

    public void UpdateTurnIndicator()
    {
        bool isMyTurn = (localPlayerNum == playerTurn);

        if (isMyTurn)
        {
            turnColorImg.color = teamColor;
            turnText.text = "Your move!";
        }
        else
        {
            turnColorImg.color = enemyColor;
            turnText.text = "Enemy's turn";
        }
    }

    public void PNPUpdateTurnIndicator()
    {
        bool isBlueTurn = (localPlayerNum == playerTurn);

        if (isBlueTurn)
        {
            turnColorImg.color = teamColor;
            turnText.text = "P1's Turn!";
        }
        else
        {
            turnColorImg.color = enemyColor;
            turnText.text = "P2's Turn!";
        }
    }

    public void EndTurnButtonClicked()
    {
        if ((localPlayerNum != playerTurn) && (PnPMode.Instance.IsPnpMode == false))
            return; //not my turn

        GetCurrPlayerController().EndMyTurn();
    }

    [Command(requiresAuthority = false)]
    public void UpdateTokenVisualCountServer(int numCurrTokens)
    {
        UpdateTokenVisualCountClient(numCurrTokens);
    }

    [ClientRpc]
    public void UpdateTokenVisualCountClient(int numCurrTokens)
    {
        /*for (int i = 0; i < tokenSpriteParent.childCount; i++)
        {
            tokenSpriteParent.GetChild(i).gameObject.SetActive(i < numCurrTokens);
        }*/

        UpdateTokenVisualCount(numCurrTokens);
    }

    public void UpdateTokenVisualCount(int numCurrTokens)
    {
        for (int i = 0; i < tokenSpriteParent.childCount; i++)
        {
            tokenSpriteParent.GetChild(i).gameObject.SetActive(i < numCurrTokens);
        }
    }

    public void UpdateAboutToUseTokenVisualCount(int totalTokensLeft, int tokensAboutToUse)
    {
        SetTokenColor();

        for (int i = 0; i < tokenSpriteParent.childCount; i++)
        {
            tokenSpriteParent.GetChild(i).gameObject.SetActive(i < totalTokensLeft);
        }

        int litTokensLeft = totalTokensLeft - tokensAboutToUse;

        for (int i = litTokensLeft; i < litTokensLeft + tokensAboutToUse; i++)
        {
            tokenSpriteParent.GetChild(i).GetComponent<Image>().color *= 0.65f;
        }
    }

    public void SetTokenColor()
    {
        Color colToUse;
        if (localPlayerNum == playerTurn)
            colToUse = playerTokenColor;
        else
            colToUse = enemyTokenColor;

        for (int i = 0; i < tokenSpriteParent.childCount; i++)
        {
            tokenSpriteParent.GetChild(i).GetComponent<Image>().color = colToUse;
        }
    }

    [ClientRpc]
    public void SetTokenColorClient()
    {
        Color colToUse;
        if (localPlayerNum == playerTurn)
            colToUse = playerTokenColor;
        else
            colToUse = enemyTokenColor;

        for (int i = 0; i < tokenSpriteParent.childCount; i++)
        {
            tokenSpriteParent.GetChild(i).GetComponent<Image>().color = colToUse;
        }
    }

    [Server]
    public void TryRespawnHeroes(List<CombatHero> heroes)
    {
        for (int i = 0; i < heroes.Count; i++)
        {
            heroes[i].TryRespawn();
        }
    }

    public void PNPTryRespawnHeroes(List<CombatHero> heroes)
    {
        for (int i = 0; i < heroes.Count; i++)
        {
            heroes[i].PNPTryRespawn();
        }
    }

    [Server]
    public void FireTowers(List<CombatHero> towers)
    {
        for(int i=0; i< towers.Count; i++)
        {
            if(towers[i].gameObject.activeSelf)
                towers[i].OnTurnStartTower();
        }
    }

    public void PNPFireTowers(List<CombatHero> towers)
    {
        for (int i = 0; i < towers.Count; i++)
        {
            if (towers[i].gameObject.activeSelf)
                towers[i].PNPOnTurnStartTower();
        }
    }

    [Client]
    public void EndGame(bool isMine)
    {
        p1Controller.SetInteractable(false);
        p2Controller.SetInteractable(false);

        if(isMine)
        {
            //player's core destroyed, lose
            loseOverlay.SetActive(true);
        }
        else
        {
            //enemy core destroyed, win! (well done gamer :^D)
            winOverlay.SetActive(true);
        }
    }

    public void PNPEndGame(bool blueWon)
    {
        p1Controller.SetInteractable(false);
        p2Controller.SetInteractable(false);


        if (blueWon)
            blueWinOverlay.SetActive(true);
        else
            redWinOverlay.SetActive(true);
    }

    //[Client]
    public void SpawnAttackProjectile(bool localPlayerAttacking, Vector3 startPos, Vector3 endpos)
    {
        AttackProjectile projectile;

        if(localPlayerAttacking)
        {
            projectile = Instantiate(blueAttackProjectile).GetComponent<AttackProjectile>();
        }
        else
        {
            projectile = Instantiate(redAttackProjectile).GetComponent<AttackProjectile>();
        }

        projectile.SetPositionsAndGo(startPos, endpos);
    }

    //[Client]
    public void SpawnInteractProjectile(int id, Vector3 startPos, Vector3 endpos)
    {
        AttackProjectile projectile = Instantiate(interactProjectile).GetComponent<AttackProjectile>();

        projectile.SetSprite(GetIngredientWithId(id).GetSprite(true));
        projectile.SetPositionsAndGo(startPos, endpos);
    }

    public Color GetCurrPlayerColor()
    {
        if (playerTurn == 1)
            return teamColor;
        else
            return enemyColor;
    }
}
