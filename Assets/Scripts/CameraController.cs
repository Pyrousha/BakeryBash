using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameBoardManager board;

    [SerializeField] Vector3 editorStartPos;
    [SerializeField] Vector3 zoomOutPos;

    [Header("Settings")]
    [SerializeField] private float cameraMoveSpeed;
    [SerializeField] private bool moveToVertexWhenClicked;

    private GameBoardManager.GameBoardStateEnum boardState;
    private bool spacePressed;
    private bool keyPressed;

    [SerializeField] private bool lockMouse;

    // Time to hardcode input keys like an actual code wizard

    void Start()
    {
        boardState = board.GetGameBoardStateEnum;
        if (boardState == GameBoardManager.GameBoardStateEnum.mapEditor)
        {
            transform.position = editorStartPos;
        }
        spacePressed = false;
        keyPressed = false;

        if(lockMouse)
            Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        keyPressed = false;
        if (boardState == GameBoardManager.GameBoardStateEnum.mapEditor || !InputManager.Instance.IsInMouseMode())
        {
            if (Input.GetKey(KeyCode.W))
            {
                keyPressed = true;
                transform.position += new Vector3(0, cameraMoveSpeed, 0);
            }
            if (Input.GetKey(KeyCode.S))
            {
                keyPressed = true;
                transform.position += new Vector3(0, -cameraMoveSpeed, 0);
            }
            if (Input.GetKey(KeyCode.D))
            {
                keyPressed = true;
                transform.position += new Vector3(cameraMoveSpeed, 0, 0);
            }
            if (Input.GetKey(KeyCode.A))
            {
                keyPressed = true;
                transform.position += new Vector3(-cameraMoveSpeed, 0, 0);
            }
        }
        if (InputManager.Instance.IsInMouseMode())
        {
            Vector3 moveDir = new Vector3(0, 0, 0);
            Vector3 mousePos = Input.mousePosition;

            if (mousePos.x <= 5)
                moveDir += new Vector3(-1, 0, 0);
            if (mousePos.x >= Screen.width - 5)
                moveDir += new Vector3(1, 0, 0);

            if (mousePos.y <= 5)
                moveDir += new Vector3(0, -1, 0);
            if (mousePos.y >= Screen.height - 5)
                moveDir += new Vector3(0, 1, 0);

            moveDir = moveDir.normalized * cameraMoveSpeed * Time.deltaTime;

            transform.position += moveDir;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.position = zoomOutPos;
            spacePressed = true;
        }
        if (spacePressed && keyPressed)
        {
            transform.position = Vector3.zero;
            spacePressed = false;
        }
    }

    public void OnVertexClicked(BoardVertex vertex)
    {
        if (InputManager.Instance.IsInMouseMode() && moveToVertexWhenClicked)
        {
            Vector3 vertexPos = vertex.gameObject.transform.position;
            transform.position = new Vector3(vertexPos.x, vertexPos.y, vertexPos.z);
        }
    }

    public void MoveToHero(CombatHero hero)
    {
        transform.position = hero.transform.position;
    }
}
