using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ItemObject")]
public class ItemObject : ScriptableObject
{
    [SerializeField] private Sprite itemSprite;
    public Sprite GetSprite => itemSprite;
    [SerializeField] private int atkBuff;
    public int Atk => atkBuff;
    [SerializeField] private int hpBuff;
    public int Hp => hpBuff;

    [SerializeField] private List<IngredientObject> recipe;
    public List<IngredientObject> Recipe => recipe;
    private List<IngredientObject> tempInv;


    public bool HasIngredientsInInventory(List<IngredientObject> inventory)
    {
        tempInv = null;

        List<IngredientObject> tempInventory = new List<IngredientObject>(inventory);

        for(int i=0; i<recipe.Count;i++)
        {
            IngredientObject currIngredient = recipe[i];
            if( tempInventory.Remove(currIngredient) == false)
            {
                return false; // item not in inventory
            }
        }

        //All items found, send new inventory
        tempInv = tempInventory;

        return true;
    }

    public List<IngredientObject> GetNewInventory()
    {
        if (tempInv != null)
        {
            return tempInv;
        }
        else
            return null;
    }
}
