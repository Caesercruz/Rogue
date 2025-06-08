using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class WarningData
{
    public List<string> shownWarnings = new();
}

public class DataManager : MonoBehaviour
{
    private WarningData data;
    private string path;

    void Awake()
    {
        path = Application.persistentDataPath + "/Info.json";
        LoadData();
    }

    void LoadData()
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<WarningData>(json);
        }
        else data = new WarningData();
    }

    void SaveData()
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);
    }

    public void ShowWarning(string id, string message)
    {
        if (data.shownWarnings.Contains(id)) return;

        data.shownWarnings.Add(id);
        SaveData();
        Debug.Log($"[AVISO] {message}");
    }

    public void ClearAllWarnings()
    {
        File.Delete(path);
        Debug.Log("Todos os avisos foram resetados.");
    }
}
