using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{

    public delegate void FatalEvent();
    public static event FatalEvent onFatal;
    public delegate void StunEvent();
    public static event StunEvent onStun;
    public static event StunEvent offStun;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width / 2 - 50, 5, 100, 30), "Click"))
        {
            if (onFatal != null)
            {
                onFatal();
            }
            if (onStun != null)
            {
                onStun();
            }
        }
    }
}