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
    private Image reticleImg;
    [SerializeField] private Text indexDisplay;

    [SerializeField] private bool isRespawnVertex = false;
    public bool IsRespawnVertex => isRespawnVertex;

    public CombatHero combatHero { get; private set; }
    private CombatHero tower;

    [SerializeField] private List<BoardVertex> adjacentVertices;
    public List<BoardVertex> AdjacentVertices => adjacentVertices;

    //private SpriteRenderer towerRangeIcon;
    //public SpriteRenderer TowerRangeIcon => towerRangeIcon;

    private void Start()
    {
        //boardManager = FindObjectOfType<GameBoardManager>();

        if (adjacentVertices == null)
        {
            adjacentVertices = new List<BoardVertex>();
        }

        spriteRenderer = GetComponent<SpriteRenderer>();

        reticleObj.SetActive(false);

        //indexDisplay.text = "";
    }

    public void SetVertexId(int newId)
    {
        vertexId = newId;
        indexDisplay.text = vertexId.ToString();
    }

    public void SetCombatHero(CombatHero newHero)
    {
        //if (newHero != null)
          //  Debug.Log("Setting vertex with id " + vertexId + " to hero " + newHero.name);
        //else
          //  Debug.Log("Setting vertex with id " + vertexId + " to hero NULL");

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

    public void HighlightMove()
    {
        spriteRenderer.color = Color.cyan;
    }

    public void ResetColor()
    {
        spriteRenderer.color = Color.white;
    }

    public void SetReticleActive(bool newActive, Sprite newSprite)
    {
        reticleObj.SetActive(newActive);
        if (reticleImg == null)
            reticleImg = reticleObj.GetComponent<Image>();

        reticleImg.sprite = newSprite;
    }

    public void SetTower(CombatHero newTower)
    {
        tower = newTower;
    }

    public void RemoveTower()
    {
        tower = null; //stepping on this vertex no longer causes hero to take damage
    }

    //[Command (requiresAuthority = false)]
    public void OnSteppedOn(CombatHero hero)
    {
        if ((tower != null) && (tower.gameObject.activeSelf))
        {
            hero.SteppedOnTrappedVertex(tower);
        }
    }

    /*
    public void SetTowerRangeIcon(SpriteRenderer newSprite, Color col)
    {
        if (towerRangeIcon != null)
            return;

        towerRangeIcon = newSprite;
        towerRangeIcon.gameObject.transform.position = transform.position;
        towerRangeIcon.color = col;
    }

    public void ReplaceTowerRangeIcon(Sprite newSprite)
    {
        if (towerRangeIcon != null)
        {
            towerRangeIcon.sprite = newSprite;
        }
    }*/
}