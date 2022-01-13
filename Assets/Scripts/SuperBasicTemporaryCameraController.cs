using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperBasicTemporaryCameraController : MonoBehaviour
{
    // Time to hardcode input keys like an actual code wizard

    // Update is called once per frame
    void Update()
    {
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
    }
}
