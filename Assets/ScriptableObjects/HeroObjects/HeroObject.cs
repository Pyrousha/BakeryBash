using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/SelectableHeroObject")]
public class HeroObject : ScriptableObject
{
    [SerializeField] private int id;
    public int Id => id;

    [SerializeField] private Sprite heroSprite;
    public Sprite HeroSprite => heroSprite;

    [SerializeField] private int basicAttackRange;
    public int BasicAttackRange => basicAttackRange;

    [SerializeField] private int basicAttackDamage;
    public int BasicAttackDamage => basicAttackDamage;

    [SerializeField] private bool canWalkOverSpecialTerrain;
    public bool CanWalkOverSpecialTerrain => canWalkOverSpecialTerrain;

    [SerializeField] private int maxHp;
    public int MaxHp => maxHp;
}
