using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class BakingObjectButton : NetworkBehaviour
{
    [SerializeField] private ItemObject item;
    [SerializeField] private Image itemImg;
    [SerializeField] private Text nameText;
    [SerializeField] private Text swordTextNum;
    [SerializeField] private Text heartTextNum;
    [SerializeField] private Transform inventoryParent;

    [SerializeField] private Sprite transparentSprite;

    private PlayerControllerCombat playerController;

    private List<Image> inventoryImages;

    // Start is called before the first frame update
    void Start()
    {
        inventoryImages = new List<Image>();
        for (int i = 0; i < inventoryParent.childCount; i++)
        {
            inventoryImages.Add(inventoryParent.GetChild(i).GetComponent<Image>());
        }

        if (item != null)
            LoadIngredient();
    }

    public void LoadIngredient()
    {
        itemImg.sprite = item.GetSprite;
        nameText.text = item.name;
        swordTextNum.text = item.Atk.ToString();
        heartTextNum.text = item.Hp.ToString();

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
}
