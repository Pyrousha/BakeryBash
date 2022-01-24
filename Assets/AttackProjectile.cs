using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackProjectile : MonoBehaviour
{
    [SerializeField] private MeshRenderer rend;
    [SerializeField] private GameObject particleChild;

    Vector3 startingPosition;
    Vector3 targetPosition;

    float speed = 3f;
    float timer = 0f;
    bool moving = false;

    public void SetPositions(Vector3 startPos, Vector3 endPos)
    {
        transform.position = startPos;

        startingPosition = startPos;
        targetPosition = endPos;

        StartMoving();
    }

    public void StartMoving()
    {
        transform.forward = (targetPosition - startingPosition).normalized;

        rend.enabled = true;
        particleChild.SetActive(true);
        moving = true;
    }

    private void Update()
    {
        if (!moving)
            return;

        timer += Time.deltaTime * speed;

        if(timer >= 1f)
        {
            //end of path
            Destroy(gameObject);
        }
        else
        {
            transform.position = Vector3.Lerp(startingPosition, targetPosition, timer);
        }
    }
}
