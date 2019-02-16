using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public string baseFilePath = "";

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void WriteToFile(string path, string data)
    {
        path = Path.Combine(baseFilePath,path);
        StreamWriter writer = new StreamWriter(path);
        writer.Write(data);
        writer.Close();
    }

    public string ReadFromFile(string path)
    {
        path = Path.Combine(baseFilePath,path);
        StreamReader reader = new StreamReader(path);
        string data = reader.ReadToEnd();
        reader.Close();
        return data;
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

}
