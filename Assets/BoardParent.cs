using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardParent : MonoBehaviour
{

    public List<BoardVertex> boardVertices { private set; get; }
    [SerializeField] private List<int> boardVerticesIndecies;
    public List<List<BoardEdge>> boardEdgesTable { private set; get; }
    [SerializeField] public List<ListBool> edgeConnectionsTable;//{ private set; get; }

    [SerializeField] private Transform vertexParent;
    [SerializeField] private Transform edgeParent;

    [System.Serializable]
    public class ListBool
    {
        public List<bool> bools;

        public ListBool(List<bool> newBools)
        {
            bools = new List<bool>(newBools);
        }
    }

    public Transform VertexParent => vertexParent;
    public Transform EdgeParent => edgeParent;

    private bool doneStart = false;

    public void DoStart()
    {
        Start();
    }

    private void Start()
    {
        Debug.Log("boardVerticesIndecies.Count: " + boardVerticesIndecies.Count);

        if (doneStart)
            return;

        boardVertices = new List<BoardVertex>();
        boardEdgesTable = new List<List<BoardEdge>>();

        if (boardVerticesIndecies == null)
            boardVerticesIndecies = new List<int>();
        else
        {
            for (int i = 0; i < boardVerticesIndecies.Count; i++)
            {
                bool foundVertex = false;

                for (int j=0; j<vertexParent.childCount;j++)
                {
                    BoardVertex tempVert = vertexParent.GetChild(j).GetComponent<BoardVertex>();
                    if (tempVert.VertexId == i)
                    {
                        boardVertices.Add(tempVert);
                        foundVertex = true;
                        continue;
                    }
                }

                if(foundVertex == false)
                    boardVertices.Add(null);
            }
        }

        if(edgeConnectionsTable == null)
        {
            edgeConnectionsTable = new List<ListBool>();
        }
        else
        {
            //Initialize boardEdge array
            for(int i=0; i< boardVerticesIndecies.Count; i++)
            {
                boardEdgesTable.Add(new List<BoardEdge>(new BoardEdge[boardVerticesIndecies.Count]));
            }

            for(int i=0; i< edgeParent.childCount; i++)
            {
                BoardEdge newChildEdge = edgeParent.GetChild(i).GetComponent<BoardEdge>();

                boardEdgesTable[newChildEdge.FirstVertexID][newChildEdge.SecondVertexID] = newChildEdge;
            }
        }

        doneStart = true;

        Debug.Log("edge table: " + PrintEdgeConnectionsTable());
    }

    public void SetBoardVertices(List<BoardVertex> newBoardVertices)
    {
        int[] tempArr = new int[newBoardVertices.Count];
        for (int i = 0; i < tempArr.Length; i++)
            tempArr[i] = -1;

        boardVerticesIndecies = new List<int>(tempArr);
        for (int j = 0; j < newBoardVertices.Count; j++)
        {
            if (newBoardVertices[j] == null)
                continue;

            for (int i = 0; i < vertexParent.childCount; i++)
            {
                BoardVertex newChild = vertexParent.GetChild(i).GetComponent<BoardVertex>();
                if (newChild.VertexId == newBoardVertices[j].VertexId)
                {
                    boardVerticesIndecies[j] = i;
                }
            }
        }

        Debug.Log("boardVerticesIndecies.Count: " + boardVerticesIndecies.Count);
    }

    public void SetEdgeConnections(List<List<bool>> newEdgeConnections)
    {
        List<ListBool> copyList = new List<ListBool>();
        for (int i = 0; i < newEdgeConnections.Count; i++)
        {
            copyList.Add(new ListBool(newEdgeConnections[i]));
        }
        edgeConnectionsTable = copyList;
    }

    public string PrintEdgeConnectionsTable()
    {
        int numVertices = boardVerticesIndecies.Count;
        print("numverts: " + numVertices);

        string strToPrint = "\n";
        for (int i = 0; i < numVertices; i++)
        {
            for (int j = 0; j < numVertices; j++)
            {
                strToPrint += ((edgeConnectionsTable[i].bools[j] ? 1 : 0) + ",");
            }
            strToPrint += "\n";
        }
        return strToPrint;
    }
}
