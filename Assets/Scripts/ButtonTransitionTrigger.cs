using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTransitionTrigger : MonoBehaviour
{
    [System.Serializable]
    private class TransitionReference {
        [SerializeField] public UITransitionManager transitionManager;
        [SerializeField] public string name;
    }

    [SerializeField] private TransitionReference[] _referenceList;

    public void ButtonPressed () {
        foreach(TransitionReference i in _referenceList) {
            i.transitionManager.TriggerTransition(i.name);
        }
    }
}
