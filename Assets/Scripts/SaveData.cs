using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class SaveData 
{
    // this is the only place the constructor should ever be called
    private static SaveData instance = new SaveData();
    
    //private int playerHealth;
    private string playerScene;
    //private Dictionary<string, bool> levers = new Dictionary<string, bool>();

    private SaveData()
    {
    }

    public static void SaveToFile(int saveNum)
    {
        if (instance == null)
        {
            Debug.LogWarning("SaveData instance was null, this shouldn't happen");
            instance = new SaveData();
        }

        instance = new SaveData
        {
            // update stuff here, then save to object file
            //playerHealth = CharController.Instance.currentHealth,
            playerScene = SceneManager.GetActiveScene().name,
            //levers = GameManager.Instance.LeverDict
        };

        // write to file
        DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(SaveData));
        MemoryStream jsonStream = new MemoryStream();
        jsonSerializer.WriteObject(jsonStream, instance);
        FileStream fileStream = File.Create(Application.persistentDataPath + "/save" + saveNum + ".pbb");
        jsonStream.Seek(0, SeekOrigin.Begin);
        jsonStream.CopyTo(fileStream);
        fileStream.Close();
    }

    public static void LoadFromFile(int saveNum)
    {
        DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(SaveData));
        FileStream fileStream;
        try
        {
            fileStream = File.OpenRead(Application.persistentDataPath + "/save" + saveNum + ".pbb");
        }
        catch (IOException ioe)
        {
            Debug.LogError("IO ERROR WHILE LOADING FILE " + saveNum + ": " + ioe.Message);
            return;
        }

        try
        {
            instance = (SaveData) jsonSerializer.ReadObject(fileStream);
        }
        catch (SerializationException)
        {
            Debug.LogError("something is wrong with the save file, ignoring it");
            return;
        }
        
        //CharController.Instance.currentHealth = instance.playerHealth;
        //UIManager.Instance.PopulateHealthBarPublic();
        //GameManager.Instance.LeverDict = instance.levers;
        GameManager.Instance.isReady = true;
        Debug.Log("scene in save file: " + instance.playerScene + " (not being loaded)");
        
        // SceneManager.LoadSceneAsync(instance.playerScene); // TODO ??

        // AsyncOperation op = SceneManager.LoadSceneAsync(instance.playerScene);
        // // apply save data to player and world here
        // op.completed += (asyncOp) =>
        // {
        //     CharController.Instance.CurrentHealth = instance.playerHealth;
        //     UIManager.Instance.PopulateHealthBarPublic();
        //     // Debug.Log("finished loading new scene");
        // };
    }
}