using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableHeroUIButton : MonoBehaviour
{
    [SerializeField] private PlayerControllerHeroSelect player;

    [SerializeField] private HeroObject heroObj;
    [SerializeField] private GameObject selectionObj;

    private void Start()
    {
        //set child's sprite
        transform.GetChild(0).GetComponent<Image>().sprite = heroObj.HeroSprite;
    }


    public void OnClicked()
    {
        player.SelectHero(heroObj.Id, selectionObj);
    }
}
