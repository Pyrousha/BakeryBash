using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PnPMode : MonoBehaviour
{
    public static PnPMode Instance;

    private bool isPnpMode;
    public bool IsPnpMode => isPnpMode;

    [SerializeField] private GameObject networkIdentityEnabler;
    private void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            Debug.LogError("Multiple PNPModeControllers Found");


        //Check if game is in PNP mode or not
        isPnpMode = HeroSelectController.Instance.PNPMode;

        //If game is not in PNP mode, get rid of the networkidentity overrider
        if (isPnpMode == false)
        { 
            Destroy(networkIdentityEnabler);
        }
    }
}
