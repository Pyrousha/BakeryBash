using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GraphHelper
{
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
        return vertices;
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

        return bigBFS;
    }
}
