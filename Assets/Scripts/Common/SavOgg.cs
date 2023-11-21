using System.IO;
using UnityEngine;

public static class SavOgg
{
    const int HEADER_SIZE = 27;
    
    public static void Save(string filename, AudioClip clip)
    {
        var filepath = Path.Combine(Application.dataPath + "/Save", filename);
        
        if (File.Exists(filepath))
            File.Delete(filepath);
        
        // Make sure directory exists if user is saving to sub dir.
        Directory.CreateDirectory(Path.GetDirectoryName(filepath));
        
        using var fileStream = CreateEmpty(filepath);
    }
    
    private static FileStream CreateEmpty(string filepath)
    {
        var fileStream = new FileStream(filepath, FileMode.Create);
        const byte emptyByte = new byte();

        for (var i = 0; i < HEADER_SIZE; i++) //preparing the header
        {
            fileStream.WriteByte(emptyByte);
        }

        return fileStream;
    }
}