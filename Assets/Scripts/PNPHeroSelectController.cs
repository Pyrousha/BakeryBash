using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PNPHeroSelectController : MonoBehaviour
{
    private int playerNum;

    //public Text playerNameText;

    public Text selectedHeroNameText;
    public Image selectedHeroImage;

    [SerializeField] private Text bioText;

    [SerializeField] private HeroObject[] selectableHeroes;
    [SerializeField] private Image readyButtonImage;
    [SerializeField] private Text readyButtonText;
    [SerializeField] private GameObject readyButtonHighlight;

    private bool isReady = false;

    private List<int> p1SelectedHeroIds;
    private List<int> p2SelectedHeroIds;

    private CharacterSelectMusicManager musicManager;

    private void Start()
    {
        p1SelectedHeroIds = new List<int>();
        p2SelectedHeroIds = new List<int>();

        musicManager = FindObjectOfType<CharacterSelectMusicManager>();

        transform.localPosition = Vector3.zero;

        SetReadyVisuals();
    }

    //Called when player clicks on a hero
    public void SelectHero(int playerNum, int heroId, GameObject selectionObj)
    {
        SetHeroDescription(heroId);

        if (isReady)
            return;

        List<int> heroList;

        switch(playerNum)
        {
            case 1:
                {
                    heroList = p1SelectedHeroIds;
                    break;
                }
            case 2:
                {
                    heroList = p2SelectedHeroIds;
                    break;
                }
            default:
                {
                    //OwO nOwO
                    return;
                }
        }

        if (heroList.IndexOf(heroId) > -1)
        {
            //hero already selected, deselect
            selectionObj.SetActive(false);
            heroList.Remove(heroId);
        }
        else
        {
            //Clicked on a hero that is not yet selected

            //If no space left
            if (heroList.Count >= 3)
                return;

            //select hero
            heroList.Add(heroId);
            selectionObj.SetActive(true);
        }

        int numTotalSelectedHeroes = p1SelectedHeroIds.Count + p2SelectedHeroIds.Count;

        musicManager.SetCharacterNumber(Mathf.Min(3, numTotalSelectedHeroes));

        if (numTotalSelectedHeroes >= 6)
            readyButtonHighlight.SetActive(true);
        else
            readyButtonHighlight.SetActive(false);
    }

    //Loads clicked on hero's name, image, and bio
    public void SetHeroDescription(int heroId)
    {
        HeroObject newHero = selectableHeroes[heroId];

        selectedHeroNameText.text = newHero.SelectName;
        selectedHeroImage.sprite = newHero.HeroSprite;
        bioText.text = newHero.BioText;
    }

    public void ReadyClicked()
    {
        if (isReady)
            return;

        if (p1SelectedHeroIds.Count + p2SelectedHeroIds.Count < 6)
            return;

        isReady = true;

        SetReadyVisuals();

        //Send player selections and start countdown
        HeroSelectController.Instance.LoadHeroSelections(p1SelectedHeroIds, p2SelectedHeroIds);
        HeroSelectController.Instance.StartGame();
    }

    public void SetReadyVisuals()
    {
        if (isReady)
        {
            readyButtonHighlight.SetActive(false);
            readyButtonImage.color = Color.green;
            readyButtonText.text = "Ready!";
        }
        else
        {
            readyButtonImage.color = Color.white;
            readyButtonText.text = "Ready?";
        }
    }
}
