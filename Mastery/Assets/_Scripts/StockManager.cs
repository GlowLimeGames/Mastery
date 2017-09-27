using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockManager : MonoBehaviour {

    public delegate void FatalEvent();
    public static event FatalEvent onFatal;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width / 2 - 50, 5, 100, 30), "Click")){
            if (onFatal != null) {
                onFatal();
            }
        }
    }
}
