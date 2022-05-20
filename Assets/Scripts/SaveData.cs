using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

[Serializable]
public class SaveData
{
    private int playerHealth;
    private string playerScene;
    // dictionary for doors
    
    // map prev scene exits to current scene spawns in EntranceManager or something

    private void SaveToFile(string file)
    {
        // update stuff here, then save to object file
        playerHealth = CharController.Instance.CurrentHealth;
        
        DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(SaveData));
        MemoryStream jsonStream = new MemoryStream();
        jsonSerializer.WriteObject(jsonStream, this);
    }
}