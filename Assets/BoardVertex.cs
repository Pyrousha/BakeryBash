using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardVertex : MonoBehaviour
{
    [SerializeField] private int vertexId;
    public int VertexId => vertexId;

    private GameBoardManager boardManager;

    [SerializeField] private Text indexDisplay;

    private void Start()
    {
        boardManager = FindObjectOfType<GameBoardManager>();
    }

    public void SetVertexId(int newId)
    {
        vertexId = newId;
        indexDisplay.text = vertexId.ToString();
    }
}