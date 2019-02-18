using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    // Class that handles file IO

    public string baseFilePath = ""; // Path that the filename gets appended to

    void Start()
    {
        DontDestroyOnLoad(this.gameObject); // Ensure this instance persists between scenes
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
