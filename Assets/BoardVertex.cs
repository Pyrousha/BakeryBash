using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class BoardVertex : NetworkBehaviour
{
    [SerializeField] private int vertexId;
    public int VertexId => vertexId;

    //private GameBoardManager boardManager;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private GameObject reticleObj;
    [SerializeField] private Text indexDisplay;

    public CombatHero combatHero { get; private set; }
    private CombatHero tower;

    [SerializeField] private List<BoardVertex> adjacentVertices;
    public List<BoardVertex> AdjacentVertices => adjacentVertices;

    private void Start()
    {
        //boardManager = FindObjectOfType<GameBoardManager>();

        if (adjacentVertices == null)
        {
            adjacentVertices = new List<BoardVertex>();
        }

        spriteRenderer = GetComponent<SpriteRenderer>();

        reticleObj.SetActive(false);
    }

    public void SetVertexId(int newId)
    {
        vertexId = newId;
        indexDisplay.text = vertexId.ToString();
    }

    public void SetCombatHero(CombatHero newHero)
    {
        if (newHero != null)
            Debug.Log("Setting vertex with id " + vertexId + " to hero " + newHero.name);
        else
            Debug.Log("Setting vertex with id " + vertexId + " to hero NULL");

        combatHero = newHero;

        if (newHero != null)
            OnSteppedOn(newHero);
    }

    public void ResetAdjVertices()
    {
        adjacentVertices = new List<BoardVertex>();
    }

    public void TryAddAdjVertex(BoardVertex newVertex)
    {
        if(adjacentVertices.IndexOf(newVertex) > -1)
        {
            //already in list, skip
        }
        else
        {
            adjacentVertices.Add(newVertex);
        }
    }

    public void SetAdjacentVertices(List<BoardVertex> newAdjVertices)
    {
        adjacentVertices = newAdjVertices;
    }

    public void HighlightMove()
    {
        spriteRenderer.color = Color.cyan;
    }

    public void ResetColor()
    {
        spriteRenderer.color = Color.white;
    }

    public void SetReticleActive(bool newActive)
    {
        reticleObj.SetActive(newActive);
    }

    public void SetTower(CombatHero newTower)
    {
        tower = newTower;
    }

    //[Command (requiresAuthority = false)]
    public void OnSteppedOn(CombatHero hero)
    {
        if ((tower != null) && (tower.gameObject.activeSelf))
        {
            hero.SteppedOnTrappedVertex(tower);
        }
    }
}