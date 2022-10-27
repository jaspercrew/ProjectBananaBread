using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Assertions.Must;
using UnityEngine.SceneManagement;

public class SpawnAreaController : MonoBehaviour
{
    public Transform spawnLocation;
    //public bool useCamera;

    // Start is called before the first frame update
    void Start()
    {
        //GetComponentInChildren<CinemachineVirtualCamera>().enabled = useCamera;

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CharController.Instance.currentArea = this;
            try
            {
                int numVal = Int32.Parse(spawnLocation.name);
                if (GameManager.Instance.levelProgress[SceneManager.GetActiveScene().buildIndex] < numVal)
                {
                    GameManager.Instance.levelProgress[SceneManager.GetActiveScene().buildIndex] = numVal;
                }
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
            }
            SaveData.SaveToFile(1);
        }
    }
}
