using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StockLives : MonoBehaviour {

    //create an array of gameobjects
    //make them disappear on event
    public GameObject[] stocks;
    private int size;
	// Use this for initialization
	void Start () {
        size = stocks.Length;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    private void OnEnable()
    {
        StockManager.onFatal += RemoveStock;
    }
    private void OnDisable()
    {
        StockManager.onFatal -= RemoveStock;
    }
    void RemoveStock() {
        if (size > 1)
        {
            stocks[size - 1].SetActive(false);
            GameObject[] temp = new GameObject[size-1];
            for (int i = 0; i < size - 1; i++) {
                temp[i] = stocks[i];
            }
            stocks = temp;
            size--;
        }
        else {
            //game over
            Debug.Log("game ended");
            SceneManager.LoadScene(2);
        }
    }
}
