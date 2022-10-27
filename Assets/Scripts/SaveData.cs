using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class SaveData 
{
    // this is the only place the constructor should ever be called
    private static SaveData instance = new SaveData();
    
    //private int playerHealth;
    //private string playerScene;
    public static bool[][] defaultLevelProgress = new bool[][]
    {
        new bool[6], 
        new bool[1],
    };

    public bool[][] levelProgress = defaultLevelProgress;

    public class Settings
    {
        public bool isFullscreen;
        public float volume;
    }

    private static Settings settingsInst = new Settings();
    
    
    //private Dictionary<string, bool> levers = new Dictionary<string, bool>();

    private SaveData()
    {
    }
    
    public static void SaveSettings()
    {

        settingsInst = new Settings
        {
            // update stuff here, then save to object file
            isFullscreen = Screen.fullScreen,
            volume = AudioSlider.instance.slider.value
            
        };

        // write to file
        DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Settings));
        MemoryStream jsonStream = new MemoryStream();
        jsonSerializer.WriteObject(jsonStream, settingsInst);
        FileStream fileStream = File.Create(Application.persistentDataPath + "/settings" + ".set");
        jsonStream.Seek(0, SeekOrigin.Begin);
        jsonStream.CopyTo(fileStream);
        fileStream.Close();
    }
    public static void LoadSettings()
    {
        DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Settings));
        FileStream fileStream;
        try
        {
            fileStream = File.OpenRead(Application.persistentDataPath + "/settings" + ".set");
        }
        catch (IOException ioe)
        {
            Debug.LogError("IO ERROR WHILE LOADING FILE, CREATING DEFAULT SETTINGS OBJECT " + ": " + ioe.Message);
            
            //DEFAULT SETTINGS
            settingsInst = new Settings();
            settingsInst.volume = .5f;
            settingsInst.isFullscreen = false;
            if (GameManager.Instance.isMenu)
            { 
                AudioSlider.instance.slider.value = settingsInst.volume;
            }
            AudioManager.Instance.UpdateVolume(settingsInst.volume);
            Screen.fullScreen = settingsInst.isFullscreen;
            return;
        }

        try
        {
            settingsInst = (Settings) jsonSerializer.ReadObject(fileStream);
        }
        catch (SerializationException)
        {
            Debug.LogError("something is wrong with the settings file, ignoring it");
            //DEFAULT SETTINGS
            return;
        }

        if (GameManager.Instance.isMenu)
        {
            //MonoBehaviour.print(GameObject.Find("RightCanvas").GetComponentInChildren<AudioSlider>().slider);
            AudioSlider.instance.slider.value = settingsInst.volume;
        }
        AudioManager.Instance.UpdateVolume(settingsInst.volume);
        Screen.fullScreen = settingsInst.isFullscreen;
    }
    

    public static void SaveToFile(int saveNum)
    {
        instance = new SaveData
        {
            // update stuff here, then save to object file
            //playerScene = SceneManager.GetActiveScene().name,
            levelProgress = GameManager.Instance.levelProgress,
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
            Debug.LogError("IO ERROR WHILE LOADING SAVEFILE, USING DEFAULT SAVE OBJECT " + saveNum + ": " + ioe.Message);
            //DEFAULT SAVE OBJECT
            instance = new SaveData();
            //instance.playerScene = "MainMenu";
            instance.levelProgress = defaultLevelProgress;
            GameManager.Instance.levelProgress = instance.levelProgress;
            return;
        }

        try
        {
            instance = (SaveData) jsonSerializer.ReadObject(fileStream);
        }
        catch (SerializationException)
        {
            GameManager.Instance.levelProgress = defaultLevelProgress;
            Debug.LogError("something is wrong with the read save file, using default lvl progress");
            return;
        }
        
        if (instance.levelProgress.Length < 1)
        {
            instance.levelProgress = defaultLevelProgress;
            Debug.LogError("save file has null scenes completed, inserted new array");
        }
        
        GameManager.Instance.levelProgress = instance.levelProgress;
    }
}