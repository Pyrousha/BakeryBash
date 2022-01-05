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
}
