using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerControllerCombat : NetworkBehaviour
{
    [SerializeField] private int playerNum;
    private CombatManager combatManager;
    private PlayerLobbyDetails playerLobbyDetails;
    private PlayerLobbyDetails enemyLobbyDetails; 

    [Client]
    // Start is called before the first frame update
    void Start()
    {
        combatManager = FindObjectOfType<CombatManager>();

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
        }

        if (playerLobbyDetails.isLocalPlayer)
        {
            SetHeroColors();
        }
    }

    [Client]
    public void SetHeroColors()
    {
        if (playerNum == 1)
        {
            foreach(CombatHero hero in combatManager.GetP1Heroes)
            {
                hero.SetOwnership(true);
                hero.SetHeroObj(playerLobbyDetails.GetHeroObj);
            }
            foreach (CombatHero hero in combatManager.GetP2Heroes)
            {
                hero.SetOwnership(false);
                hero.SetHeroObj(enemyLobbyDetails.GetHeroObj);
            }

            return;
        }
        if (playerNum == 2)
        {
            foreach (CombatHero hero in combatManager.GetP1Heroes)
            {
                hero.SetOwnership(false);
                hero.SetHeroObj(enemyLobbyDetails.GetHeroObj);
            }
            foreach (CombatHero hero in combatManager.GetP2Heroes)
            {
                hero.SetOwnership(true);
                hero.SetHeroObj(playerLobbyDetails.GetHeroObj);
            }
        }
    }
}
