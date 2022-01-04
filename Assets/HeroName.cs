using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroName : MonoBehaviour
{
    public SelectionPlayer player;
    Text heroNameText;

    // Start is called before the first frame update
    void Start()
    {
        heroNameText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetText(string name)
    {
        //if (player.hasAuthority)
        heroNameText.text = name;
    }
}
