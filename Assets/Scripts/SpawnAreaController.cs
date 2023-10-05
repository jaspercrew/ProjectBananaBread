using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnAreaController : MonoBehaviour
{
    public Transform spawnLocation;

    //public bool useCamera;

    // Start is called before the first frame update
    private void Start()
    {
        //GetComponentInChildren<CinemachineVirtualCamera>().enabled = useCamera;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CharController.instance.currentArea = this;
            try
            {
                var numVal = int.Parse(spawnLocation.name);
                if (
                    GameManager.instance.levelProgress[SceneManager.GetActiveScene().buildIndex]
                    < numVal
                )
                    GameManager.instance.levelProgress[SceneManager.GetActiveScene().buildIndex] =
                        numVal;
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
            }

            SaveData.SaveToFile(1);
        }
    }
}