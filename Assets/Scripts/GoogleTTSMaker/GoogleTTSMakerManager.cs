using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Application = UnityEngine.Application;
using Ookii.Dialogs;
using Screen = UnityEngine.Screen;

public class GoogleTTSMakerManager: MonoBehaviour
{
    public List<DataStruct> data;
    [SerializeField] private Text statusText;
    [SerializeField] private Config _config;
    [SerializeField] private Text version;
    
    private void Start()
    {
        _config.SetConfigure();
        version.text = Application.version;

        statusText.text = "Please load tsv file";
        Screen.SetResolution(800, 600, FullScreenMode.Windowed);
    }

    private string LoadCSV()
    {
        var dialog = new VistaOpenFileDialog();
        
        dialog.Filter = "tsv files (*.tsv) |*.tsv|csv files (*.csv)|*.csv|All files (*.*)|*.*";
        dialog.FilterIndex = 3;
        dialog.Title = "Open file";
        
        if (dialog.ShowDialog() != DialogResult.OK) return "";
        if (dialog.OpenFile() == null) return "";
        FileIOUtility.LoadFile(dialog.FileName, d => { data = d; });
        return dialog.FileName;
    }

    public void OnClick_LoadCSV()
    {
        var url = LoadCSV();
        Debug.Log(url);
        if (url != "")
            statusText.text = $"{url} Loaded!";
    }

    public void OnClick_MakeTTS()
    {
        StartCoroutine(Make());
    }

    private IEnumerator Make()
    {
        for (var i = 0; i < data.Count; i++)
        {
            yield return new WaitForSeconds(1.0f);
            statusText.text = $"'{i + 1}/{data.Count}\n{data[i].fileName}' is processing...";
            yield return StartCoroutine(TTSManager.GoogleTTS(
                data[i].fileName, data[i].contents, _config, log =>
                {
                    statusText.text += log;
                }));
        }
    }
}