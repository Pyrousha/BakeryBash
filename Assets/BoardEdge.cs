using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardEdge : MonoBehaviour
{
    [SerializeField] private BoardVertex firstVertex;
    [SerializeField] private BoardVertex secondVertex;

    [SerializeField] private int firstVertexID;
    [SerializeField] private int secondVertexID;
    public int FirstVertexID => firstVertexID;
    public int SecondVertexID => secondVertexID;

    [SerializeField] private LineRenderer lineRend;

    [System.Serializable]
    public enum EdgeTypeEnum
    {
        normal, 
        unwalkable,
        specialTerrain,
        respawn
    }

    [SerializeField] private EdgeTypeEnum edgeType = EdgeTypeEnum.normal;
    public EdgeTypeEnum EdgeType => edgeType;

    // Start is called before the first frame update
    void Start()
    {
        if (firstVertex != null && secondVertex != null)
            UpdateLineRenderer();

        UpdateEdgeColor();
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

    public void SetEdgeType(EdgeTypeEnum newType)
    {
        edgeType = newType;
        UpdateEdgeColor();
    }

    public void UpdateEdgeColor()
    {
        Color newColor = Color.black;
        //newColor = Color.clear;

        switch (edgeType)
        {
            case EdgeTypeEnum.normal:
                {
                    //newColor = Color.black;
                    break;
                }
            case EdgeTypeEnum.unwalkable:
                {
                    newColor = Color.red;
                    //newColor = Color.clear;
                    break;
                }
            case EdgeTypeEnum.specialTerrain:
                {
                    newColor = Color.green;
                    break;
                }
            case EdgeTypeEnum.respawn:
                {
                    newColor = Color.grey;
                    break;
                }
        }

        lineRend.startColor = newColor;
        lineRend.endColor = newColor;
    }
}
