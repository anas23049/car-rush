using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class carselecter : MonoBehaviour
{
    public int currentbusindex;
    public GameObject[] buses;
    // Start is called before the first frame update
    void Start()
    {
        currentbusindex = PlayerPrefs.GetInt("Selectedbus", 0);
        foreach (GameObject bus in buses)
        {
            bus.SetActive(false);
        }
        buses[currentbusindex].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
