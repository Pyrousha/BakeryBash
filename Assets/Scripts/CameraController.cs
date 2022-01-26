using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameBoardManager board;

    [SerializeField] Vector3 editorStartPos;
    [SerializeField] Vector3 zoomOutPos;
    private GameBoardManager.GameBoardStateEnum boardState;

    // Time to hardcode input keys like an actual code wizard

    void Start() {
        boardState = board.GetGameBoardStateEnum;
        if(boardState == GameBoardManager.GameBoardStateEnum.mapEditor) {
            transform.position = editorStartPos;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(boardState == GameBoardManager.GameBoardStateEnum.mapEditor) {
            if(Input.GetKey(KeyCode.W)) {
                transform.position += new Vector3(0, 0.1f, 0);
            }
            if(Input.GetKey(KeyCode.S)) {
                transform.position += new Vector3(0, -0.1f, 0);
            }
            if(Input.GetKey(KeyCode.D)) {
                transform.position += new Vector3(0.1f, 0, 0);
            }
            if(Input.GetKey(KeyCode.A)) {
                transform.position += new Vector3(-0.1f, 0, 0);
            }
        } else {
            if(Input.GetKeyDown(KeyCode.Space)) {
                transform.position = zoomOutPos;
            }
        }
    }

    public void OnVertexClicked(BoardVertex vertex) {
        Debug.Log("Yo");
        Vector3 vertexPos = vertex.gameObject.transform.position;
        transform.position = new Vector3(vertexPos.x, vertexPos.y, vertexPos.z);
    }
}
