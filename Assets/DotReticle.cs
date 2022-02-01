using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotReticle : MonoBehaviour
{
    private BoardVertex vertex;
    public BoardVertex Vertex => vertex;

    [SerializeField] private SpriteRenderer sprRend;
    public SpriteRenderer SprRend => sprRend;

    public void SetVertex(BoardVertex vert)
    {
        vertex = vert;
        transform.position = vert.transform.position;
    }

    public void SetColor(Color newCol)
    {
        sprRend.color = newCol;
    }
}
