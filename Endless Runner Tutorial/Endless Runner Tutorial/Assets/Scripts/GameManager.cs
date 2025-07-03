using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[Serializable]
public class PlayerData
{
    public int[] characterCost;
}

public class GameManager : MonoBehaviour 
{
    public static GameManager gm;
    public int[] characterCost;
    public int characterIndex;
    private string filePath;
    

    private void Awake()
    {
        if(gm == null)
        {
            gm = this;
            DontDestroyOnLoad(gameObject);
            filePath = Application.persistentDataPath + "/save.sav";
            
            if (File.Exists(filePath))
            {
                Load();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(filePath);

        PlayerData data = new PlayerData
        {
            characterCost = new int[characterCost.Length]
        };

        for (int i = 0; i < characterCost.Length; i++)
        {
            data.characterCost[i] = characterCost[i];
        }

        bf.Serialize(file, data);
        file.Close();
    }

    void Load()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(filePath, FileMode.Open);

        PlayerData data = (PlayerData)bf.Deserialize(file);
        file.Close();

        for (int i = 0; i < data.characterCost.Length; i++)
        {
            characterCost[i] = data.characterCost[i];
        }
    }

    public void StartRun(int charIndex)
    {
        characterIndex = charIndex;
        SceneManager.LoadScene("Game");
    }

    public void EndRun()
    {
        SceneManager.LoadScene("Menu");
    }





 public void CallMenu()
	{
		GameManager.gm.EndRun();
	}

    
}