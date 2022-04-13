using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeroSelectController : MonoBehaviour
{
    public static HeroSelectController Instance;

    private int[] p1HeroIndices;
    private int[] p2HeroIndices;

    public List<int> P1HeroIndices => new List<int>(p1HeroIndices);
    public List<int> P2HeroIndices => new List<int>(p2HeroIndices);

    [SerializeField] private bool pnpMode;
    public bool PNPMode => pnpMode;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Debug.LogError("Multiple HeroSelectControllers Found!");
    }

    public void LoadHeroSelections(List<int> p1Heroes, List<int> p2Heroes)
    {
        int numHeroes = p1Heroes.Count;

        p1HeroIndices = new int[numHeroes];
        p2HeroIndices = new int[numHeroes];

        for(int i=0; i < numHeroes; i++)
        {
            p1HeroIndices[i] = p1Heroes[i];
            p2HeroIndices[i] = p2Heroes[i];
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("CombatScene");
    }
}
