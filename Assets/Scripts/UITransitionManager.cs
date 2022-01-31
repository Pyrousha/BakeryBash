using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UITransitionManager : MonoBehaviour
{
    CanvasGroup cGroup;

    [System.Serializable]
    private class TransitionInstance {
        
        [SerializeField] public string name;

        [Space(10)]

        [SerializeField] public bool includePosition;
        [SerializeField] public Vector3 positionEnd;
        [SerializeField] public float positionTime;

        [Space(10)]

        [SerializeField] public bool includeScale;
        [SerializeField] public float scaleEnd;
        [SerializeField] public float scaleTime;

        [Space(10)]

        [SerializeField] public bool includeAlpha;
        [SerializeField] public float alphaEnd;
        [SerializeField] public float alphaTime;
    }

    [SerializeField] private TransitionInstance[] _transitionList;

    // Start is called before the first frame update
    void Start()
    {
        cGroup = GetComponent<CanvasGroup>();
    }

    public void TriggerTransition(string TransitionName) {
        TransitionInstance transition = null;
        foreach(TransitionInstance i in _transitionList) {
            if (i.name == TransitionName) {
                transition = i;
            }
        }
        if (transition == null) {
            return;
        }
        if (transition.includePosition) {
            transform.DOLocalMove(transition.positionEnd, transition.positionTime);
        }
        if (transition.includeScale) {
            transform.DOScale(transition.scaleEnd, transition.scaleTime);
        }
        if (transition.includeAlpha) {
            cGroup.DOFade(transition.alphaEnd, transition.alphaTime);
        }   
    }
}
