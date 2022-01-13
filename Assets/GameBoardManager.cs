using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameBoardManager : MonoBehaviour
{
    private int numVertices;
    private List<int> openIndices;

    [System.Serializable]
    public enum GameBoardStateEnum
    {
        mapEditor,
        playing
    }

    [SerializeField] private GameBoardStateEnum gameBoardState;
    public GameBoardStateEnum GetGameBoardStateEnum => gameBoardState;

    private List<BoardVertex> boardVertices; 
    private List<List<BoardEdge>> boardEdgesTable;
    private List<List<bool>> edgeConnectionsTable;

    [SerializeField] private string mapName;

    [Header("References")]
    [SerializeField] private CombatManager combatManager;
    
    private bool vertexMode = true;
    private Transform vertexToMove;
    [SerializeField] private Text vertexModeText;

    [SerializeField] private Transform boardParent;
    [SerializeField] private Transform vertexParent;
    [SerializeField] private Transform edgeParent;

    [SerializeField] private GameObject boardVertexPrefab;
    [SerializeField] private GameObject boardEdgePrefab;

    [Header("Board To Load")]
    [SerializeField] private GameObject boardToLoadPrefab;

    private bool doneStart = false;
    // Start is called before the first frame update
    void Start()
    {
        if (doneStart)
            return;

        openIndices = new List<int>();

        //create new empty board
        boardVertices = new List<BoardVertex>();
        boardEdgesTable = new List<List<BoardEdge>>();
        edgeConnectionsTable = new List<List<bool>>();

        if (boardToLoadPrefab != null)
        {
            Destroy(boardParent.gameObject);
            boardParent = null;
            vertexParent = null;
            edgeParent = null;

            //load board prefab
            GameObject boardToLoad = Instantiate(boardToLoadPrefab);
            boardToLoad.transform.parent = transform;
            boardToLoad.transform.localPosition = Vector3.zero;
            BoardParent boardParentScript = boardToLoad.GetComponent<BoardParent>();

            boardParentScript.DoStart();

            boardParent = boardToLoad.transform;
            vertexParent = boardParentScript.VertexParent;
            edgeParent = boardParentScript.EdgeParent;

            mapName = boardToLoad.name;

            boardVertices =  boardParentScript.boardVertices;
            boardEdgesTable = boardParentScript.boardEdgesTable;

            //edgeConnectionsTable = boardParentScript.edgeConnectionsTable;
            
            edgeConnectionsTable = new List<List<bool>>();
            List<BoardParent.ListBool> tempListBoolList = boardParentScript.edgeConnectionsTable;
            for(int i =0; i<tempListBoolList.Count;i++)
            {
                edgeConnectionsTable.Add(new List<bool>());

                for(int j = 0; j<tempListBoolList.Count; j++)
                {
                    edgeConnectionsTable[i].Add(tempListBoolList[i].bools[j]);

                    if(tempListBoolList[i].bools[j])
                    {
                        //Debug.Log("edge should exist at (" + i + "," + j + "), actual value:" + boardEdgesTable[i][j]);
                        //if (boardEdgesTable[i][j] == null)
                            //Debug.Log("BRUH");
                    }

                    if(boardEdgesTable[i][j] != null)
                    {
                        //Debug.Log("found it! " + +i + "," + j + ")");
                    }
                }
            }

            CalculateAdjacentVertices();

            //Debug.Log("table after load:" + PrintEdgeConnectionsTable());

            //Load openIndices
            for (int i = 0; i < boardVertices.Count; i++)
            {
                if (boardVertices[i] == null)
                    openIndices.Add(i);
            }

            openIndices.Sort();

            numVertices = boardVertices.Count;
        }

        doneStart = true;
    }

    public BoardVertex GetVertexWithId(int id)
    {
        if (!doneStart)
            Start();

        foreach(BoardVertex vert in boardVertices)
        {
            if(vert != null)
                if (vert.VertexId == id)
                    return vert;
        }

        return null;
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            VertexClicked();
            return;
        }

        if (gameBoardState == GameBoardStateEnum.mapEditor)
        {
            if (vertexToMove != null)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                vertexToMove.position = new Vector3(mousePos.x, mousePos.y, vertexToMove.position.z);

                //Update linerenderes
                for (int i = 0; i < boardEdgesTable.Count; i++)
                {
                    for (int j = 0; j < boardEdgesTable.Count; j++)
                    {
                        if (boardEdgesTable[i][j] != null)
                        {
                            boardEdgesTable[i][j].UpdateLineRenderer();
                        }
                    }
                }
            }

            if(Input.GetKeyDown(KeyCode.Space))
            {
                //Update linerenderes
                for (int i = 0; i < boardEdgesTable.Count; i++)
                {
                    for (int j = 0; j < boardEdgesTable.Count; j++)
                    {
                        if (boardEdgesTable[i][j] != null)
                        {
                            boardEdgesTable[i][j].UpdateLineRenderer();
                        }
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
                VertexReleased();

            if (Input.GetMouseButtonDown(1))
                RemoveVertexUnderMouse();

            if (Input.GetKeyDown(KeyCode.Tab))
                SwapVertexEdgeMode();

            if (Input.GetKeyDown(KeyCode.Return))
                SaveMapToPrefab();
        }
    }

    private void SwapVertexEdgeMode()
    {
        vertexToMove = null;

        vertexMode = !vertexMode;

        if (vertexMode)
            vertexModeText.text = "Vertex Mode (Press TAB to switch)";
        else
            vertexModeText.text = "Edge Mode (Press TAB to switch)";
    }

    private void SaveMapToPrefab()
    {
        CalculateAdjacentVertices();

        string localPath = "Assets/Prefabs/MapPrefabs/" + mapName + ".prefab";

        #if UNITY_EDITOR
        //Make sure filename is unique
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

        //Set values for lists and stuff
        BoardParent boardParentScript = boardParent.GetComponent<BoardParent>();
        boardParentScript.SetBoardVertices(boardVertices);
        boardParentScript.SetEdgeConnections(edgeConnectionsTable);

        //Create new prefab
        PrefabUtility.SaveAsPrefabAssetAndConnect(boardParent.gameObject, localPath, InteractionMode.UserAction);

        //Unpack so things can still be edited
        PrefabUtility.UnpackPrefabInstance(boardParent.gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);
        #endif
    }

    private void OnMouseDown()
    {
        if ((gameBoardState == GameBoardStateEnum.mapEditor) && (vertexMode))
        {
            //Add new vertex when map is clicked on
            AddNewVertex();
        }
    }

    public void AddNewVertex()
    {
        //Create and place board prefab
        Transform newBoardVertex = Instantiate(boardVertexPrefab).transform;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos -= new Vector3(0, 0, 0.25f + mousePos.z);
        newBoardVertex.position = mousePos;
        newBoardVertex.parent = vertexParent;

        BoardVertex newVertex = newBoardVertex.GetComponent<BoardVertex>();

        if (openIndices.Count > 0)
        {
            //A vertex was removed, so fill that slot instead of expanding table (since recalulating all the indices would be ass lmao)
            int newIndex = openIndices[0];
            openIndices.RemoveAt(0);

            newVertex.SetVertexId(newIndex);

            boardVertices[newIndex] = newVertex;
        }
        else
        {
            newVertex.SetVertexId(numVertices);
            numVertices++;

            boardVertices.Add(newVertex);

            //Update edge tables to include new vertex
            for (int i = 0; i < numVertices - 1; i++)
            {
                //Add a new element to each row (add a new column) to each table
                boardEdgesTable[i].Add(null);
                edgeConnectionsTable[i].Add(false);
            }

            //Add a new row to each table
            boardEdgesTable.Add(new List<BoardEdge>(new BoardEdge[numVertices]));
            edgeConnectionsTable.Add(new List<bool>(new bool[numVertices]));
        }

        CalculateAdjacentVertices();

        //Debug.Log("table after adding vertex:" + PrintEdgeConnectionsTable());
    }

    public void AddNewEdge(BoardVertex vertex1, BoardVertex vertex2)
    {
        if (vertex1 == null || vertex2 == null)
        {
            //Debug.LogError("AddNewEdge called with null verticies");
            firstVertexClicked = null;
            return;
        }

        Debug.Log("AddnewEdge called with verticies with ids: " + vertex1.VertexId + ", " + vertex2.VertexId);

        if(vertex1.VertexId == vertex2.VertexId)
        {
            Debug.LogError("Edge attempted to be added to the same vertex with id: " + vertex1.VertexId);
            return;
        }

        if(vertex1.VertexId < vertex2.VertexId)
        {
            if (edgeConnectionsTable[vertex1.VertexId][vertex2.VertexId] == true)
            {
                Debug.LogError("Cannot add duplicate edge");
                return; //don't add duplicate edge
            }

            GameObject newBoardEdge = Instantiate(boardEdgePrefab);
            newBoardEdge.transform.parent = edgeParent;

            BoardEdge newEdge = newBoardEdge.GetComponent<BoardEdge>();
            newEdge.SetVertices(vertex1, vertex2);

            boardEdgesTable[vertex1.VertexId][vertex2.VertexId] = newEdge;
            edgeConnectionsTable[vertex1.VertexId][vertex2.VertexId] = true;

            //Debug.Log("table after adding edge:"+ PrintEdgeConnectionsTable());

            CalculateAdjacentVertices();

            return;
        }
        if (vertex1.VertexId > vertex2.VertexId)
        {
            if (edgeConnectionsTable[vertex2.VertexId][vertex1.VertexId] == true)
            {
                Debug.LogError("Cannot add duplicate edge");
                return; //don't add duplicate edge
            }

            GameObject newBoardEdge = Instantiate(boardEdgePrefab);
            newBoardEdge.transform.parent = edgeParent;

            BoardEdge newEdge = newBoardEdge.GetComponent<BoardEdge>();
            newEdge.SetVertices(vertex2, vertex1);

            boardEdgesTable[vertex2.VertexId][vertex1.VertexId] = newEdge;
            edgeConnectionsTable[vertex2.VertexId][vertex1.VertexId] = true;

            //Debug.Log("table after adding edge:" + PrintEdgeConnectionsTable());

            CalculateAdjacentVertices();

            return;
        }
    }

    private BoardVertex firstVertexClicked;

    public void VertexClicked()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        BoardVertex newVertex = hit.transform.gameObject.GetComponent<BoardVertex>();

        if (gameBoardState == GameBoardStateEnum.mapEditor)
        {
            if (newVertex != null)
            {
                if(vertexMode)
                {
                    vertexToMove = newVertex.gameObject.transform;
                }
                else
                {
                    firstVertexClicked = newVertex;
                }
            }
        }
        else
        {
            if (newVertex != null)
                combatManager.GetCurrPlayerController().OnVertexClicked(newVertex);
        }
    }

    public void VertexReleased()
    {
        if(vertexMode)
        {
            vertexToMove = null;
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        BoardVertex newVertex = hit.transform.gameObject.GetComponent<BoardVertex>();
        if (newVertex != null)
        {
            //create edge between verticies
            AddNewEdge(firstVertexClicked, newVertex);
            firstVertexClicked = null;
        }
    }

    private void RemoveVertexUnderMouse()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        BoardVertex newVertex = hit.transform.gameObject.GetComponent<BoardVertex>();
        if (newVertex != null)
            RemoveVertex(newVertex);
    }

    private void RemoveVertex(BoardVertex newVertex)
    {
        //Debug.Log("table before remove:"+ PrintEdgeConnectionsTable());

        int idToRemove = newVertex.VertexId;
        boardVertices[idToRemove] = null;
 
        Destroy(newVertex.gameObject);

        openIndices.Add(idToRemove);
        openIndices.Sort();

        Debug.Log("Trying to remove vertex with ID/index: " + idToRemove);

        //Remove relevant element from each row (remove vertex's column) from each table
        for (int i = 0; i < numVertices; i++)
        {
            bool possibleEdge = edgeConnectionsTable[i][idToRemove];
            if (possibleEdge == false)
            {
                //No edge here, No need to do anything
                    //edgeConnectionsTable[i].RemoveAt(idToRemove);
                    //boardEdgesTable[i].RemoveAt(idToRemove);
            }
            else
            {
                //edge here, must delete from scene before clearing table
                BoardEdge edgeToDelete = boardEdgesTable[i][idToRemove];
                Destroy(edgeToDelete.gameObject);

                boardEdgesTable[i][idToRemove] = null;
                edgeConnectionsTable[i][idToRemove] = false;
            }
        }

        //Clear vertex's row from both tables
        for (int i = 0; i < numVertices; i++)
        {
            bool possibleEdge = edgeConnectionsTable[idToRemove][i];

            if (possibleEdge == false)
            {
                //No edge here, No need to do anything
            }
            else
            {
                //edge here, must delete from scene before clearing table
                BoardEdge edgeToDelete = boardEdgesTable[idToRemove][i];
                Destroy(edgeToDelete.gameObject);

                boardEdgesTable[idToRemove][i] = null;
                edgeConnectionsTable[idToRemove][i] = false;
            }
        }


        //Debug.Log("table after remove: " + PrintEdgeConnectionsTable());

        CalculateAdjacentVertices();
    }

    public string PrintEdgeConnectionsTable()
    {
        string strToPrint = "\n";
        for (int i = 0; i < edgeConnectionsTable.Count; i++)
        {
            for (int j = 0; j < edgeConnectionsTable.Count; j++)
            {
                strToPrint += ((edgeConnectionsTable[i][j] ? 1:0) + ",");
            }
            strToPrint += "\n";
        }
        return strToPrint;
    }

    public void CalculateAdjacentVertices()
    {
        for(int i =0; i<boardVertices.Count;i++)
        {
            BoardVertex vertex = boardVertices[i];

            if (vertex != null)
                vertex.ResetAdjVertices();
        }

        for (int i = 0; i < edgeConnectionsTable.Count; i++)
        {
            for (int j = 0; j < edgeConnectionsTable.Count; j++)
            {
                if (edgeConnectionsTable[i][j])
                {
                    BoardVertex v1 = boardVertices[i];
                    BoardVertex v2 = boardVertices[j];

                    v1.TryAddAdjVertex(v2);
                    v2.TryAddAdjVertex(v1);
                }
            }
        }
    }
}
