using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroIndexToHeroObjectConverter : MonoBehaviour
{
    [SerializeField] private HeroObject[] selectableHeroes;

    public static HeroIndexToHeroObjectConverter Instance;
    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Debug.LogError("Multiple HeroIndexToHeroObjectConverters found");
    }

    public HeroObject GetHeroOfIndex(int index)
    {
        return selectableHeroes[index];
    }
}
