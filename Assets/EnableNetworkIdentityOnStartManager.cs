using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnableNetworkIdentityOnStartManager : MonoBehaviour
{
    [SerializeField] private NetworkIdentity[] objectsToEnable;

    // Start is called before the first frame update
    void Start()
    {
        foreach (NetworkIdentity obj in objectsToEnable)
            obj.gameObject.SetActive(true);
    }
}
