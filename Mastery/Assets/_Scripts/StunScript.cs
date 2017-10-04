using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StunScript : MonoBehaviour
{

    //icon will appear on stun event
    public GameObject stunIcon;

    private void OnEnable()
    {
        EventManager.onStun += stunPlayer;
        EventManager.offStun += unstunPlayer;
    }
    private void OnDisable()
    {
        EventManager.onStun -= stunPlayer;
        EventManager.offStun -= unstunPlayer;
    }
    void stunPlayer()
    {
        stunIcon.SetActive(true);
    }
    void unstunPlayer()
    {
        stunIcon.SetActive(false);
    }
}

