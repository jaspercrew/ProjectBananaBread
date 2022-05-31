using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class SaveData 
{
    // this is the only place the constructor should ever be called
    private static SaveData instance = new SaveData();
    
    private int playerHealth;
    private string playerScene;
    private Dictionary<string, bool> doors = new Dictionary<string, bool>();

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
            playerHealth = CharController.Instance.CurrentHealth,
            playerScene = SceneManager.GetActiveScene().name,
            doors = new Dictionary<string, bool>(instance.doors)
        };

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

        instance = (SaveData) jsonSerializer.ReadObject(fileStream);
        CharController.Instance.CurrentHealth = instance.playerHealth;
        UIManager.Instance.PopulateHealthBarPublic();

        // AsyncOperation op = SceneManager.LoadSceneAsync(instance.playerScene);
        // // apply save data to player and world here
        // op.completed += (asyncOp) =>
        // {
        //     CharController.Instance.CurrentHealth = instance.playerHealth;
        //     UIManager.Instance.PopulateHealthBarPublic();
        //     // Debug.Log("finished loading new scene");
        // };
    }

    public static void OpenDoor(string door)
    {
        instance.doors[door] = true;
    }

    public static void CloseDoor(string door)
    {
        instance.doors[door] = false;
    }
}