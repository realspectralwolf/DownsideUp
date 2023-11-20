using UnityEngine;
using System.IO;

public static class FileWriter
{
    public static void WriteCurrentLevelToFile(int content)
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "progress.txt");
        // Use StreamWriter to write content to the file
        using (StreamWriter writer = new StreamWriter(path))
        {
            try
            {
                writer.Write(content);
            }
            catch { }
        }
    }

    public static int ReadCurrentLevelFromFile()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "progress.txt");
        string content;

        if (!File.Exists(path))
        {
            return 1;
        }

        using (StreamReader reader = new StreamReader(path))
        {
            content = reader.ReadToEnd();
        }

        if (int.TryParse(content, out int num))
        {
            return num;
        }
        else
        {
            return 1;
        }
    }
}