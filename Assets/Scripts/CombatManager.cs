using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private CombatHero[] p1Heroes;
    [SerializeField] private CombatHero[] p2Heroes;
    public CombatHero[] GetP1Heroes => p1Heroes;
    public CombatHero[] GetP2Heroes => p2Heroes;

    [Header("Colors")]
    [SerializeField] private Color teamColor;
    [SerializeField] private Color enemyColor;
    public Color TeamColor => teamColor;
    public Color EnemyColor => enemyColor;
}
