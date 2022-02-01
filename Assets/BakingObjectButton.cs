using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;

public class BakingObjectButton : NetworkBehaviour
{
    [SerializeField] private ItemObject item;
    public ItemObject Item => item;
    [SerializeField] private Image overlayImage;
    [SerializeField] private Image itemImg;
    [SerializeField] private Text nameText;
    [SerializeField] private Text swordTextNum;
    [SerializeField] private Text heartTextNum;
    [SerializeField] private Transform inventoryParent;

    [SerializeField] private Sprite transparentSprite;

    [Header("Colors")]
    [SerializeField] private Color darkOverlayColor;
    [SerializeField] private Color darkIngredColor;

    private PlayerControllerCombat playerController;

    private List<Image> inventoryImages;

    private bool doneStart = false;
    // Start is called before the first frame update
    void Start()
    {
        if (doneStart)
            return;

        inventoryImages = new List<Image>();

        for (int i = 0; i < inventoryParent.childCount; i++)
        {
            inventoryImages.Add(inventoryParent.GetChild(i).GetComponent<Image>());
        }

        if (item != null)
            LoadIngredient();

        doneStart = true;
    }

    public void LoadIngredient()
    {
        itemImg.sprite = item.GetSprite;
        nameText.text = item.name;

        bool moveIcon = false;

        Transform swordTransform = swordTextNum.transform.parent;
        Transform heartTransform = heartTextNum.transform.parent;

        float offset = -2.5f;

        //Load sword number if attack is over 0, hide sword icon otherwise
        if (item.Atk > 0)
        {
            swordTextNum.text = "+"+item.Atk.ToString();
        }
        else
        {
            swordTransform.gameObject.SetActive(false);

            heartTransform.localPosition -= new Vector3(22.5f, 0, 0); //Center between sword and heart
            heartTransform.localPosition -= new Vector3(offset, 0, 0);
            moveIcon = true;
        }

        //Load hp number if attack is over 0, hide heart icon otherwise
        if (item.Hp > 0)
        {
            heartTextNum.text = "+"+item.Hp;
        }
        else
        {
            heartTransform.gameObject.SetActive(false);

            swordTransform.localPosition += new Vector3(22.5f, 0, 0); //Center between sword and heart
            swordTransform.localPosition -= new Vector3(offset, 0, 0);
            moveIcon = true;
        }

        if (moveIcon)
        {
            itemImg.transform.localPosition += new Vector3(22.5f, 0, 0); //Center between mario and sword
            itemImg.transform.localPosition += new Vector3(offset, 0, 0);
        }

        UpdateRecipeUI();
    }

    public void SetController(PlayerControllerCombat controller)
    {
        playerController = controller;
    }

    [Client]
    public void OnClicked()
    {
        playerController.TryBake(item);
    }

    public void UpdateRecipeUI()
    {
        List<IngredientObject> ingredientInventory = item.Recipe;

        for (int i = 0; i < inventoryImages.Count; i++)
        {
            if (ingredientInventory.Count > i)
            {
                inventoryImages[i].sprite = ingredientInventory[i].GetSprite(true);
            }
            else
            {
                inventoryImages[i].sprite = transparentSprite;
            }
        }
    }

    public void SetVisualsFromInventory(ItemObject item, bool hasIngredients)
    {
        if (!doneStart)
            Start();

        Debug.Log("Setting visuals for item " + item.name + ", has all? " + hasIngredients);

        if(hasIngredients)
        {
            overlayImage.color = Color.clear;
        }
        else
        {
            overlayImage.color = darkOverlayColor;
        }

        List<IngredientObject> missingIngredients = item.GetMissingIngredients();
        List<IngredientObject> recipeIngredients = item.Recipe;

        for (int i = 0; i < recipeIngredients.Count; i++)
        {
            IngredientObject currIngredient = recipeIngredients[i];

            if(missingIngredients.Remove(currIngredient) == true) //missing current ingredient
            {
                inventoryImages[i].color = darkIngredColor;
            }
            else
            {
                //Has this ingredient, highlight
                inventoryImages[i].color = Color.white;
            }
        }
    }
}
