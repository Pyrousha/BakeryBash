using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/IngredientObject")]
public class IngredientObject : ScriptableObject
{
    [SerializeField] private int id;
    public int Id => id;

    [SerializeField] private Sprite allySprite;
    [SerializeField] private Sprite enemySprite;
    [SerializeField] private Sprite inventorySprite;

    public Sprite GetSprite(bool isAlly)
    {
        if (isAlly)
            return allySprite;
        else
            return enemySprite;
    }

    internal Sprite GetInventorySprite()
    {
        if (inventorySprite != null)
            return inventorySprite;
        else
            return allySprite;
    }
}
