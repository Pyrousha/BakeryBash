using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableHeroUIButton : MonoBehaviour
{
    [SerializeField] private HeroObject heroObj;
    [SerializeField] private GameObject selectionObj;

    [Header("Network Specific Stuff")]
    [SerializeField] private PlayerControllerHeroSelect player;

    [Header("PNP Specific Stuff")]
    [SerializeField] private PNPHeroSelectController pNPlayer;
    [SerializeField] private int playerNum;

    private void Start()
    {
        //set child's sprite
        transform.GetChild(0).GetComponent<Image>().sprite = heroObj.HeroSprite;
    }


    public void OnClicked()
    {
        if (pNPlayer != null)
            pNPlayer.SelectHero(playerNum, heroObj.Id, selectionObj);
        else
            player.SelectHero(heroObj.Id, selectionObj);
    }
}
