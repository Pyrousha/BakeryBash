using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableNetworkIdentityOnStartManager : MonoBehaviour
{
    [SerializeField] private GameObject[] objectsToEnable;

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject obj in objectsToEnable)
            obj.SetActive(true);
    }
}
