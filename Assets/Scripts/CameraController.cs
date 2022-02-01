using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameBoardManager board;

    [SerializeField] Vector3 editorStartPos;
    [SerializeField] Vector3 zoomOutPos;
    [SerializeField] private float cameraMoveSpeed;

    private GameBoardManager.GameBoardStateEnum boardState;
    private bool spacePressed;
    private bool keyPressed;

    // Time to hardcode input keys like an actual code wizard

    void Start() {
        boardState = board.GetGameBoardStateEnum;
        if(boardState == GameBoardManager.GameBoardStateEnum.mapEditor) {
            transform.position = editorStartPos;
        }
        spacePressed = false;
        keyPressed = false;
    }

    // Update is called once per frame
    void Update()
    {
        keyPressed = false;
        if(boardState == GameBoardManager.GameBoardStateEnum.mapEditor || !InputManager.Instance.IsInMouseMode()) {
            if(Input.GetKey(KeyCode.W)) {
                keyPressed = true;
                transform.position += new Vector3(0, cameraMoveSpeed, 0);
            }
            if(Input.GetKey(KeyCode.S)) {
                keyPressed = true;
                transform.position += new Vector3(0, -cameraMoveSpeed, 0);
            }
            if(Input.GetKey(KeyCode.D)) {
                keyPressed = true;
                transform.position += new Vector3(cameraMoveSpeed, 0, 0);
            }
            if(Input.GetKey(KeyCode.A)) {
                keyPressed = true;
                transform.position += new Vector3(-cameraMoveSpeed, 0, 0);
            }
        }
        if(Input.GetKeyDown(KeyCode.Space)) {
            transform.position = zoomOutPos;
            spacePressed = true;
        }
        if (spacePressed && keyPressed) {
            transform.position = Vector3.zero;
            spacePressed = false;
        }
    }

    public void OnVertexClicked(BoardVertex vertex) {
        if(InputManager.Instance.IsInMouseMode()) {
            Vector3 vertexPos = vertex.gameObject.transform.position;
            transform.position = new Vector3(vertexPos.x, vertexPos.y, vertexPos.z);
        }
    }
}
