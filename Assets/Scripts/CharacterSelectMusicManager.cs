using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectMusicManager : MonoBehaviour
{
    FMOD.Studio.EventInstance musicInstance;

    // Start is called before the first frame update
    void Start()
    {
        musicInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Character Select");
        musicInstance.start();
    }

    public void SetCharacterNumber(int numChars) {
        switch(numChars) {
            case 0:
                musicInstance.setParameterByName("Character1", 0);
                musicInstance.setParameterByName("Character2", 0);
                musicInstance.setParameterByName("Character3", 0);
                break;
            case 1:
                musicInstance.setParameterByName("Character1", 1);
                musicInstance.setParameterByName("Character2", 0);
                musicInstance.setParameterByName("Character3", 0);
                break;
            case 2:
                musicInstance.setParameterByName("Character1", 1);
                musicInstance.setParameterByName("Character2", 1);
                musicInstance.setParameterByName("Character3", 0);
                break;
            case 3:
                musicInstance.setParameterByName("Character1", 1);
                musicInstance.setParameterByName("Character2", 1);
                musicInstance.setParameterByName("Character3", 1);
                break;
        }
    }

    void OnDestroy()
    {
        musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicInstance.release();
    }
}
