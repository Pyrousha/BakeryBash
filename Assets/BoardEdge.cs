using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardEdge : MonoBehaviour
{
    private BoardVertex firstVertex;
    private BoardVertex secondVertex;

    [SerializeField] private int firstVertexID;
    [SerializeField] private int secondVertexID;
    public int FirstVertexID => firstVertexID;
    public int SecondVertexID => secondVertexID;

    [SerializeField] private LineRenderer lineRend;

    // Start is called before the first frame update
    void Start()
    {
        //lineRend = GetComponent<LineRenderer>();
    }

    public void SetVertices(BoardVertex vertex1, BoardVertex vertex2)
    {
        firstVertex = vertex1;
        secondVertex = vertex2;

        firstVertexID = firstVertex.VertexId;
        secondVertexID = secondVertex.VertexId;

        UpdateLineRenderer();
    }

    public void UpdateLineRenderer()
    {
        lineRend.SetPosition(0, firstVertex.transform.position);
        lineRend.SetPosition(1, secondVertex.transform.position);
    }
}
