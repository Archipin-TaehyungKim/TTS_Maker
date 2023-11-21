using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class FileIOUtility
{
    public static void LoadFile(string filePath, Action<List<DataStruct>> data)
    {
        var result = new List<DataStruct>();
        var lines = File.ReadAllLines(filePath);

        foreach (var strLineValue in lines)
        {
            var lineNumber = strLineValue.Split('\t')[0];
            var fileName = strLineValue.Split('\t')[1];
            var contents = strLineValue.Split('\t')[2];
            
            result.Add(new DataStruct()
            {
                lineNumber = lineNumber,
                fileName = fileName,
                contents = contents
            });
            
            data(result);
        }
    }
}
