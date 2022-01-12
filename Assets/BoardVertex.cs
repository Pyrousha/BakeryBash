using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardVertex : MonoBehaviour
{
    [SerializeField] private int vertexId;
    public int VertexId => vertexId;

    //private GameBoardManager boardManager;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private GameObject reticleObj;
    [SerializeField] private Text indexDisplay;

    public CombatHero combatHero { get; private set; }

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
        combatHero = newHero;
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
}