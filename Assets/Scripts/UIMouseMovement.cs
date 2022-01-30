using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMouseMovement : MonoBehaviour
{
    public float multiplier;
    
    private Vector3 initialPosition;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePosClamped = new Vector3(Mathf.Clamp(Input.mousePosition.x, 0, Screen.width), Mathf.Clamp(Input.mousePosition.y, 0, Screen.height), Input.mousePosition.z);
        Vector3 mousePosCentered = new Vector3(mousePosClamped.x - (Screen.width/2), mousePosClamped.y - (Screen.height/2), mousePosClamped.z);
        transform.localPosition = initialPosition - (mousePosCentered * multiplier);
    }
}
