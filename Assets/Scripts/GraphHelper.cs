using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class GraphHelper
{
    private static List<BoardVertex> activeArrowVertices = new List<BoardVertex>();
    private static GameBoardManager boardManager;


   public static List<BoardVertex> BFS(BoardVertex startingVertex, int depth)
    {
        List<BoardVertex> vertices = new List<BoardVertex>();
        vertices.Add(startingVertex);

        for(int i = 0; i<depth; i++)
        {
            List<BoardVertex> tempVerts = new List<BoardVertex>(vertices);
            foreach (BoardVertex vert in vertices)
            {
                tempVerts.AddRange(BFSHelper(vert, vertices));
            }

            vertices = tempVerts;
        }

        vertices.Remove(startingVertex);

        //Debug.Log("Size: " + vertices.Count + ", distinctSize: " + vertices.Distinct().ToList().Count);

        return vertices.Distinct().ToList();
    }

    public static List<BoardVertex> BFSWalkable(BoardVertex startingVertex, int depth, bool canWalkOverSpecialTerrain, GameBoardManager newBoardManager)
    {
        if (boardManager == null)
            boardManager = newBoardManager;

        foreach (BoardVertex vert in boardManager.BoardVertices)
        {
            if (vert != null)
            {
                vert.SetVisited(false);
                vert.SetPrevVertex(null);
            }
        }

        List<BoardVertex> vertices = new List<BoardVertex>();
        vertices.Add(startingVertex);

        for (int i = 0; i < depth; i++)
        {
            List<BoardVertex> tempVerts = new List<BoardVertex>(vertices);
            foreach (BoardVertex vert in vertices)
            {
                tempVerts.AddRange(BFSHelperWalkable(vert, vertices, canWalkOverSpecialTerrain));
            }

            vertices = tempVerts;
        }

        vertices.Remove(startingVertex);

        //Debug.Log("(Walkable) Size: " + vertices.Count + ", distinctSize: " + vertices.Distinct().ToList().Count);

        return vertices.Distinct().ToList();
    }

    /// <summary>
    /// Returns a list of BoardVertices adjacent to the given vertex, that have not been visited yet
    /// </summary>
    /// <param name="startingVertex"></param>
    /// <param name="visitedNodes"></param>
    /// <returns></returns>
    private static List<BoardVertex> BFSHelper(BoardVertex startingVertex, List<BoardVertex> visitedNodes)
    {
        List<BoardVertex> newVertices = new List<BoardVertex>();

        foreach(BoardVertex vert in startingVertex.AdjacentVertices)
        {
            if (visitedNodes.IndexOf(vert) < 0)
            {
                //Not yet visited, add to array
                newVertices.Add(vert);

                //vert.SetPrevVertex(startingVertex);
            }
        }

        return newVertices;
    }

    private static List<BoardVertex> BFSHelperWalkable(BoardVertex startingVertex, List<BoardVertex> visitedNodes, bool canWalkOverSpecialTerrain)
    {
        List<BoardVertex> newVertices = new List<BoardVertex>();

        foreach (BoardVertex vert in startingVertex.AdjacentVertices)
        {
            if (vert.visited)
                continue;

            if (vert.combatHero == null)
            {
                BoardEdge.EdgeTypeEnum edgeType = boardManager.GetEdgeWithVertices(startingVertex.VertexId, vert.VertexId).EdgeType;
                //Debug.Log("Edge from " + startingVertex.VertexId + " to " + vert.VertexId + ": " + edgeType);

                bool addToList = false;

                switch (edgeType)
                {
                    case BoardEdge.EdgeTypeEnum.normal:
                        {
                            addToList = true;
                            break;
                        }
                    case BoardEdge.EdgeTypeEnum.specialTerrain:
                        {
                            if (canWalkOverSpecialTerrain)
                                addToList = true;
                            break;
                        }
                    case BoardEdge.EdgeTypeEnum.respawn:
                        {
                            if (startingVertex.IsRespawnVertex)
                                addToList = true;
                            break;
                        }
                    default:
                        {
                            addToList = false;
                            break;
                        }
                }

                if (addToList)
                {
                    //Not yet visited, add to array
                    newVertices.Add(vert);

                    vert.SetPrevVertex(startingVertex);
                    vert.SetVisited(true);
                }
            }
        }

        return newVertices;
    }

    public static List<BoardVertex> BFSOnlyAtDepth(BoardVertex startingVertex, int depth)
    {
        List<BoardVertex> bigBFS = BFS(startingVertex, depth);
        List<BoardVertex> smallBFS = BFS(startingVertex, depth-1);

        foreach (BoardVertex vert in smallBFS)
            bigBFS.Remove(vert);

        return bigBFS.Distinct().ToList();
    }

    public static void ResetVertexArrows()
    {
        foreach (BoardVertex vert in activeArrowVertices)
        {
            vert.ResetArrowVisual();
        }
    }

    public static List<BoardVertex> SetVertexArrows(BoardVertex startingVertex, BoardVertex destinationVertex)
    {
        ResetVertexArrows();

        List<BoardVertex> path = new List<BoardVertex>();

        BoardVertex currVert = destinationVertex;
        while (currVert != startingVertex)
        {
            path.Add(currVert);

            currVert.SetVertexArrowVisualsToPrev();
            activeArrowVertices.Add(currVert);

            currVert = currVert.PrevVertex;
        }

        path.Reverse();

        return path;
    }
}
